namespace Project41225.Api.Extensions;

public static class CorsExtensions
{
    private const string PolicyName = "CorsPolicy";

    /// <summary>
    /// Registers a CORS policy using origins from configuration["Cors:AllowedOrigins"].
    /// </summary>
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration config)
    {
        var origins = config
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? new string[0];

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, builder =>
            {
                builder
                  .WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
            });
        });

        return services;
    }

    public static WebApplication UseCorsPolicy(this WebApplication app)
    {
        app.UseCors(PolicyName);
        return app;
    }
}
