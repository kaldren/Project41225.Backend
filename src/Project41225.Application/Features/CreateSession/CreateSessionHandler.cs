using MediatR;
using Project41225.Application.Interfaces;
using Project41225.Domain.Entities;

namespace Project41225.Application.Features.CreateSession;

public class CreateSessionHandler : IRequestHandler<CreateSessionCommand, Guid>
{
    private readonly IInterviewRepository _repo;
    public CreateSessionHandler(IInterviewRepository repo) => _repo = repo;

    public async Task<Guid> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
    {
        var session = new InterviewSession { Id = Guid.NewGuid() };
        await _repo.CreateSessionAsync(session, cancellationToken);
        return session.Id;
    }
}
