using Microsoft.Azure.Cosmos;
using Project41225.Application.Interfaces;
using Project41225.Domain.Entities;

namespace Project41225.Infrastructure.Services;

public class InterviewSessionRepository : IInterviewSessionRepository
{
    private readonly Container _container;

    public InterviewSessionRepository(CosmosClient cosmosClient)
    {
        _container = cosmosClient.GetContainer("InterviewDB", "Interviews");
    }

    public async Task SaveAsync(InterviewSession session)
    {
        await _container.UpsertItemAsync(session, new PartitionKey(session.UserId));
    }

    public async Task<InterviewSession?> GetByIdAsync(string id, string userId)
    {
        try
        {
            var response = await _container.ReadItemAsync<InterviewSession>(id, new PartitionKey(userId));
            return response.Resource;
        }
        catch (CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
