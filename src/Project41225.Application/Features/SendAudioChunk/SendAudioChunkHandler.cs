using MediatR;
using Project41225.Application.Interfaces;
using Project41225.Domain.Entities;

namespace Project41225.Application.Features.SendAudioChunk;

public class SendAudioChunkHandler : IRequestHandler<SendAudioChunkCommand, Unit>
{
    private readonly IAudioRepository _audioRepo;
    public SendAudioChunkHandler(IAudioRepository audioRepo) => _audioRepo = audioRepo;

    public async Task<Unit> Handle(SendAudioChunkCommand request, CancellationToken cancellationToken)
    {
        var chunk = new AudioChunk
        {
            SessionId = request.SessionId,
            Data = request.Data
        };
        await _audioRepo.AddAudioChunkAsync(chunk, cancellationToken);
        return Unit.Value;
    }
}