using Microsoft.AspNetCore.Mvc;
using Models;
using MeetingAnalyzer.Core;

namespace MeetingAnalyzer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MeetingController : ControllerBase
{
    private readonly MeetingService _meetingService;
    private readonly ILogger<MeetingController> _logger;

    public MeetingController(
        MeetingService meetingService,
        ILogger<MeetingController> logger)
    {
        _meetingService = meetingService;
        _logger = logger;
    }

    [HttpPost("transcribe")]
    public async Task<ActionResult<string>> TranscribeAudio(IFormFile audioFile, [FromQuery] string? language = null)
    {
        try
        {
            using var stream = audioFile.OpenReadStream();
            var transcript = await _meetingService.TranscribeAudioAsync(stream, audioFile.FileName, language);
            return Ok(new { transcript });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transcribing audio");
            return StatusCode(500, new { error = "Failed to transcribe audio", message = ex.Message });
        }
    }

    [HttpPost("summarize")]
    public async Task<ActionResult<string>> SummarizeMeeting([FromBody] TranscriptRequest request)
    {
        try
        {
            var summary = await _meetingService.GenerateSummaryAsync(request.Transcript);
            return Ok(new { summary });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error summarizing meeting");
            return StatusCode(500, new { error = "Failed to summarize meeting", message = ex.Message });
        }
    }

    [HttpPost("extract-action-items")]
    public async Task<ActionResult<List<ActionItem>>> ExtractActionItems([FromBody] TranscriptRequest request)
    {
        try
        {
            var actionItems = await _meetingService.ExtractActionItemsAsync(request.Transcript);
            return Ok(actionItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting action items");
            return StatusCode(500, new { error = "Failed to extract action items", message = ex.Message });
        }
    }

    [HttpPost("generate-email")]
    public async Task<ActionResult<string>> GenerateFollowUpEmail([FromBody] EmailRequest request)
    {
        try
        {
            var email = await _meetingService.GenerateFollowUpEmailAsync(
                request.Transcript,
                request.Attendees);
            return Ok(new { email });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating email");
            return StatusCode(500, new { error = "Failed to generate email", message = ex.Message });
        }
    }
}

public class TranscriptRequest
{
    public string Transcript { get; set; } = string.Empty;
}

public class EmailRequest
{
    public string Transcript { get; set; } = string.Empty;
    public List<string> Attendees { get; set; } = new();
}
