using Microsoft.AspNetCore.Mvc;
using OpenAIShared;

namespace RequirementsAssistant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly MetricsService _metricsService;
    private readonly ILogger<MetricsController> _logger;

    public MetricsController(
        MetricsService metricsService,
        ILogger<MetricsController> logger)
    {
        _metricsService = metricsService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all metrics
    /// </summary>
    [HttpGet]
    public IActionResult GetMetrics()
    {
        var summary = _metricsService.GetSummary();
        var allMetrics = _metricsService.GetAllMetrics();

        return Ok(new
        {
            Summary = summary,
            ByModel = allMetrics
        });
    }

    /// <summary>
    /// Gets metrics for a specific model
    /// </summary>
    [HttpGet("{model}")]
    public IActionResult GetModelMetrics(string model)
    {
        var metrics = _metricsService.GetMetrics(model);
        if (metrics == null)
        {
            return NotFound(new { error = $"No metrics found for model: {model}" });
        }

        return Ok(metrics);
    }

    /// <summary>
    /// Resets all metrics
    /// </summary>
    [HttpPost("reset")]
    public IActionResult ResetMetrics()
    {
        _metricsService.Reset();
        return Ok(new { message = "Metrics reset successfully" });
    }
}
