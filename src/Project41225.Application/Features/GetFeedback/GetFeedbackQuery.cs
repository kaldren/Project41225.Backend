using MediatR;
using Project41225.Application.DTOs;

namespace Project41225.Application.Features.GetFeedback;

public class GetFeedbackQuery : IRequest<FeedbackDto>
{
    public GetFeedbackQuery(Guid sessionId)
    {
        SessionId = sessionId;
    }

    public Guid SessionId { get; }
}
