using Microsoft.AspNetCore.Mvc;
using DevOpsAssistant.Core;

namespace DevOpsAssistant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevOpsController : ControllerBase
{
    private readonly DevOpsService _devOpsService;
    private readonly ILogger<DevOpsController> _logger;

    public DevOpsController(
        DevOpsService devOpsService,
        ILogger<DevOpsController> logger)
    {
        _devOpsService = devOpsService;
        _logger = logger;
    }

    /// <summary>
    /// Analyzes application logs
    /// </summary>
    [HttpPost("analyze-logs")]
    public async Task<ActionResult<LogAnalysis>> AnalyzeLogs([FromBody] AnalyzeLogsRequest request)
    {
        try
        {
            var analysis = await _devOpsService.AnalyzeLogsAsync(
                request.Logs,
                request.LogType ?? "application",
                request.TimeRangeHours.HasValue ? TimeSpan.FromHours(request.TimeRangeHours.Value) : null);
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing logs");
            return StatusCode(500, new { error = "Failed to analyze logs", message = ex.Message });
        }
    }

    /// <summary>
    /// Generates incident report
    /// </summary>
    [HttpPost("incident-report")]
    public async Task<ActionResult<IncidentReport>> GenerateIncidentReport([FromBody] IncidentReportRequest request)
    {
        try
        {
            var report = await _devOpsService.GenerateIncidentReportAsync(
                request.LogAnalysis,
                request.Severity ?? "Medium");
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating incident report");
            return StatusCode(500, new { error = "Failed to generate incident report", message = ex.Message });
        }
    }

    /// <summary>
    /// Analyzes CI/CD pipeline
    /// </summary>
    [HttpPost("analyze-pipeline")]
    public async Task<ActionResult<PipelineAnalysis>> AnalyzePipeline([FromBody] AnalyzePipelineRequest request)
    {
        try
        {
            var analysis = await _devOpsService.AnalyzePipelineAsync(
                request.PipelineLogs,
                request.PipelineType ?? "GitHub Actions");
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing pipeline");
            return StatusCode(500, new { error = "Failed to analyze pipeline", message = ex.Message });
        }
    }

    /// <summary>
    /// Optimizes CI/CD pipeline
    /// </summary>
    [HttpPost("optimize-pipeline")]
    public async Task<ActionResult<PipelineOptimization>> OptimizePipeline([FromBody] OptimizePipelineRequest request)
    {
        try
        {
            var optimization = await _devOpsService.OptimizePipelineAsync(
                request.PipelineAnalysis,
                request.TargetMetrics?.ToArray());
            return Ok(optimization);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing pipeline");
            return StatusCode(500, new { error = "Failed to optimize pipeline", message = ex.Message });
        }
    }

    /// <summary>
    /// Reviews Infrastructure as Code
    /// </summary>
    [HttpPost("review-infrastructure")]
    public async Task<ActionResult<InfrastructureReview>> ReviewInfrastructure([FromBody] ReviewInfrastructureRequest request)
    {
        try
        {
            var review = await _devOpsService.ReviewInfrastructureCodeAsync(
                request.Code,
                request.InfrastructureType ?? "Terraform");
            return Ok(review);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing infrastructure");
            return StatusCode(500, new { error = "Failed to review infrastructure", message = ex.Message });
        }
    }

    /// <summary>
    /// Generates deployment script
    /// </summary>
    [HttpPost("deployment-script")]
    public async Task<ActionResult<DeploymentScript>> GenerateDeploymentScript([FromBody] DeploymentScriptRequest request)
    {
        try
        {
            var script = await _devOpsService.GenerateDeploymentScriptAsync(
                request.ApplicationType,
                request.TargetEnvironment,
                request.DeploymentMethod ?? "CI/CD");
            return Ok(script);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating deployment script");
            return StatusCode(500, new { error = "Failed to generate deployment script", message = ex.Message });
        }
    }

    /// <summary>
    /// Generates runbook
    /// </summary>
    [HttpPost("runbook")]
    public async Task<ActionResult<Runbook>> GenerateRunbook([FromBody] RunbookRequest request)
    {
        try
        {
            var runbook = await _devOpsService.GenerateRunbookAsync(
                request.ApplicationName,
                request.DeploymentSteps);
            return Ok(runbook);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating runbook");
            return StatusCode(500, new { error = "Failed to generate runbook", message = ex.Message });
        }
    }

    /// <summary>
    /// Analyzes metrics
    /// </summary>
    [HttpPost("analyze-metrics")]
    public async Task<ActionResult<MetricsAnalysis>> AnalyzeMetrics([FromBody] AnalyzeMetricsRequest request)
    {
        try
        {
            var analysis = await _devOpsService.AnalyzeMetricsAsync(
                request.Metrics,
                request.TimeRangeDays.HasValue ? TimeSpan.FromDays(request.TimeRangeDays.Value) : null);
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing metrics");
            return StatusCode(500, new { error = "Failed to analyze metrics", message = ex.Message });
        }
    }

    /// <summary>
    /// Optimizes alerting rules
    /// </summary>
    [HttpPost("optimize-alerts")]
    public async Task<ActionResult<AlertingOptimization>> OptimizeAlerts([FromBody] OptimizeAlertsRequest request)
    {
        try
        {
            var optimization = await _devOpsService.OptimizeAlertingRulesAsync(
                request.CurrentAlerts,
                request.MetricsAnalysis);
            return Ok(optimization);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing alerts");
            return StatusCode(500, new { error = "Failed to optimize alerts", message = ex.Message });
        }
    }

    /// <summary>
    /// Scans for security issues
    /// </summary>
    [HttpPost("security-scan")]
    public async Task<ActionResult<SecurityScan>> ScanSecurity([FromBody] SecurityScanRequest request)
    {
        try
        {
            var scan = await _devOpsService.ScanSecurityAsync(
                request.ScanTarget,
                request.ConfigFiles);
            return Ok(scan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing security scan");
            return StatusCode(500, new { error = "Failed to perform security scan", message = ex.Message });
        }
    }

    /// <summary>
    /// Analyzes Dockerfile
    /// </summary>
    [HttpPost("analyze-dockerfile")]
    public async Task<ActionResult<DockerfileAnalysis>> AnalyzeDockerfile([FromBody] AnalyzeDockerfileRequest request)
    {
        try
        {
            var analysis = await _devOpsService.AnalyzeDockerfileAsync(request.DockerfileContent);
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing Dockerfile");
            return StatusCode(500, new { error = "Failed to analyze Dockerfile", message = ex.Message });
        }
    }

    /// <summary>
    /// Analyzes Kubernetes manifests
    /// </summary>
    [HttpPost("analyze-kubernetes")]
    public async Task<ActionResult<KubernetesAnalysis>> AnalyzeKubernetes([FromBody] AnalyzeKubernetesRequest request)
    {
        try
        {
            var analysis = await _devOpsService.AnalyzeKubernetesManifestsAsync(request.ManifestYaml);
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing Kubernetes manifests");
            return StatusCode(500, new { error = "Failed to analyze Kubernetes manifests", message = ex.Message });
        }
    }
}

// Request models
public class AnalyzeLogsRequest
{
    public string Logs { get; set; } = string.Empty;
    public string? LogType { get; set; }
    public int? TimeRangeHours { get; set; }
}

public class IncidentReportRequest
{
    public LogAnalysis LogAnalysis { get; set; } = new();
    public string? Severity { get; set; }
}

public class AnalyzePipelineRequest
{
    public string PipelineLogs { get; set; } = string.Empty;
    public string? PipelineType { get; set; }
}

public class OptimizePipelineRequest
{
    public PipelineAnalysis PipelineAnalysis { get; set; } = new();
    public List<string>? TargetMetrics { get; set; }
}

public class ReviewInfrastructureRequest
{
    public string Code { get; set; } = string.Empty;
    public string? InfrastructureType { get; set; }
}

public class DeploymentScriptRequest
{
    public string ApplicationType { get; set; } = string.Empty;
    public string TargetEnvironment { get; set; } = string.Empty;
    public string? DeploymentMethod { get; set; }
}

public class RunbookRequest
{
    public string ApplicationName { get; set; } = string.Empty;
    public List<string>? DeploymentSteps { get; set; }
}

public class AnalyzeMetricsRequest
{
    public string Metrics { get; set; } = string.Empty;
    public int? TimeRangeDays { get; set; }
}

public class OptimizeAlertsRequest
{
    public string CurrentAlerts { get; set; } = string.Empty;
    public MetricsAnalysis? MetricsAnalysis { get; set; }
}

public class SecurityScanRequest
{
    public string ScanTarget { get; set; } = string.Empty;
    public List<string>? ConfigFiles { get; set; }
}

public class AnalyzeDockerfileRequest
{
    public string DockerfileContent { get; set; } = string.Empty;
}

public class AnalyzeKubernetesRequest
{
    public string ManifestYaml { get; set; } = string.Empty;
}
