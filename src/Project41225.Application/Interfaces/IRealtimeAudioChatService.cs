namespace Project41225.Application.Interfaces;

public interface IRealtimeAudioChatService
{
    Task StartSessionAsync(string connectionId, Guid sessionId);
    Task ProcessAudioChunkAsync(string connectionId, byte[] pcm16Chunk);
    Task StopSessionAsync(string connectionId);
}
