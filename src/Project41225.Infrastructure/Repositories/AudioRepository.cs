using Project41225.Application.Interfaces;
using Project41225.Domain.Entities;
using Project41225.Infrastructure.Data;

namespace Project41225.Infrastructure.Repositories;

public class AudioRepository : IAudioRepository
{
    private readonly CosmosContext _cosmos;
    public AudioRepository(CosmosContext cosmos) => _cosmos = cosmos;

    public async Task AddAudioChunkAsync(AudioChunk chunk, CancellationToken cancellationToken)
    {
        _cosmos.AudioChunks.Add(chunk);
        await _cosmos.SaveChangesAsync(cancellationToken);
    }
}