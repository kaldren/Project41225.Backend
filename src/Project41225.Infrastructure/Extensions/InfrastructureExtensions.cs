using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Project41225.Application.Common.Settings;
using Project41225.Application.Interfaces;
using Project41225.Infrastructure.Data;
using Project41225.Infrastructure.Hubs;
using Project41225.Infrastructure.Repositories;
using Project41225.Infrastructure.Services;

namespace Project41225.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDataCosmosDB(configuration);

        services.AddRepositoriesAndServices(configuration);

        services.AddOpenAIServices();

        services.AddHubRealtimeAudioChatServices();

        return services;
    }

    public static WebApplication UseInfrastructure(this WebApplication app)
    {
        app.UseHubRealtimeAudioChat();

        return app;
    }

    private static IServiceCollection AddDataCosmosDB(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<CosmosSettings>()
            .BindConfiguration(CosmosSettings.AppSettingsKey)
            .ValidateDataAnnotations()
            .ValidateOnStart();

     
        services.AddDbContext<CosmosContext>((sp, options) =>
        {
            var cosmosSettings = sp.GetRequiredService<IOptions<CosmosSettings>>().Value;

            options.UseCosmos(
                 cosmosSettings.AccountEndpoint,
                 cosmosSettings.AccountKey,
                 cosmosSettings.DatabaseName,
                 cosmosOptions =>
                 {
                     cosmosOptions.ConnectionMode(ConnectionMode.Gateway);
                 });
        });


        return services;
    }

    private static IServiceCollection AddRepositoriesAndServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<KeyVaultSettings>()
            .BindConfiguration(KeyVaultSettings.AppSettingsKey)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        
        services.AddScoped<IInterviewRepository, InterviewRepository>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<IAudioRepository, AudioRepository>();
        services.AddScoped<IKeyVaultProvider, KeyVaultService>();
        

        return services;
    }

    private static IServiceCollection AddOpenAIServices(this IServiceCollection services)
    {
        services.AddOptions<AzureOpenAISettings>()
            .BindConfiguration(AzureOpenAISettings.AppSettingsKey)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IAzureOpenAIProvider, AzureOpenAIService>();
        return services;
    }

    private static IServiceCollection AddHubRealtimeAudioChatServices(this IServiceCollection services)
    {
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.MaximumReceiveMessageSize = 1024 * 1024; // allow up to 1MB per message
        })
        .AddMessagePackProtocol();

        services.AddSingleton<IRealtimeAudioChatService, RealtimeAudioChatService>();

        return services;
    }

    private static WebApplication UseHubRealtimeAudioChat(this WebApplication app)
    {
        app.MapHub<AudioHub>("/hubs/audio");

        return app;
    }
}
