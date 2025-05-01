namespace Project41225.Application.Interfaces;

public interface IRealtimeAudioChatService
{
    Task StartSessionAsync(string connectionId, Guid sessionId);
    Task ProcessAudioChunkAsync(Guid sessionId, byte[] pcm16Chunk);
    Task StopSessionAsync(string connectionId);
}
