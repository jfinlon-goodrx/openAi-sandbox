using Microsoft.AspNetCore.Mvc;
using PublishingAssistant.Core;

namespace PublishingAssistant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PublishingController : ControllerBase
{
    private readonly PublishingService _publishingService;
    private readonly ILogger<PublishingController> _logger;

    public PublishingController(
        PublishingService publishingService,
        ILogger<PublishingController> logger)
    {
        _publishingService = publishingService;
        _logger = logger;
    }

    /// <summary>
    /// Generates a comprehensive book review
    /// </summary>
    [HttpPost("review")]
    public async Task<ActionResult<BookReview>> GenerateReview([FromBody] ReviewRequest request)
    {
        try
        {
            var review = await _publishingService.GenerateReviewAsync(
                request.Content,
                request.Genre);
            return Ok(review);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating review");
            return StatusCode(500, new { error = "Failed to generate review", message = ex.Message });
        }
    }

    /// <summary>
    /// Generates a book summary/blurb
    /// </summary>
    [HttpPost("summary")]
    public async Task<ActionResult<string>> GenerateSummary([FromBody] SummaryRequest request)
    {
        try
        {
            var summary = await _publishingService.GenerateSummaryAsync(
                request.Content,
                request.MaxLength ?? 250);
            return Ok(new { summary });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating summary");
            return StatusCode(500, new { error = "Failed to generate summary", message = ex.Message });
        }
    }

    /// <summary>
    /// Generates marketing copy/blurb
    /// </summary>
    [HttpPost("marketing-blurb")]
    public async Task<ActionResult<MarketingBlurb>> GenerateMarketingBlurb([FromBody] MarketingRequest request)
    {
        try
        {
            var blurb = await _publishingService.GenerateMarketingBlurbAsync(
                request.Content,
                request.TargetAudience);
            return Ok(blurb);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating marketing blurb");
            return StatusCode(500, new { error = "Failed to generate marketing blurb", message = ex.Message });
        }
    }

    /// <summary>
    /// Generates cover image description
    /// </summary>
    [HttpPost("cover-description")]
    public async Task<ActionResult<CoverImageDescription>> GenerateCoverDescription([FromBody] CoverRequest request)
    {
        try
        {
            var description = await _publishingService.GenerateCoverImageDescriptionAsync(
                request.Content,
                request.Genre);
            return Ok(description);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cover description");
            return StatusCode(500, new { error = "Failed to generate cover description", message = ex.Message });
        }
    }

    /// <summary>
    /// Converts markdown to various formats
    /// </summary>
    [HttpPost("convert")]
    public async Task<ActionResult<string>> ConvertFormat([FromBody] ConvertRequest request)
    {
        try
        {
            var converted = await _publishingService.ConvertMarkdownToFormatAsync(
                request.MarkdownContent,
                request.TargetFormat);
            return Ok(new { content = converted, format = request.TargetFormat });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting format");
            return StatusCode(500, new { error = "Failed to convert format", message = ex.Message });
        }
    }

    /// <summary>
    /// Generates editorial notes
    /// </summary>
    [HttpPost("editorial-notes")]
    public async Task<ActionResult<string>> GenerateEditorialNotes([FromBody] ContentRequest request)
    {
        try
        {
            var notes = await _publishingService.GenerateEditorialNotesAsync(request.Content);
            return Ok(new { notes });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating editorial notes");
            return StatusCode(500, new { error = "Failed to generate editorial notes", message = ex.Message });
        }
    }
}

public class ReviewRequest
{
    public string Content { get; set; } = string.Empty;
    public string? Genre { get; set; }
}

public class SummaryRequest
{
    public string Content { get; set; } = string.Empty;
    public int? MaxLength { get; set; }
}

public class MarketingRequest
{
    public string Content { get; set; } = string.Empty;
    public string? TargetAudience { get; set; }
}

public class CoverRequest
{
    public string Content { get; set; } = string.Empty;
    public string? Genre { get; set; }
}

public class ConvertRequest
{
    public string MarkdownContent { get; set; } = string.Empty;
    public string TargetFormat { get; set; } = "html"; // html, plaintext, epub, pdf
}

public class ContentRequest
{
    public string Content { get; set; } = string.Empty;
}
