using Microsoft.AspNetCore.SignalR;
using Project41225.Application.Interfaces;

namespace Project41225.Infrastructure.Hubs;

public class AudioHub : Hub
{
    private readonly ISpeechTranscriptionService _speechService;

    public AudioHub(ISpeechTranscriptionService speechService)
    {
        _speechService = speechService;
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        // Optionally do per-connection init
    }

    public async Task StartSession(Guid sessionId)
    {
        await _speechService.StartRecognitionAsync(Context.ConnectionId, sessionId);
    }

    public async Task SendAudioChunk(Guid sessionId, byte[] chunk)
    {
        await _speechService.ProcessAudioChunkAsync(Context.ConnectionId, chunk);
    }

    public async Task StopSession()
    {
        await _speechService.StopRecognitionAsync(Context.ConnectionId);
    }
}
