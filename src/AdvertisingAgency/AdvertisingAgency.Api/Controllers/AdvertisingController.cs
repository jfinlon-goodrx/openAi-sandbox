using Microsoft.AspNetCore.Mvc;
using AdvertisingAgency.Core;

namespace AdvertisingAgency.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdvertisingController : ControllerBase
{
    private readonly AdvertisingService _advertisingService;
    private readonly ILogger<AdvertisingController> _logger;

    public AdvertisingController(
        AdvertisingService advertisingService,
        ILogger<AdvertisingController> logger)
    {
        _advertisingService = advertisingService;
        _logger = logger;
    }

    /// <summary>
    /// Generates ad copy for various channels
    /// </summary>
    [HttpPost("ad-copy")]
    public async Task<ActionResult<AdCopy>> GenerateAdCopy([FromBody] AdCopyRequest request)
    {
        try
        {
            var adCopy = await _advertisingService.GenerateAdCopyAsync(
                request.ProductName,
                request.ProductDescription,
                request.TargetAudience,
                request.Channel,
                request.Tone);
            return Ok(adCopy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating ad copy");
            return StatusCode(500, new { error = "Failed to generate ad copy", message = ex.Message });
        }
    }

    /// <summary>
    /// Develops campaign strategy
    /// </summary>
    [HttpPost("campaign-strategy")]
    public async Task<ActionResult<CampaignStrategy>> DevelopCampaignStrategy([FromBody] CampaignStrategyRequest request)
    {
        try
        {
            var strategy = await _advertisingService.DevelopCampaignStrategyAsync(
                request.BrandName,
                request.ProductDescription,
                request.TargetAudience,
                request.CampaignObjective,
                request.Budget);
            return Ok(strategy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error developing campaign strategy");
            return StatusCode(500, new { error = "Failed to develop campaign strategy", message = ex.Message });
        }
    }

    /// <summary>
    /// Analyzes target audience
    /// </summary>
    [HttpPost("analyze-audience")]
    public async Task<ActionResult<TargetAudienceAnalysis>> AnalyzeAudience([FromBody] AudienceAnalysisRequest request)
    {
        try
        {
            var analysis = await _advertisingService.AnalyzeTargetAudienceAsync(
                request.ProductDescription,
                request.Demographic,
                request.Psychographic);
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing audience");
            return StatusCode(500, new { error = "Failed to analyze audience", message = ex.Message });
        }
    }

    /// <summary>
    /// Develops brand voice
    /// </summary>
    [HttpPost("brand-voice")]
    public async Task<ActionResult<BrandVoice>> DevelopBrandVoice([FromBody] BrandVoiceRequest request)
    {
        try
        {
            var brandVoice = await _advertisingService.DevelopBrandVoiceAsync(
                request.BrandName,
                request.BrandDescription,
                request.ExistingContent);
            return Ok(brandVoice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error developing brand voice");
            return StatusCode(500, new { error = "Failed to develop brand voice", message = ex.Message });
        }
    }

    /// <summary>
    /// Generates creative brief
    /// </summary>
    [HttpPost("creative-brief")]
    public async Task<ActionResult<CreativeBrief>> GenerateCreativeBrief([FromBody] CreativeBriefRequest request)
    {
        try
        {
            var brief = await _advertisingService.GenerateCreativeBriefAsync(
                request.ClientName,
                request.ProductDescription,
                request.CampaignObjective,
                request.TargetAudience,
                request.Budget);
            return Ok(brief);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating creative brief");
            return StatusCode(500, new { error = "Failed to generate creative brief", message = ex.Message });
        }
    }

    /// <summary>
    /// Generates A/B test hypotheses
    /// </summary>
    [HttpPost("ab-test-hypotheses")]
    public async Task<ActionResult<List<ABTestHypothesis>>> GenerateABTestHypotheses([FromBody] ABTestRequest request)
    {
        try
        {
            var hypotheses = await _advertisingService.GenerateABTestHypothesesAsync(
                request.CampaignDescription,
                request.Metric);
            return Ok(hypotheses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating A/B test hypotheses");
            return StatusCode(500, new { error = "Failed to generate A/B test hypotheses", message = ex.Message });
        }
    }
}

public class AdCopyRequest
{
    public string ProductName { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty; // e.g., "Print", "Digital", "TV", "Radio", "Social Media"
    public string? Tone { get; set; }
}

public class CampaignStrategyRequest
{
    public string BrandName { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public string CampaignObjective { get; set; } = string.Empty;
    public decimal? Budget { get; set; }
}

public class AudienceAnalysisRequest
{
    public string ProductDescription { get; set; } = string.Empty;
    public string? Demographic { get; set; }
    public string? Psychographic { get; set; }
}

public class BrandVoiceRequest
{
    public string BrandName { get; set; } = string.Empty;
    public string BrandDescription { get; set; } = string.Empty;
    public string? ExistingContent { get; set; }
}

public class CreativeBriefRequest
{
    public string ClientName { get; set; } = string.Empty;
    public string ProductDescription { get; set; } = string.Empty;
    public string CampaignObjective { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public string? Budget { get; set; }
}

public class ABTestRequest
{
    public string CampaignDescription { get; set; } = string.Empty;
    public string Metric { get; set; } = string.Empty; // e.g., "Click-through rate", "Conversion rate"
}
