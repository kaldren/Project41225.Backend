using Newtonsoft.Json;

namespace Project41225.Domain.Entities;
public class InterviewSession
{
    [JsonProperty("id")]
    public string Id { get; private set; } = Guid.NewGuid().ToString();
    public DateTimeOffset StartTime { get; private set; } = DateTimeOffset.Now;
    public DateTimeOffset EndTime { get; set; }
    public string UserId { get; set; }
    public string Role { get; set; }
    public List<QuestionAnswer> QuestionsAnswers { get; set; } = new();
}
