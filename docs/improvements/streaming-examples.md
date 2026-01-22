# Streaming Response Examples

Examples demonstrating streaming responses for better user experience.

## Overview

Streaming allows responses to be sent incrementally as they're generated, providing:
- **Faster perceived response time** - Users see results immediately
- **Better UX** - Progressive display of content
- **Real-time feedback** - Especially useful for long operations

## Implementation

### Server-Side (ASP.NET Core)

```csharp
[HttpPost("summarize-stream")]
public async Task SummarizeStream([FromBody] SummarizeRequest request, CancellationToken cancellationToken)
{
    var chatRequest = new ChatCompletionRequest
    {
        Model = "gpt-4-turbo-preview",
        Messages = new List<ChatMessage>
        {
            new() { Role = "user", Content = $"Summarize: {request.Content}" }
        }
    };

    await _streamingService.StreamChatCompletionToHttpResponseAsync(chatRequest, Response, cancellationToken);
}
```

### Client-Side (JavaScript)

```javascript
async function streamSummarize(content) {
    const response = await fetch('/api/streaming/summarize-stream', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ content })
    });

    const reader = response.body.getReader();
    const decoder = new TextDecoder();
    let result = '';

    while (true) {
        const { done, value } = await reader.read();
        if (done) break;

        const chunk = decoder.decode(value);
        const lines = chunk.split('\n');

        for (const line of lines) {
            if (line.startsWith('data: ')) {
                const data = line.substring(6);
                if (data === '[DONE]') break;

                try {
                    const parsed = JSON.parse(data);
                    result += parsed.content || '';
                    // Update UI incrementally
                    document.getElementById('output').textContent = result;
                } catch (e) {
                    console.error('Parse error:', e);
                }
            }
        }
    }

    return result;
}
```

### Client-Side (C# HttpClient)

```csharp
public async Task<string> StreamSummarizeAsync(string content)
{
    var request = new HttpRequestMessage(HttpMethod.Post, "/api/streaming/summarize-stream")
    {
        Content = new StringContent(JsonSerializer.Serialize(new { content }), Encoding.UTF8, "application/json")
    };

    var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
    response.EnsureSuccessStatusCode();

    using var stream = await response.Content.ReadAsStreamAsync();
    using var reader = new StreamReader(stream);

    var result = new StringBuilder();
    string? line;

    while ((line = await reader.ReadLineAsync()) != null)
    {
        if (line.StartsWith("data: "))
        {
            var data = line.Substring(6);
            if (data == "[DONE]") break;

            var chunk = JsonSerializer.Deserialize<StreamingChunk>(data);
            if (chunk?.Content != null)
            {
                result.Append(chunk.Content);
            }
        }
    }

    return result.ToString();
}
```

## Use Cases

### 1. Code Review Streaming

Stream code review results as they're generated:

```csharp
[HttpPost("review-stream")]
public async Task ReviewCodeStream([FromBody] ReviewRequest request, CancellationToken cancellationToken)
{
    var chatRequest = new ChatCompletionRequest
    {
        Model = "gpt-4-turbo-preview",
        Messages = new List<ChatMessage>
        {
            new() { Role = "system", Content = "You are a code reviewer..." },
            new() { Role = "user", Content = $"Review this code:\n\n{request.Code}" }
        }
    };

    await _streamingService.StreamChatCompletionToHttpResponseAsync(chatRequest, Response, cancellationToken);
}
```

### 2. Meeting Transcription Streaming

Stream transcription results in real-time:

```csharp
// Process audio in chunks and stream results
public async Task StreamTranscription(Stream audioStream, CancellationToken cancellationToken)
{
    // Process audio chunks and stream transcription results
    // This would require chunking the audio and processing incrementally
}
```

### 3. Long Document Processing

Stream processing results for long documents:

```csharp
[HttpPost("process-document-stream")]
public async Task ProcessDocumentStream([FromBody] DocumentRequest request, CancellationToken cancellationToken)
{
    // Process document in sections and stream results
    var sections = SplitDocument(request.Content);
    
    foreach (var section in sections)
    {
        var result = await ProcessSection(section);
        await Response.WriteAsync($"data: {JsonSerializer.Serialize(new { section, result })}\n\n", cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
    }
    
    await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
}
```

## Best Practices

1. **Set Proper Headers**: Always set `Content-Type: text/event-stream` and `Cache-Control: no-cache`
2. **Handle Cancellation**: Respect cancellation tokens
3. **Error Handling**: Send error events in SSE format
4. **Connection Management**: Handle client disconnections gracefully
5. **Rate Limiting**: Consider rate limiting for streaming endpoints

## Testing Streaming

```csharp
[Fact]
public async Task StreamSummarize_ReturnsIncrementalResults()
{
    // Arrange
    var client = _factory.CreateClient();
    var request = new { content = "Test requirements document" };

    // Act
    var response = await client.PostAsync("/api/streaming/summarize-stream", 
        new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"),
        HttpCompletionOption.ResponseHeadersRead);

    // Assert
    response.EnsureSuccessStatusCode();
    response.Content.Headers.ContentType.MediaType.Should().Be("text/event-stream");

    using var stream = await response.Content.ReadAsStreamAsync();
    using var reader = new StreamReader(stream);

    var chunks = new List<string>();
    string? line;
    while ((line = await reader.ReadLineAsync()) != null)
    {
        if (line.StartsWith("data: ") && line != "data: [DONE]")
        {
            chunks.Add(line);
        }
    }

    chunks.Should().NotBeEmpty();
}
```

## Resources

- [Server-Sent Events (SSE) Specification](https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events)
- [OpenAI Streaming Documentation](https://platform.openai.com/docs/api-reference/streaming)
