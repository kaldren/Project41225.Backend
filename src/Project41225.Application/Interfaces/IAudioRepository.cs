using Project41225.Domain.Entities;

namespace Project41225.Application.Interfaces;

public interface IAudioRepository
{
    Task AddAudioChunkAsync(AudioChunk chunk, CancellationToken cancellationToken);
}
