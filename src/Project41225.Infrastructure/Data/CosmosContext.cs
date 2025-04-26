using Microsoft.EntityFrameworkCore;
using Project41225.Domain.Entities;

namespace Project41225.Infrastructure.Data;

public class CosmosContext : DbContext
{
    public CosmosContext(DbContextOptions<CosmosContext> options) : base(options) { }
    public DbSet<InterviewSession> Sessions { get; set; }
    public DbSet<AudioChunk> AudioChunks { get; set; }
    public DbSet<QuestionScore> QuestionScores { get; set; }
    public DbSet<SessionFeedback> Feedbacks { get; set; }
}
