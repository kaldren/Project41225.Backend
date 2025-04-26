using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Project41225.Application.Interfaces;
using Project41225.Domain.Entities;

namespace Project41225.Infrastructure.Services;

public class InterviewSessionRepository : IInterviewSessionRepository
{
    private readonly Container _container;
    private readonly IConfiguration _configuration;

    public InterviewSessionRepository(CosmosClient cosmosClient, IConfiguration configuration)
    {
        _configuration = configuration;
        _container = cosmosClient.GetContainer(_configuration["CosmosDB:DatabaseName"], _configuration["CosmosDB:InterviewsContainer"]);
    }

    public async Task SaveAsync(InterviewSession session)
    {
        try
        {
            await _container.CreateItemAsync(session, new PartitionKey(session.UserId));
        }
        catch (CosmosException ex)
        {
            Console.WriteLine($"Cosmos error: {ex.StatusCode} - {ex.Message}");
            Console.WriteLine($"ActivityId: {ex.ActivityId}");
            throw;
        }
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
