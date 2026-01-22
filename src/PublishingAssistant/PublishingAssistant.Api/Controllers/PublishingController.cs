using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OpenAIShared;
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
    /// Generates actual cover image using DALL-E
    /// </summary>
    [HttpPost("generate-cover-image")]
    public async Task<ActionResult<CoverImageResult>> GenerateCoverImage([FromBody] GenerateCoverRequest request)
    {
        try
        {
            var coverGenerator = new CoverImageGenerator(
                HttpContext.RequestServices.GetRequiredService<OpenAIClient>(),
                HttpContext.RequestServices.GetRequiredService<ILogger<CoverImageGenerator>>());

            CoverImageResult result;
            
            if (!string.IsNullOrEmpty(request.MarkdownContent))
            {
                // Generate from markdown repository
                result = await coverGenerator.GenerateCoverFromMarkdownAsync(
                    request.MarkdownContent,
                    request.Genre,
                    request.Size ?? "1024x1024");
            }
            else if (!string.IsNullOrEmpty(request.Description))
            {
                // Generate from description
                result = await coverGenerator.GenerateCoverImageAsync(
                    request.Description,
                    request.Size ?? "1024x1024",
                    request.Quality ?? "standard");
            }
            else
            {
                return BadRequest(new { error = "Either markdownContent or description must be provided" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cover image");
            return StatusCode(500, new { error = "Failed to generate cover image", message = ex.Message });
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

    /// <summary>
    /// Analyzes cover image using Vision API
    /// </summary>
    [HttpPost("analyze-cover-image")]
    public async Task<ActionResult<CoverImageAnalysis>> AnalyzeCoverImage([FromBody] AnalyzeCoverRequest request)
    {
        try
        {
            var visionService = HttpContext.RequestServices.GetRequiredService<VisionService>();
            var coverAnalyzer = new CoverImageAnalyzer(visionService, 
                HttpContext.RequestServices.GetRequiredService<ILogger<CoverImageAnalyzer>>());
            
            var analysis = await coverAnalyzer.AnalyzeCoverImageAsync(
                request.ImageUrl,
                request.Genre);
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing cover image");
            return StatusCode(500, new { error = "Failed to analyze cover image", message = ex.Message });
        }
    }
}

public class AnalyzeCoverRequest
{
    public string ImageUrl { get; set; } = string.Empty;
    public string? Genre { get; set; }
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

public class GenerateCoverRequest
{
    public string? MarkdownContent { get; set; }
    public string? Description { get; set; }
    public string? Genre { get; set; }
    public string? Size { get; set; }
    public string? Quality { get; set; }
}
