namespace Project41225.Domain.Entities;

public class SessionFeedback : BaseEntity
{
    public Guid SessionId { get; set; }
    public int Clarity { get; set; }
    public int Content { get; set; }
    public int Pace { get; set; }
    public List<QuestionScore> Questions { get; set; } = new();
}
