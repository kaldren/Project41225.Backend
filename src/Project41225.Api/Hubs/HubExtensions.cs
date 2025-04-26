namespace Project41225.Api.Hubs;

public static class ApiHubExtensions
{
    public static IServiceCollection AddApiHub(this IServiceCollection services)
    {
        services.AddSignalR();

        return services;
    }

    public static WebApplication UseApiHub(this WebApplication app)
    {
        app.MapHub<AudioHub>("/hubs/audio");

        return app;
    }
}
