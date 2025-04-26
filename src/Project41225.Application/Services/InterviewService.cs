using Project41225.Application.Interfaces;
using Project41225.Domain.Entities;

namespace Project41225.Application.Services;

public class InterviewService : IInterviewService
{
    private readonly IInterviewSessionRepository _interviewSessionRepository;

    public InterviewService(IInterviewSessionRepository interviewSessionRepository)
    {
        _interviewSessionRepository = interviewSessionRepository;
    }

    public async Task<string> StartInterviewAsync(string role)
    {
        var interviewSession = new InterviewSession
        {
            UserId = "0000-0000-0000-0000-0001",
            Role = role,
            QuestionsAnswers = new List<QuestionAnswer>()
        };

        await _interviewSessionRepository.SaveAsync(interviewSession);

        return interviewSession.Id;
    }
}
