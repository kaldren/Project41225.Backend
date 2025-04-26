using Project41225.Application.Interfaces;
using Project41225.Domain.Entities;
using Project41225.Infrastructure.Data;

namespace Project41225.Infrastructure.Repositories;

public class InterviewRepository : IInterviewRepository
{
    private readonly CosmosContext _cosmos;
    public InterviewRepository(CosmosContext cosmos) => _cosmos = cosmos;
    public async Task CreateSessionAsync(InterviewSession session, CancellationToken cancellationToken)
    {
        _cosmos.Sessions.Add(session);
        await _cosmos.SaveChangesAsync(cancellationToken);
    }
}