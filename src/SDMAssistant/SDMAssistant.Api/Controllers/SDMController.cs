using Microsoft.AspNetCore.Mvc;
using SDMAssistant.Core;

namespace SDMAssistant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SDMController : ControllerBase
{
    private readonly SDMService _sdmService;
    private readonly ILogger<SDMController> _logger;

    public SDMController(
        SDMService sdmService,
        ILogger<SDMController> logger)
    {
        _sdmService = sdmService;
        _logger = logger;
    }

    /// <summary>
    /// Gets daily activity summary
    /// </summary>
    [HttpPost("daily-summary")]
    public async Task<ActionResult<DailyActivitySummary>> GetDailySummary([FromBody] DailySummaryRequest request)
    {
        try
        {
            var summary = await _sdmService.GetDailyActivitySummaryAsync(
                request.ProjectKey,
                request.Date);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating daily summary");
            return StatusCode(500, new { error = "Failed to generate daily summary", message = ex.Message });
        }
    }

    /// <summary>
    /// Generates standup talking points
    /// </summary>
    [HttpPost("standup-talking-points")]
    public async Task<ActionResult<List<string>>> GenerateStandupTalkingPoints([FromBody] DailyActivitySummary summary)
    {
        try
        {
            var points = await _sdmService.GenerateStandupTalkingPointsAsync(summary);
            return Ok(points);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating talking points");
            return StatusCode(500, new { error = "Failed to generate talking points", message = ex.Message });
        }
    }

    /// <summary>
    /// Analyzes team velocity
    /// </summary>
    [HttpPost("analyze-velocity")]
    public async Task<ActionResult<VelocityAnalysis>> AnalyzeVelocity([FromBody] VelocityAnalysisRequest request)
    {
        try
        {
            var analysis = await _sdmService.AnalyzeTeamVelocityAsync(
                request.ProjectKey,
                request.SprintCount);
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing velocity");
            return StatusCode(500, new { error = "Failed to analyze velocity", message = ex.Message });
        }
    }

    /// <summary>
    /// Generates sprint plan
    /// </summary>
    [HttpPost("sprint-plan")]
    public async Task<ActionResult<SprintPlan>> GenerateSprintPlan([FromBody] SprintPlanRequest request)
    {
        try
        {
            var plan = await _sdmService.GenerateSprintPlanAsync(
                request.ProjectKey,
                request.SprintGoal,
                request.TeamCapacity);
            return Ok(plan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sprint plan");
            return StatusCode(500, new { error = "Failed to generate sprint plan", message = ex.Message });
        }
    }

    /// <summary>
    /// Identifies risks
    /// </summary>
    [HttpPost("identify-risks")]
    public async Task<ActionResult<List<Risk>>> IdentifyRisks([FromBody] RiskAnalysisRequest request)
    {
        try
        {
            var risks = await _sdmService.IdentifyRisksAsync(
                request.ProjectKey,
                request.SprintId);
            return Ok(risks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error identifying risks");
            return StatusCode(500, new { error = "Failed to identify risks", message = ex.Message });
        }
    }

    /// <summary>
    /// Generates status report
    /// </summary>
    [HttpPost("status-report")]
    public async Task<ActionResult<StatusReport>> GenerateStatusReport([FromBody] StatusReportRequest request)
    {
        try
        {
            var report = await _sdmService.GenerateStatusReportAsync(
                request.ProjectKey,
                request.StartDate,
                request.EndDate,
                request.IncludeMetrics);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating status report");
            return StatusCode(500, new { error = "Failed to generate status report", message = ex.Message });
        }
    }
}

public class DailySummaryRequest
{
    public string ProjectKey { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

public class VelocityAnalysisRequest
{
    public string ProjectKey { get; set; } = string.Empty;
    public int SprintCount { get; set; } = 5;
}

public class SprintPlanRequest
{
    public string ProjectKey { get; set; } = string.Empty;
    public string SprintGoal { get; set; } = string.Empty;
    public int TeamCapacity { get; set; }
}

public class RiskAnalysisRequest
{
    public string ProjectKey { get; set; } = string.Empty;
    public string? SprintId { get; set; }
}

public class StatusReportRequest
{
    public string ProjectKey { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IncludeMetrics { get; set; } = true;
}
