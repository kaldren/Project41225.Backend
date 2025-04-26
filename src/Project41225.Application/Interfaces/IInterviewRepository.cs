using Project41225.Domain.Entities;

namespace Project41225.Application.Interfaces;

public interface IInterviewRepository
{
    Task CreateSessionAsync(InterviewSession session, CancellationToken cancellationToken);
}
