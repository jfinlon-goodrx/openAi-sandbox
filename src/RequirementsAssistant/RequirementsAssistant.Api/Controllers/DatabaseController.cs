using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Data;

namespace RequirementsAssistant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DatabaseController> _logger;

    public DatabaseController(
        ApplicationDbContext dbContext,
        ILogger<DatabaseController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Gets all user stories from database
    /// </summary>
    [HttpGet("user-stories")]
    public async Task<ActionResult<List<Models.UserStory>>> GetUserStories()
    {
        var stories = await _dbContext.UserStories
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return Ok(stories);
    }

    /// <summary>
    /// Gets user story by ID
    /// </summary>
    [HttpGet("user-stories/{id}")]
    public async Task<ActionResult<Models.UserStory>> GetUserStory(string id)
    {
        var story = await _dbContext.UserStories.FindAsync(id);
        if (story == null)
        {
            return NotFound();
        }

        return Ok(story);
    }

    /// <summary>
    /// Gets requirement documents
    /// </summary>
    [HttpGet("requirement-documents")]
    public async Task<ActionResult<List<RequirementDocument>>> GetRequirementDocuments()
    {
        var documents = await _dbContext.RequirementDocuments
            .Include(d => d.UserStories)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return Ok(documents);
    }

    /// <summary>
    /// Gets requirement document by ID
    /// </summary>
    [HttpGet("requirement-documents/{id}")]
    public async Task<ActionResult<RequirementDocument>> GetRequirementDocument(string id)
    {
        var document = await _dbContext.RequirementDocuments
            .Include(d => d.UserStories)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null)
        {
            return NotFound();
        }

        return Ok(document);
    }
}
