namespace Project41225.Domain.Entities;
public class QuestionAnswer
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Question { get; set; }
    public string Answer { get; set; }
}