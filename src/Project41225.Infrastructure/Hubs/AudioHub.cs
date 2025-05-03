using Microsoft.AspNetCore.SignalR;
using Project41225.Application.Interfaces;
using System;

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
        try
        {
            await _chatService.ProcessAudioChunkAsync(sessionId, chunk);
        }
        catch (Exception)
        {
            await _chatService.StopSessionByConnectionAsync(Context.ConnectionId);
            throw;
        }

        
    }

    public async Task StopSession(Guid sessionId)
    {
        try
        {
            await _chatService.StopSessionAsync(sessionId);
        }
        catch (Exception)
        {
            await _chatService.StopSessionByConnectionAsync(Context.ConnectionId);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        // Tell your service to stop and remove the session
        await _chatService.StopSessionByConnectionAsync(Context.ConnectionId);
        await base.OnDisconnectedAsync(ex);
    }
}
