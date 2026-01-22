using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RetroAnalyzer.Core;
using RetroAnalyzer.Api.Hubs;

namespace RetroAnalyzer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RetroController : ControllerBase
{
    private readonly RetroAnalyzerService _service;
    private readonly IHubContext<RetroHub> _hubContext;
    private readonly ILogger<RetroController> _logger;

    public RetroController(
        RetroAnalyzerService service,
        IHubContext<RetroHub> hubContext,
        ILogger<RetroController> logger)
    {
        _service = service;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Analyzes retrospective comments with real-time updates
    /// </summary>
    [HttpPost("analyze-stream")]
    public async Task AnalyzeRetroStream(
        [FromBody] AnalyzeRequest request,
        CancellationToken cancellationToken)
    {
        var roomId = request.RoomId ?? Guid.NewGuid().ToString();
        
        try
        {
            // Notify clients that analysis started
            await _hubContext.Clients.Group(roomId).SendAsync("AnalysisStarted", new { RoomId = roomId });

            // Analyze comments
            var result = await _service.AnalyzeRetrospectiveAsync(
                request.Comments,
                cancellationToken);

            // Send progress updates
            await _hubContext.Clients.Group(roomId).SendAsync("ProgressUpdate", new { Message = "Extracting action items..." });
            
            // Send final result (convert to anonymous object for SignalR)
            await _hubContext.Clients.Group(roomId).SendAsync("AnalysisComplete", new
            {
                Summary = result.Summary,
                ActionItems = result.ActionItems,
                Sentiment = result.Sentiment,
                Themes = result.Themes
            });
            
            await _hubContext.Clients.Group(roomId).SendAsync("AnalysisFinished", new { RoomId = roomId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing retrospective");
            await _hubContext.Clients.Group(roomId).SendAsync("AnalysisError", new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Analyzes retrospective comments (non-streaming)
    /// </summary>
    [HttpPost("analyze")]
    public async Task<ActionResult<object>> AnalyzeRetro(
        [FromBody] AnalyzeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.AnalyzeRetrospectiveAsync(
            request.Comments,
            cancellationToken);

        return Ok(result);
    }
}

public class AnalyzeRequest
{
    public List<string> Comments { get; set; } = new();
    public string? RoomId { get; set; }
}
