using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using Project41225.Application.Configuration;
using Project41225.Application.Interfaces;
using System.Text;

namespace Project41225.Infrastructure.Services;

public class AzureOpenAIService : IAzureOpenAIService
{
    private readonly IOptions<AzureOpenAISettings> _azureOpenAISettings;
    private readonly AzureOpenAIClient _azureClient;
    private readonly ChatClient _chatClient;

    public AzureOpenAIService(IOptions<AzureOpenAISettings> azureOpenAISettings)
    {
        _azureOpenAISettings = azureOpenAISettings;
        _azureClient = new(
            new Uri(_azureOpenAISettings.Value.Endpoint),
            new AzureKeyCredential(_azureOpenAISettings.Value.Key));

        _chatClient = _azureClient.GetChatClient(_azureOpenAISettings.Value.DeploymentName);
    }

    public async Task<string> GetChatCompletionAsync(string prompt, string sessionId)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(@"
        You are a professional interviewer helping the user prepare for a job interview.
        Your role is to simulate a realistic interview experience.

        - Ask at least 30 thoughtful, challenging, and role-specific interview questions.
        - If the conversation calls for it, ask follow-up questions or go beyond 30 to maintain depth.
        - Use a natural conversational tone, but stay professional.
        - Adapt your questions based on the candidate's responses and the role they are applying for.
        - You may occasionally provide small hints or ask the candidate to elaborate, but do not break character as the interviewer.
        "),
            new UserChatMessage("Hi, can you help me prepare for an interview?"),
            new AssistantChatMessage("Of course. I'm here to simulate a full interview session. What role are you applying for?"),
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
