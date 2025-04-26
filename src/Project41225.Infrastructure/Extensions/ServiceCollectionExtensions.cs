using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Project41225.Application.Interfaces;
using Project41225.Infrastructure.Services;
namespace Project41225.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(s =>
        {
            var connectionString = configuration["CosmosDB:ConnectionString"];
            return new CosmosClient(connectionString);
        });

        services.AddScoped<IInterviewSessionRepository, InterviewSessionRepository>();

        return services;
    }
}
