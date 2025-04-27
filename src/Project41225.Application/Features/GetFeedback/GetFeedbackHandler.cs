using MediatR;
using Project41225.Application.DTOs;
using Project41225.Application.Interfaces;

namespace Project41225.Application.Features.GetFeedback;

public class GetFeedbackHandler : IRequestHandler<GetFeedbackQuery, FeedbackDto>
{
    private readonly IFeedbackService _feedbackService;
    public GetFeedbackHandler(IFeedbackService feedbackService) => _feedbackService = feedbackService;

    public async Task<FeedbackDto> Handle(GetFeedbackQuery request, CancellationToken cancellationToken)
    {
        var result = await _feedbackService.CompileFeedbackAsync(request.SessionId, cancellationToken);
        return new FeedbackDto
        {
            SessionId = result.SessionId,
            Clarity = result.Clarity,
            Content = result.Content,
            Pace = result.Pace,
            Questions = [.. result.Questions.Select(q => new QuestionScoreDto
            {
                Id = q.Id,
                Text = q.Text,
                Score = q.Score
            })]
        };
    }
}
