using Microsoft.AspNetCore.Mvc;
using Project41225.Application.Interfaces;

namespace Project41225.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InterviewController : ControllerBase
{
    private readonly IAzureOpenAIService _openAIService;

    public InterviewController(IAzureOpenAIService openAIService)
    {
        _openAIService = openAIService;
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return BadRequest("Prompt is required.");

        var response = await _openAIService.GetChatCompletionAsync(prompt);
        return Ok(response);
    }
}