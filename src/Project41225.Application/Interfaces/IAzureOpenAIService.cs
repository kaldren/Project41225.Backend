namespace Project41225.Application.Interfaces;

public interface IAzureOpenAIService
{
    Task<string> GetChatCompletionAsync(string role, string sessionId);
}
