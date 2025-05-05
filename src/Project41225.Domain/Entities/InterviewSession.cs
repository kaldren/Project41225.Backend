using Newtonsoft.Json;

namespace Project41225.Domain.Entities;
public class InterviewSession : BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
