using Microsoft.AspNetCore.Mvc;
using Models;
using RetroAnalyzer.Core;

namespace RetroAnalyzer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RetroController : ControllerBase
{
    private readonly RetroAnalyzerService _retroService;
    private readonly ILogger<RetroController> _logger;

    public RetroController(
        RetroAnalyzerService retroService,
        ILogger<RetroController> logger)
    {
        _retroService = retroService;
        _logger = logger;
    }

    [HttpPost("extract-action-items")]
    public async Task<ActionResult<List<ActionItem>>> ExtractActionItems([FromBody] CommentsRequest request)
    {
        try
        {
            var actionItems = await _retroService.ExtractActionItemsAsync(request.Comments);
            return Ok(actionItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting action items");
            return StatusCode(500, new { error = "Failed to extract action items", message = ex.Message });
        }
    }

    [HttpPost("identify-themes")]
    public async Task<ActionResult<List<string>>> IdentifyThemes([FromBody] CommentsRequest request)
    {
        try
        {
            var themes = await _retroService.IdentifyThemesAsync(request.Comments);
            return Ok(themes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error identifying themes");
            return StatusCode(500, new { error = "Failed to identify themes", message = ex.Message });
        }
    }

    [HttpPost("analyze-sentiment")]
    public async Task<ActionResult<SentimentAnalysis>> AnalyzeSentiment([FromBody] CommentsRequest request)
    {
        try
        {
            var sentiment = await _retroService.AnalyzeSentimentAsync(request.Comments);
            return Ok(sentiment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing sentiment");
            return StatusCode(500, new { error = "Failed to analyze sentiment", message = ex.Message });
        }
    }

    [HttpPost("improvement-suggestions")]
    public async Task<ActionResult<string>> GetImprovementSuggestions([FromBody] CommentsRequest request)
    {
        try
        {
            var suggestions = await _retroService.GenerateImprovementSuggestionsAsync(request.Comments);
            return Ok(new { suggestions });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating suggestions");
            return StatusCode(500, new { error = "Failed to generate suggestions", message = ex.Message });
        }
    }
}

public class CommentsRequest
{
    public List<string> Comments { get; set; } = new();
}
