using Microsoft.AspNetCore.Mvc;
using OpenAIShared;
using RequirementsAssistant.Core;

namespace RequirementsAssistant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StreamingController : ControllerBase
{
    private readonly StreamingService _streamingService;
    private readonly RequirementsService _requirementsService;
    private readonly ILogger<StreamingController> _logger;

    public StreamingController(
        StreamingService streamingService,
        RequirementsService requirementsService,
        ILogger<StreamingController> logger)
    {
        _streamingService = streamingService;
        _requirementsService = requirementsService;
        _logger = logger;
    }

    /// <summary>
    /// Streams requirements summarization
    /// </summary>
    [HttpPost("summarize-stream")]
    public async Task SummarizeStream([FromBody] SummarizeRequest request, CancellationToken cancellationToken)
    {
        var chatRequest = new ChatCompletionRequest
        {
            Model = "gpt-4-turbo-preview",
            Messages = new List<ChatMessage>
            {
                new()
                {
                    Role = "system",
                    Content = "You are an expert at summarizing requirements documents. Provide clear, concise summaries."
                },
                new()
                {
                    Role = "user",
                    Content = $"Summarize the following requirements document:\n\n{request.Content}"
                }
            },
            Temperature = 0.3,
            MaxTokens = 1000
        };

        await _streamingService.StreamChatCompletionToHttpResponseAsync(chatRequest, Response, cancellationToken);
    }
}

public class SummarizeRequest
{
    public string Content { get; set; } = string.Empty;
}
