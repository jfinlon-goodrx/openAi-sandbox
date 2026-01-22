using AutonomousDevelopmentAgent.Core;
using Microsoft.AspNetCore.Mvc;

namespace AutonomousDevelopmentAgent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AutonomousAgentController : ControllerBase
{
    private readonly AutonomousDevelopmentAgent _agent;
    private readonly ILogger<AutonomousAgentController> _logger;

    public AutonomousAgentController(
        AutonomousDevelopmentAgent agent,
        ILogger<AutonomousAgentController> logger)
    {
        _agent = agent;
        _logger = logger;
    }

    /// <summary>
    /// Analyzes code and determines if improvements are needed
    /// </summary>
    [HttpPost("analyze")]
    public async Task<ActionResult<AnalysisResult>> AnalyzeCode(
        [FromBody] AnalyzeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _agent.AnalyzeCodeAsync(
            request.Code,
            request.Context,
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Generates improved code based on analysis
    /// </summary>
    [HttpPost("improve")]
    public async Task<ActionResult<ImproveResponse>> ImproveCode(
        [FromBody] ImproveRequest request,
        CancellationToken cancellationToken)
    {
        var improvedCode = await _agent.GenerateImprovedCodeAsync(
            request.OriginalCode,
            request.Analysis,
            cancellationToken);

        return Ok(new ImproveResponse { ImprovedCode = improvedCode });
    }

    /// <summary>
    /// Executes full autonomous workflow: analyze, improve, create PR
    /// </summary>
    [HttpPost("workflow")]
    public async Task<ActionResult<AutonomousWorkflowResult>> ExecuteWorkflow(
        [FromBody] WorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _agent.ExecuteAutonomousWorkflowAsync(
            request.Code,
            request.Context,
            request.Repository,
            request.FilePath,
            cancellationToken);

        return Ok(result);
    }
}

public class AnalyzeRequest
{
    public string Code { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
}

public class ImproveRequest
{
    public string OriginalCode { get; set; } = string.Empty;
    public AnalysisResult Analysis { get; set; } = new();
}

public class ImproveResponse
{
    public string ImprovedCode { get; set; } = string.Empty;
}

public class WorkflowRequest
{
    public string Code { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
}
