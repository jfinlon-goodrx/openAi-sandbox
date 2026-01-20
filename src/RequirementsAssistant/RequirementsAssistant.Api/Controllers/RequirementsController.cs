using Microsoft.AspNetCore.Mvc;
using Models;
using RequirementsAssistant.Core;

namespace RequirementsAssistant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RequirementsController : ControllerBase
{
    private readonly RequirementsService _requirementsService;
    private readonly ILogger<RequirementsController> _logger;

    public RequirementsController(
        RequirementsService requirementsService,
        ILogger<RequirementsController> logger)
    {
        _requirementsService = requirementsService;
        _logger = logger;
    }

    /// <summary>
    /// Summarizes a requirements document
    /// </summary>
    [HttpPost("summarize")]
    public async Task<ActionResult<string>> SummarizeDocument([FromBody] DocumentRequest request)
    {
        try
        {
            var summary = await _requirementsService.SummarizeDocumentAsync(request.Content);
            return Ok(new { summary });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error summarizing document");
            return StatusCode(500, new { error = "Failed to summarize document", message = ex.Message });
        }
    }

    /// <summary>
    /// Generates user stories from requirements document
    /// </summary>
    [HttpPost("generate-user-stories")]
    public async Task<ActionResult<List<UserStory>>> GenerateUserStories([FromBody] DocumentRequest request)
    {
        try
        {
            var stories = await _requirementsService.GenerateUserStoriesAsync(request.Content);
            return Ok(stories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating user stories");
            return StatusCode(500, new { error = "Failed to generate user stories", message = ex.Message });
        }
    }

    /// <summary>
    /// Answers questions about requirements
    /// </summary>
    [HttpPost("ask")]
    public async Task<ActionResult<string>> AskQuestion([FromBody] QuestionRequest request)
    {
        try
        {
            var answer = await _requirementsService.AnswerQuestionAsync(
                request.Question,
                request.DocumentContent);
            return Ok(new { answer });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error answering question");
            return StatusCode(500, new { error = "Failed to answer question", message = ex.Message });
        }
    }

    /// <summary>
    /// Identifies gaps and conflicts in requirements
    /// </summary>
    [HttpPost("analyze")]
    public async Task<ActionResult<string>> AnalyzeRequirements([FromBody] DocumentRequest request)
    {
        try
        {
            var analysis = await _requirementsService.IdentifyGapsAndConflictsAsync(request.Content);
            return Ok(new { analysis });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing requirements");
            return StatusCode(500, new { error = "Failed to analyze requirements", message = ex.Message });
        }
    }
}

public class DocumentRequest
{
    public string Content { get; set; } = string.Empty;
}

public class QuestionRequest
{
    public string Question { get; set; } = string.Empty;
    public string DocumentContent { get; set; } = string.Empty;
}
