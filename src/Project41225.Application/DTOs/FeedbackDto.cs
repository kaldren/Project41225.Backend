namespace Project41225.Application.DTOs;

public class QuestionScoreDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Score { get; set; }
}

public class FeedbackDto
{
    public Guid SessionId { get; set; }
    public int Clarity { get; set; }
    public int Content { get; set; }
    public int Pace { get; set; }
    public List<QuestionScoreDto> Questions { get; set; } = [];
}
