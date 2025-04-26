using Newtonsoft.Json;

namespace Project41225.Domain.Entities;
public class InterviewSession
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
