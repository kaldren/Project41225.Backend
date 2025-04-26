namespace Project41225.Application.Interfaces;

public interface ISpeechTranscriptionService
{
    Task StartRecognitionAsync(string connectionId, Guid sessionId);
    Task ProcessAudioChunkAsync(string connectionId, byte[] audioChunk);
    Task StopRecognitionAsync(string connectionId);
}
