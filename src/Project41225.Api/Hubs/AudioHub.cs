using MediatR;
using Microsoft.AspNetCore.SignalR;
using Project41225.Application.Features.SendAudioChunk;

namespace Project41225.Api.Hubs;

public class AudioHub : Hub
{
    private readonly IMediator _mediator;
    public AudioHub(IMediator mediator) => _mediator = mediator;

    public async Task SendAudioChunk(Guid sessionId, byte[] chunk)
    {
        // Delegate persistence to CQRS command
        await _mediator.Send(new SendAudioChunkCommand(sessionId, chunk));
    }
}
