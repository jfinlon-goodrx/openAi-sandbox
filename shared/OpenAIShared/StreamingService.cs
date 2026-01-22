using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace OpenAIShared;

/// <summary>
/// Service for streaming OpenAI API responses
/// </summary>
public class StreamingService
{
    private readonly OpenAIClient _openAIClient;
    private readonly ILogger<StreamingService> _logger;

    public StreamingService(
        OpenAIClient openAIClient,
        ILogger<StreamingService> logger)
    {
        _openAIClient = openAIClient;
        _logger = logger;
    }

    /// <summary>
    /// Streams chat completion responses using Server-Sent Events (SSE)
    /// </summary>
    public async Task StreamChatCompletionAsync(
        ChatCompletionRequest request,
        Stream responseStream,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await foreach (var chunk in _openAIClient.GetChatCompletionStreamAsync(request, cancellationToken))
            {
                var data = System.Text.Json.JsonSerializer.Serialize(new { content = chunk });
                var sseData = $"data: {data}\n\n";
                await responseStream.WriteAsync(Encoding.UTF8.GetBytes(sseData), cancellationToken);
                await responseStream.FlushAsync(cancellationToken);
            }

            // Send done signal
            await responseStream.WriteAsync(Encoding.UTF8.GetBytes("data: [DONE]\n\n"), cancellationToken);
            await responseStream.FlushAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error streaming chat completion");
            throw;
        }
    }

    /// <summary>
    /// Streams chat completion to HTTP response using Server-Sent Events
    /// </summary>
    public async Task StreamChatCompletionToHttpResponseAsync(
        ChatCompletionRequest request,
        HttpResponse httpResponse,
        CancellationToken cancellationToken = default)
    {
        httpResponse.ContentType = "text/event-stream";
        httpResponse.Headers.Add("Cache-Control", "no-cache");
        httpResponse.Headers.Add("Connection", "keep-alive");

        await StreamChatCompletionAsync(request, httpResponse.Body, cancellationToken);
    }
}
