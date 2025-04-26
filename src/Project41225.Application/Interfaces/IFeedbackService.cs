using Project41225.Domain.Entities;

namespace Project41225.Application.Interfaces;

public interface IFeedbackService
{
    Task<SessionFeedback> CompileFeedbackAsync(Guid sessionId, CancellationToken cancellationToken);
}
