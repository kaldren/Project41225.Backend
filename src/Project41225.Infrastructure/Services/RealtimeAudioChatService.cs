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
    private readonly ConcurrentDictionary<string, WebSocketSession> _sessions = new();

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
            ws.Options.SetRequestHeader("Authorization", $"Bearer {_openAIOptions.Key}");
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
                    turn_detection = new { type = "server_vad" }
                }
            };
            await SendJsonAsync(ws, sessionUpdate);

            var s = new WebSocketSession(ws, connectionId, sessionId);
            _sessions[connectionId] = s;

            _ = ReceiveLoopAsync(s); // fire‐and‐forget
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occured starting a new session: {ex.Message}");
        }
    }

    public async Task ProcessAudioChunkAsync(string connectionId, byte[] pcm16Chunk)
    {
        if (!_sessions.TryGetValue(connectionId, out var s))
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

    public async Task StopSessionAsync(string connectionId)
    {
        if (_sessions.TryRemove(connectionId, out var s))
        {
            // commit buffer → triggers transcription & then chat response
            await SendJsonAsync(s.Socket, new { type = "input_audio_buffer.commit" });
            // close
            await s.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Session ended", CancellationToken.None);
        }
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