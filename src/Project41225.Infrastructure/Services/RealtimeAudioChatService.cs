using Microsoft.AspNetCore.SignalR;
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
                        voice_activity_parameters = new
                        {
                            // how loud must speech be to count as “voice”? 
                            // default is 0.5; raise to 0.7 for stricter detection.
                            threshold = 0.5,
                            // how many ms of silence before “turn.end” fires?
                            silence_timeout_ms = 300
                        }
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

        // append audio buffer
        var msg = new { type = "input_audio_buffer.append" };
        var header = JsonSerializer.Serialize(msg);
        // send header
        await s.Socket.SendAsync(
            new ArraySegment<byte>(Encoding.UTF8.GetBytes(header)),
            WebSocketMessageType.Text, true, CancellationToken.None);
        // then send raw PCM16 binary
        await s.Socket.SendAsync(
            new ArraySegment<byte>(pcm16Chunk),
            WebSocketMessageType.Binary, true, CancellationToken.None);
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
            if (result.MessageType == WebSocketMessageType.Close)
                break;

            ms.Write(buffer, 0, result.Count);
            if (result.EndOfMessage)
            {
                var raw = ms.ToArray();
                ms.SetLength(0);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var doc = JsonDocument.Parse(raw);
                    var type = doc.RootElement.GetProperty("type").GetString();
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
                            await SendJsonAsync(s.Socket, new { type = "response.create" });
                            break;

                        case "conversation.item.audio_transcription.completed":
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
                        case "response.text.delta":
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
        }
    }

    private Task SendJsonAsync(ClientWebSocket ws, object obj)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj));
        return ws.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private record WebSocketSession(ClientWebSocket Socket, string ConnectionId, Guid SessionId);
}