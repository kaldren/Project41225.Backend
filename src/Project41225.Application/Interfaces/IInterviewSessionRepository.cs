using Project41225.Domain.Entities;

namespace Project41225.Application.Interfaces;

public interface IInterviewSessionRepository
{
    Task SaveAsync(InterviewSession session);
    Task<InterviewSession?> GetByIdAsync(string id, string userId);
}
