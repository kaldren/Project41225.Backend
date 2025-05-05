namespace Project41225.Domain.Entities;
public class AudioChunk : BaseEntity
{
    public Guid SessionId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public byte[] Data { get; set; } = [];
}
