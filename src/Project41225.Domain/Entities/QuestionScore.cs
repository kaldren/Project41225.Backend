namespace Project41225.Domain.Entities;

public class QuestionScore : BaseEntity
{
    public Guid SessionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Score { get; set; }
}
