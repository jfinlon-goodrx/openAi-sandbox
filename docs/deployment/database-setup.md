# Database Setup

**For:** Developers who want to persist data from AI-powered applications.

**What you'll learn:** How to set up Entity Framework Core with SQL Server, SQLite, or in-memory databases for storing user stories, action items, test cases, and other application data.

## Overview

This guide covers setting up database integration using Entity Framework Core to persist data from AI-powered applications.

**Why use a database?**
- **Persistence:** Store AI-generated content for later retrieval
- **History:** Track changes and maintain audit trails
- **Relationships:** Model relationships between entities (user stories, test cases, etc.)
- **Querying:** Efficiently query and filter stored data
- **Integration:** Connect AI workflows with existing data systems Guide

Complete guide to setting up Entity Framework Core with SQL Server or In-Memory database.

## Overview

Entity Framework Core provides data persistence for:
- User stories
- Action items
- Test cases
- Requirement documents
- Meeting transcripts

## Setup

### 1. SQL Server

#### Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OpenAIPlatform;User Id=sa;Password=YourPassword;TrustServerCertificate=true;"
  }
}
```

#### Register Services

```csharp
using Shared.Data;

builder.Services.AddApplicationDbContext(builder.Configuration);
```

#### Migrations

```bash
# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Create migration
dotnet ef migrations add InitialCreate --project shared/Data --startup-project src/RequirementsAssistant/RequirementsAssistant.Api

# Apply migration
dotnet ef database update --project shared/Data --startup-project src/RequirementsAssistant/RequirementsAssistant.Api
```

### 2. In-Memory Database (Development/Testing)

```csharp
builder.Services.AddApplicationDbContext(builder.Configuration, useInMemory: true);
```

**Note**: In-memory database is cleared when application restarts.

## Usage

### Save User Stories

```csharp
public class RequirementsService
{
    private readonly ApplicationDbContext _dbContext;

    public async Task<List<UserStory>> GenerateAndSaveUserStoriesAsync(string content)
    {
        // Generate user stories
        var stories = await GenerateUserStoriesAsync(content);

        // Save to database
        _dbContext.UserStories.AddRange(stories);
        await _dbContext.SaveChangesAsync();

        return stories;
    }
}
```

### Query User Stories

```csharp
// Get all user stories
var stories = await _dbContext.UserStories.ToListAsync();

// Get stories by priority
var highPriorityStories = await _dbContext.UserStories
    .Where(s => s.Priority == "High")
    .ToListAsync();

// Get recent stories
var recentStories = await _dbContext.UserStories
    .OrderByDescending(s => s.CreatedAt)
    .Take(10)
    .ToListAsync();
```

### Save Meeting Transcripts

```csharp
var transcript = new MeetingTranscript
{
    Title = "Sprint Planning Meeting",
    Transcript = transcriptText,
    Summary = summary,
    MeetingDate = DateTime.UtcNow,
    Attendees = attendees,
    ActionItems = actionItems
};

_dbContext.MeetingTranscripts.Add(transcript);
await _dbContext.SaveChangesAsync();
```

### Query with Related Data

```csharp
// Get requirement document with user stories
var document = await _dbContext.RequirementDocuments
    .Include(d => d.UserStories)
    .FirstOrDefaultAsync(d => d.Id == documentId);

// Get meeting transcript with action items
var meeting = await _dbContext.MeetingTranscripts
    .Include(m => m.ActionItems)
    .FirstOrDefaultAsync(m => m.Id == meetingId);
```

## Migration Commands

```bash
# Add migration
dotnet ef migrations add MigrationName --project shared/Data --startup-project src/[Project]/[Project].Api

# Update database
dotnet ef database update --project shared/Data --startup-project src/[Project]/[Project].Api

# Remove last migration
dotnet ef migrations remove --project shared/Data --startup-project src/[Project]/[Project].Api

# Generate SQL script
dotnet ef migrations script --project shared/Data --startup-project src/[Project]/[Project].Api
```

## Docker with SQL Server

### docker-compose.yml

```yaml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Password123
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql

  requirements-assistant:
    # ... other config
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=OpenAIPlatform;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=true;
    depends_on:
      - sqlserver

volumes:
  sqlserver-data:
```

## Best Practices

1. **Use Migrations**: Always use EF Core migrations for schema changes
2. **Connection Strings**: Store in configuration, not code
3. **Async Methods**: Use async/await for all database operations
4. **Error Handling**: Handle database exceptions appropriately
5. **Connection Pooling**: EF Core handles connection pooling automatically
6. **Transactions**: Use transactions for multi-step operations

## Resources

- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [SQL Server Documentation](https://learn.microsoft.com/en-us/sql/)
