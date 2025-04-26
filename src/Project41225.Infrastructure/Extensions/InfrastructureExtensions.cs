using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Project41225.Application.Configuration;
using Project41225.Application.Interfaces;
using Project41225.Infrastructure.Data;
using Project41225.Infrastructure.Repositories;
using Project41225.Infrastructure.Services;

namespace Project41225.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDataCosmosDB(configuration);

        services.AddRepositoriesAndServices(configuration);

        return services;
    }

    private static IServiceCollection AddDataCosmosDB(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CosmosContext>(opts =>
                opts.UseCosmos(
                    configuration["Cosmos:Endpoint"] ?? throw new KeyNotFoundException("Cosmos Endpoint missing"),
                    configuration["Cosmos:AccountKey"] ?? throw new KeyNotFoundException("Cosmos Account Key missing"),
                    databaseName: "InterviewDb"));


        return services;
    }

    private static IServiceCollection AddRepositoriesAndServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<KeyVaultSettings>()
            .BindConfiguration(KeyVaultSettings.AppSettingsKey)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<AzureOpenAISettings>()
            .BindConfiguration(AzureOpenAISettings.AppSettingsKey)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IAzureOpenAIProvider, AzureOpenAIService>();
        services.AddScoped<IInterviewRepository, InterviewRepository>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<IAudioRepository, AudioRepository>();
        services.AddScoped<IKeyVaultProvider, KeyVaultService>();
        

        return services;
    }
}
