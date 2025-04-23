using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using Project41225.Application.Configuration;
using Project41225.Application.Interfaces;
using System.ClientModel;
using System.Text;

namespace Project41225.Infrastructure.Services;

public class AzureOpenAIService : IAzureOpenAIService
{
    private readonly IOptions<AzureOpenAISettings> _settings;
    private readonly AzureOpenAIClient _azureClient;
    private readonly ChatClient _chatClient;

    public AzureOpenAIService(IOptions<AzureOpenAISettings> settings)
    {
        _settings = settings;
        _azureClient = new(
            new Uri(_settings.Value.Endpoint),
            new DefaultAzureCredential());

        _chatClient = _azureClient.GetChatClient(_settings.Value.DeploymentName);
    }

    public async Task<string> GetChatCompletionAsync(string prompt)
    {
        var messages = new List<ChatMessage>
       {
           new SystemChatMessage("You are a helpful assistant that talks like a pirate."),
           new UserChatMessage("Hi, can you help me?"),
           new AssistantChatMessage("Arrr! Of course, me hearty! What can I do for ye?"),
           new UserChatMessage(prompt)
       };

        var completionUpdates = _chatClient.CompleteChatStreaming(messages);

        var result = new StringBuilder();

        foreach (var completionUpdate in completionUpdates)
        {
            foreach (var contentPart in completionUpdate.ContentUpdate)
            {
                result.Append(contentPart.Text);
            }
        }

        return await Task.FromResult(result.ToString());
    }
}
