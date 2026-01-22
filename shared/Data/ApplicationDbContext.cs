using Microsoft.EntityFrameworkCore;
using Models;
using RequirementsAssistant.Core;

namespace Shared.Data;

/// <summary>
/// Application database context for storing OpenAI Platform data
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Models.UserStory> UserStories { get; set; }
    public DbSet<Models.ActionItem> ActionItems { get; set; }
    public DbSet<Models.TestCase> TestCases { get; set; }
    public DbSet<RequirementDocument> RequirementDocuments { get; set; }
    public DbSet<MeetingTranscript> MeetingTranscripts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure UserStory
        modelBuilder.Entity<Models.UserStory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.AsA).HasMaxLength(200);
            entity.Property(e => e.IWant).HasMaxLength(1000);
            entity.Property(e => e.SoThat).HasMaxLength(1000);
        });

        // Configure ActionItem
        modelBuilder.Entity<Models.ActionItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Owner).HasMaxLength(200);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Owner);
        });

        // Configure TestCase
        modelBuilder.Entity<Models.TestCase>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.HasIndex(e => e.TestType);
        });

        // Configure RequirementDocument
        modelBuilder.Entity<RequirementDocument>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Content).IsRequired();
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure MeetingTranscript
        modelBuilder.Entity<MeetingTranscript>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Transcript).IsRequired();
            entity.HasIndex(e => e.MeetingDate);
        });

    }
}

/// <summary>
/// Meeting transcript entity
/// </summary>
public class MeetingTranscript
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Transcript { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public DateTime MeetingDate { get; set; }
    public List<string> Attendees { get; set; } = new();
    public List<Models.ActionItem> ActionItems { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Requirement document entity
/// </summary>
public class RequirementDocument
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Source { get; set; } // Confluence page ID, file path, etc.
    public List<Models.UserStory> UserStories { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
