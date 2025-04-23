using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using Microsoft.Graph;
using Project41225.Application.Interfaces;

namespace Project41225.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly IKeyVaultService _keyVaultService;
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, GraphServiceClient graphServiceClient, IKeyVaultService keyVaultService)
        {
            _logger = logger;
            _graphServiceClient = graphServiceClient;
            _keyVaultService = keyVaultService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            // Example of using Key Vault service
            var secretValue = await _keyVaultService.GetSecretAsync("MySecret");
            if (string.IsNullOrEmpty(secretValue))
            {
                _logger.LogWarning("Secret value is null or empty.");
            }
            else
            {
                _logger.LogInformation($"Secret value retrieved: {secretValue}");
            }
            var user = await _graphServiceClient.Me.Request().GetAsync();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
