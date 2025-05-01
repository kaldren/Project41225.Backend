using Microsoft.AspNetCore.SignalR;
using Project41225.Application.Interfaces;

namespace Project41225.Infrastructure.Hubs;

public class AudioHub : Hub
{
    private readonly IRealtimeAudioChatService _chatService;

    public AudioHub(IRealtimeAudioChatService chatService)
    {
        _chatService = chatService;
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        // Optionally do per-connection init
    }

    public async Task StartSession(Guid sessionId)
    {
        await _chatService.StartSessionAsync(Context.ConnectionId, sessionId);
    }

    public async Task SendAudioChunk(Guid sessionId, byte[] chunk)
    {
        await _chatService.ProcessAudioChunkAsync(sessionId, chunk);
    }

    public async Task StopSession()
    {
        await _chatService.StopSessionAsync(Context.ConnectionId);
    }
}
