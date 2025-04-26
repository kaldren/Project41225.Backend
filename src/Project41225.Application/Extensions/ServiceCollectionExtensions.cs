using Microsoft.Extensions.DependencyInjection;
using Project41225.Application.Interfaces;
using Project41225.Application.Services;

namespace Project41225.Application.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IInterviewService, InterviewService>();

        return services;
    }
}
