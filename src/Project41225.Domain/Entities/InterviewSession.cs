using Newtonsoft.Json;

namespace Project41225.Domain.Entities;
public class InterviewSession : BaseEntity<Guid>
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
