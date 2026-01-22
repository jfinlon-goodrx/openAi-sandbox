# Usage Examples for New Features

Practical examples showing how to use all the new improvements.

## Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/OpenAIShared.Tests/

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Writing Tests

```csharp
public class MyServiceTests
{
    [Fact]
    public async Task MyMethod_WithValidInput_ReturnsExpectedResult()
    {
        // Arrange
        var mockHttpClient = OpenAIMockHelper.CreateMockHttpClient(
            OpenAIMockHelper.CreateChatCompletionResponse("Test response")
        );
        var client = new OpenAIClient(mockHttpClient, logger, config);

        // Act
        var result = await client.GetChatCompletionAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Choices.First().Message.Content.Should().Be("Test response");
    }
}
```

## Streaming

### Server-Side Streaming Endpoint

```csharp
[HttpPost("stream")]
public async Task Stream([FromBody] StreamRequest request, CancellationToken cancellationToken)
{
    var chatRequest = new ChatCompletionRequest
    {
        Model = "gpt-4-turbo-preview",
        Messages = new List<ChatMessage>
        {
            new() { Role = "user", Content = request.Prompt }
        }
    };

    await _streamingService.StreamChatCompletionToHttpResponseAsync(chatRequest, Response, cancellationToken);
}
```

### Client-Side JavaScript

```javascript
async function streamResponse(prompt) {
    const response = await fetch('/api/stream', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ prompt })
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

                const parsed = JSON.parse(data);
                result += parsed.content || '';
                updateUI(result);
            }
        }
    }

    return result;
}
```

## Caching

### Using Caching Service

```csharp
public class CachedRequirementsService
{
    private readonly RequirementsService _requirementsService;
    private readonly CachingService _cachingService;

    public async Task<string> SummarizeAsync(string content)
    {
        var cacheKey = CachingService.GenerateCacheKey(content, "gpt-4-turbo-preview");

        return await _cachingService.GetOrSetAsync(
            cacheKey,
            async () => await _requirementsService.SummarizeDocumentAsync(content),
            TimeSpan.FromHours(24)
        );
    }
}
```

## Rate Limiting

### Configure Rate Limiting

```csharp
app.UseRateLimiting(options =>
{
    // 100 requests per minute per IP
    options.MaxRequests = 100;
    options.WindowSeconds = 60;
    
    // Or per API key
    options.KeySelector = context => 
        context.Request.Headers["X-API-Key"].FirstOrDefault() ?? 
        context.Connection.RemoteIpAddress?.ToString() ?? 
        "anonymous";
});
```

### Handle Rate Limit Responses

```csharp
[HttpGet("test")]
public IActionResult Test()
{
    // Check rate limit headers
    Response.Headers.TryGetValue("X-RateLimit-Remaining", out var remaining);
    
    return Ok(new 
    { 
        Message = "Success",
        RemainingRequests = remaining.ToString()
    });
}
```

## Authentication

### Protect Endpoints

```csharp
[HttpGet("protected")]
[Authorize]
public IActionResult Protected()
{
    return Ok("This endpoint requires authentication");
}

// Or use minimal API
app.MapGet("/api/protected", () => "Protected")
    .RequireAuthorization();
```

### Test with API Key

```bash
curl -X GET http://localhost:5001/api/protected \
  -H "X-API-Key: your-api-key-here"
```

## Health Checks

### Basic Health Check

```bash
curl http://localhost:5001/health
```

### Readiness Check

```bash
curl http://localhost:5001/health/ready
```

### Custom Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("openai", () => HealthCheckResult.Healthy("OpenAI API is accessible"))
    .AddCheck("database", () => HealthCheckResult.Healthy("Database is connected"));
```

## Correlation IDs

### Access in Controllers

```csharp
[HttpGet("test")]
public IActionResult Test(HttpContext context)
{
    var correlationId = context.GetCorrelationId();
    return Ok(new { CorrelationId = correlationId });
}
```

### Use in Logging

```csharp
_logger.LogInformation(
    "Processing request. CorrelationId: {CorrelationId}",
    HttpContext.GetCorrelationId()
);
```

## Complete Example API

See [MiddlewareExamples/Program.cs](../../samples/MiddlewareExamples/Program.cs) for a complete example showing all middleware in use.
