using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

namespace Project41225.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly CosmosClient _cosmosClient;
    private readonly IConfiguration _config;

    public HealthController(CosmosClient cosmosClient, IConfiguration config)
    {
        _cosmosClient = cosmosClient;
        _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> CheckHealth()
    {
        try
        {
            var databaseResponse = await _cosmosClient.GetDatabase(_config["CosmosDB:DatabaseName"]).ReadAsync();

            if (databaseResponse.StatusCode == System.Net.HttpStatusCode.OK)
                return Ok(new { status = "Healthy", service = "CosmosDB" });

            return StatusCode((int)databaseResponse.StatusCode, new { status = "Unhealthy", service = "CosmosDB" });
        }
        catch (CosmosException ex)
        {
            return StatusCode((int)ex.StatusCode, new { status = "Unhealthy", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = "Unhealthy", message = ex.Message });
        }
    }
}