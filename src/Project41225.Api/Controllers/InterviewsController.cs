using MediatR;
using Microsoft.AspNetCore.Mvc;
using Project41225.Application.DTOs;
using Project41225.Application.Features.GetFeedback;

namespace Project41225.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InterviewsController : Controller
{
    private readonly IMediator _mediator;
    public InterviewsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Retrieves feedback for a completed interview session.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the interview session.</param>
    [HttpGet("{sessionId}/feedback")]
    [ProducesResponseType(typeof(FeedbackDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFeedback(Guid sessionId)
    {
        var feedback = await _mediator.Send(new GetFeedbackQuery(sessionId));
        if (feedback == null)
            return NotFound();
        return Ok(feedback);
    }
}
