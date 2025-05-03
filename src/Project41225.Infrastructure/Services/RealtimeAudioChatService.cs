using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Cosmos.Core;
using Microsoft.Extensions.Options;
using Project41225.Application.Common.Settings;
using Project41225.Application.Interfaces;
using Project41225.Infrastructure.Hubs;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Project41225.Infrastructure.Services;

public class RealtimeAudioChatService : IRealtimeAudioChatService
{
    private readonly AzureOpenAISettings _openAIOptions;
    private readonly IHubContext<AudioHub> _hubContext;
    private readonly ConcurrentDictionary<Guid, WebSocketSession> _sessions = new();

    public RealtimeAudioChatService(IOptions<AzureOpenAISettings> options, IHubContext<AudioHub> hubContext)
    {
        _openAIOptions = options.Value;
        _hubContext = hubContext;
    }

    public async Task StartSessionAsync(string connectionId, Guid sessionId)
    {
        try
        {
            var ws = new ClientWebSocket();
            // 1️ Auth
            ws.Options.SetRequestHeader("Authorization", $"Bearer {_openAIOptions.Key}");
            // 2️ Tell OpenAI you’re opting into the realtime beta
            ws.Options.SetRequestHeader("openai-beta", "realtime=v1");
            // connect to Realtime API
            var uri = new Uri($"{_openAIOptions.RealtimeApi.WebSocketEndpoint}/realtime?model={_openAIOptions.RealtimeApi.ModelDeploymentName}&session_id={sessionId}");
            await ws.ConnectAsync(uri, CancellationToken.None);

            // send session.update to configure transcription & VAD
            var sessionUpdate = new
            {
                type = "session.update",
                session = new
                {
                    input_audio_format = _openAIOptions.RealtimeApi.AudioFormat,
                    input_audio_transcription = new { model = _openAIOptions.RealtimeApi.AudioTranscriptionModel },
                    turn_detection = new 
                    { 
                        type = "server_vad",
                        // how loud must speech be?
                        threshold = 0.7,
                        // how many ms of silence to trigger turn.end?
                        silence_duration_ms = 700,
                        // (optional) include a little audio before speech
                        prefix_padding_ms = 100,
                        // (optional) auto‐create response on turn.end
                        create_response = true,
                        // (optional) cut off mid‐generation if user speaks
                        interrupt_response = true

                    }
                }
            };
            await SendJsonAsync(ws, sessionUpdate);

            var s = new WebSocketSession(ws, connectionId, sessionId);
            _sessions[sessionId] = s;

            _ = ReceiveLoopAsync(s); // fire‐and‐forget
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occured starting a new session: {ex.Message}");
        }
    }

    public async Task ProcessAudioChunkAsync(Guid sessionId, byte[] pcm16Chunk)
    {
        if (!_sessions.TryGetValue(sessionId, out var s))
            throw new InvalidOperationException("Session not found");

        if (s.Socket.State != WebSocketState.Open)
        {
            Console.WriteLine($"WebSocket not open for session: {sessionId}");
            _sessions.TryRemove(sessionId, out _);
            return;
        }

        try
        {
            // Build a single JSON payload with base64-encoded audio
            var payload = new
            {
                type = "input_audio_buffer.append",
                audio = Convert.ToBase64String(pcm16Chunk)
            };

            await SendJsonAsync(s.Socket, payload);
        }
        catch (WebSocketException)
        {
            // remote party closed it—cleanup and quit silently
            _sessions.TryRemove(sessionId, out _);
        }
        catch (Exception ex)
        {
            // you may log unexpected errors, but don't rethrow
            Console.Error.WriteLine($"Error sending audio chunk: {ex}");
            throw;
        }
    }

    public async Task StopSessionAsync(Guid sessionId)
    {
        if (_sessions.TryRemove(sessionId, out var s))
        {
            // commit buffer → triggers transcription & then chat response
            await SendJsonAsync(s.Socket, new { type = "input_audio_buffer.commit" });
            // close
            await s.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Session ended", CancellationToken.None);
        }
    }

    public async Task StopSessionByConnectionAsync(string connectionId)
    {
        var session = _sessions.First(s => s.Value.ConnectionId == connectionId).Value;
        // commit buffer → triggers transcription & then chat response
        await SendJsonAsync(session.Socket, new { type = "input_audio_buffer.commit" });
        // close
        await session.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Session ended", CancellationToken.None);
    }

    private async Task ReceiveLoopAsync(WebSocketSession s)
    {
        var buffer = new byte[8_192];
        var ms = new MemoryStream();
        while (s.Socket.State == WebSocketState.Open)
        {
            var result = await s.Socket.ReceiveAsync(buffer, CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close) break;

            ms.Write(buffer, 0, result.Count);
            if (!result.EndOfMessage) continue;

            var raw = ms.ToArray();
            ms.SetLength(0);

            if (result.MessageType != WebSocketMessageType.Text) continue;

            using var doc = JsonDocument.Parse(raw);
            var type = doc.RootElement.GetProperty("type").GetString();
            Console.WriteLine($"Event types: {type}");
                    
            switch (type)
            {
                case "session.created":
                    // (you can log or ignore)
                    break;

                case "turn.started":
                    // user began speaking
                    // e.g. clear any buffers or UI state
                    break;

                case "turn.ended":
                    // server-side VAD detected end of user speech
                    // automatically ask for the model’s reply
                    //await SendJsonAsync(s.Socket, new { type = "response.create" });
                    break;
                
                case "conversation.item.input_audio_transcription.failed":
                    {
                        var itemId = doc.RootElement.GetProperty("item_id").GetString();
                        var errorObj = doc.RootElement.GetProperty("error");
                        var errType = errorObj.GetProperty("type").GetString();
                        var errCode = errorObj.GetProperty("code").GetString();
                        var errMsg = errorObj.GetProperty("message").GetString();

                        Console.Error.WriteLine(
                            $"[OpenAI] transcription FAILED for item {itemId}: " +
                            $"{errType}/{errCode}: {errMsg}"
                        );
                        // push to client if you like:
                        await _hubContext.Clients.Client(s.ConnectionId)
                                .SendAsync("ReceiveTranscriptionError", s.SessionId, errMsg);
                        break;
                    }
                case "response.audio_transcript.delta":
                    {
                        var partial = doc.RootElement
                                         .GetProperty("data")
                                         .GetProperty("transcript")
                                         .GetString();
                        await _hubContext.Clients
                            .Client(s.ConnectionId)
                            .SendAsync("ReceivePartialTranscript", s.SessionId, partial);
                    }
                    break;
                case "response.audio_transcript.done":
                    {
                        var text = doc.RootElement
                            .GetProperty("data")
                            .GetProperty("transcript")
                            .GetString();
                        await _hubContext.Clients
                            .Client(s.ConnectionId)
                            .SendAsync("ReceiveFinalTranscript", s.SessionId, text);
                    }
                    break;
                case "response.audio.delta":
                    {
                        var delta = doc.RootElement
                            .GetProperty("data")
                            .GetProperty("content")
                            .GetString();
                        await _hubContext.Clients
                            .Client(s.ConnectionId)
                            .SendAsync("ReceiveChatResponsePart", s.SessionId, delta);
                    }
                    break;
                case "response.text.done":
                    await _hubContext.Clients
                        .Client(s.ConnectionId)
                        .SendAsync("ReceiveChatResponseComplete", s.SessionId);
                    break;
            }
        }
    }

    private Task SendJsonAsync(ClientWebSocket ws, object obj)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj));
        return ws.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private record WebSocketSession(ClientWebSocket Socket, string ConnectionId, Guid SessionId);
}