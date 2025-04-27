namespace Project41225.Application.Interfaces;

public interface IAzureOpenAIProvider
{
    Task<string> GetChatCompletionAsync(string role, string sessionId);
}
