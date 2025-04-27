using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Project41225.Application.Common.Settings;
using Project41225.Domain.Entities;

namespace Project41225.Infrastructure.Data;

public class CosmosContext : DbContext
{
    private readonly CosmosSettings _settings;

    public CosmosContext(
        DbContextOptions<CosmosContext> options,
        IOptions<CosmosSettings> settings) : base(options) 
    {
        _settings = settings.Value;
        Database.EnsureCreated();
    }
    public DbSet<InterviewSession> Sessions { get; set; }
    public DbSet<AudioChunk> AudioChunks { get; set; }
    public DbSet<QuestionScore> QuestionScores { get; set; }
    public DbSet<SessionFeedback> Feedbacks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set container name and partition keys
        modelBuilder.HasDefaultContainer(_settings.ContainerName);

        modelBuilder.Entity<InterviewSession>(entity =>
        {
            entity.HasPartitionKey(s => s.Id); // Partition by Id (adjust if needed)
        });

        modelBuilder.Entity<SessionFeedback>(entity =>
        {
            entity.HasPartitionKey(f => f.Id); // partition feedback by session
        });

        modelBuilder.Entity<AudioChunk>(entity =>
        {
            entity.HasPartitionKey(f => f.Id); // partition feedback by session
        });

        modelBuilder.Entity<QuestionScore>(entity =>
        {
            entity.HasPartitionKey(f => f.Id); // partition feedback by session
        });

        base.OnModelCreating(modelBuilder);
    }
}
