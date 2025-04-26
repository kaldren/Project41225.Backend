namespace Project41225.Domain.Entities;
public class AudioChunk
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public Guid SessionId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public byte[] Data { get; set; } = [];
}
