# Error Handling Best Practices

**For:** Developers building production applications with OpenAI Platform.

**What you'll learn:** How to handle API errors gracefully, implement retry logic, use circuit breakers, provide fallback mechanisms, and ensure your application remains stable when external services have issues.

## Overview

Robust error handling is essential for production applications. This guide covers best practices for handling errors from OpenAI Platform APIs and ensuring your application remains stable and user-friendly.

**Why proper error handling matters:**
- **User experience:** Users get helpful error messages instead of crashes
- **Reliability:** Applications continue working even when external services have issues
- **Cost control:** Prevent runaway costs from retry loops
- **Debugging:** Better error messages help identify and fix issues quickly
- **Resilience:** Applications can recover from temporary failures automatically

## Common Error Types

### 1. API Errors

```csharp
try
{
    var response = await openAIClient.GetChatCompletionAsync(request);
}
catch (HttpRequestException ex) when (ex.Message.Contains("401"))
{
    // Unauthorized - check API key
    _logger.LogError("Invalid API key");
    throw new InvalidOperationException("API key is invalid or expired", ex);
}
catch (HttpRequestException ex) when (ex.Message.Contains("429"))
{
    // Rate limit exceeded
    _logger.LogWarning("Rate limit exceeded, retrying...");
    await Task.Delay(TimeSpan.FromSeconds(60));
    // Retry logic
}
catch (HttpRequestException ex) when (ex.Message.Contains("500"))
{
    // Server error
    _logger.LogError("OpenAI API server error: {Error}", ex.Message);
    throw new ServiceUnavailableException("OpenAI service temporarily unavailable", ex);
}
```

### 2. Timeout Errors

```csharp
try
{
    var response = await openAIClient.GetChatCompletionAsync(request);
}
catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
{
    _logger.LogWarning("Request timed out after {Timeout}s", requestTimeout);
    // Implement fallback or retry
}
```

### 3. Invalid Response Errors

```csharp
try
{
    var response = await openAIClient.GetChatCompletionAsync(request);
    var content = response.Choices.FirstOrDefault()?.Message?.Content;
    
    if (string.IsNullOrEmpty(content))
    {
        throw new InvalidOperationException("Empty response from OpenAI API");
    }
}
catch (JsonException ex)
{
    _logger.LogError(ex, "Failed to parse OpenAI API response");
    throw new InvalidOperationException("Invalid response format from OpenAI API", ex);
}
```

## Retry Logic

The `OpenAIClient` includes built-in retry logic, but you can add custom retry:

```csharp
public async Task<T> RetryWithBackoffAsync<T>(
    Func<Task<T>> operation,
    int maxRetries = 3)
{
    for (int attempt = 0; attempt < maxRetries; attempt++)
    {
        try
        {
            return await operation();
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("429") && attempt < maxRetries - 1)
        {
            var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
            _logger.LogWarning("Retry attempt {Attempt} after {Delay}s", attempt + 1, delay.TotalSeconds);
            await Task.Delay(delay);
        }
    }
    
    throw new InvalidOperationException("Max retries exceeded");
}
```

## Fallback Strategies

### Fallback to Simpler Model

```csharp
public async Task<string> GetResponseWithFallback(ChatCompletionRequest request)
{
    try
    {
        request.Model = "gpt-4-turbo-preview";
        var response = await openAIClient.GetChatCompletionAsync(request);
        return response.Choices.First().Message.Content;
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "GPT-4 failed, falling back to GPT-3.5");
        
        request.Model = "gpt-3.5-turbo";
        var response = await openAIClient.GetChatCompletionAsync(request);
        return response.Choices.First().Message.Content;
    }
}
```

### Fallback to Cached Response

```csharp
public async Task<string> GetResponseWithCache(string input)
{
    // Try cache first
    if (_cache.TryGetValue(input, out string cached))
    {
        return cached;
    }

    try
    {
        var response = await GetAIResponse(input);
        _cache.Set(input, response, TimeSpan.FromHours(24));
        return response;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "AI request failed");
        
        // Return cached response even if stale
        if (_cache.TryGetValue(input, out string stale))
        {
            _logger.LogWarning("Returning stale cached response");
            return stale;
        }
        
        throw;
    }
}
```

## Validation

### Input Validation

```csharp
public void ValidateRequest(ChatCompletionRequest request)
{
    if (request == null)
        throw new ArgumentNullException(nameof(request));

    if (request.Messages == null || !request.Messages.Any())
        throw new ArgumentException("Messages cannot be empty", nameof(request));

    if (string.IsNullOrEmpty(request.Model))
        throw new ArgumentException("Model must be specified", nameof(request));

    // Validate token limits
    var totalTokens = request.Messages.Sum(m => TokenCounter.EstimateTokenCount(m.Content ?? ""));
    if (totalTokens > 32000) // Context window limit
        throw new ArgumentException("Input exceeds token limit", nameof(request));
}
```

### Response Validation

```csharp
public void ValidateResponse(ChatCompletionResponse response)
{
    if (response == null)
        throw new InvalidOperationException("Response is null");

    if (response.Choices == null || !response.Choices.Any())
        throw new InvalidOperationException("No choices in response");

    var choice = response.Choices.First();
    if (choice.Message == null || string.IsNullOrEmpty(choice.Message.Content))
        throw new InvalidOperationException("Empty message content");
}
```

## Logging

### Structured Logging

```csharp
_logger.LogInformation(
    "OpenAI API call - Model: {Model}, Tokens: {Tokens}, Cost: {Cost}, Duration: {Duration}ms",
    request.Model,
    response.Usage?.TotalTokens ?? 0,
    CostCalculator.FormatCost(cost),
    stopwatch.ElapsedMilliseconds
);
```

### Error Logging

```csharp
catch (Exception ex)
{
    _logger.LogError(ex,
        "Error processing OpenAI request - Model: {Model}, InputLength: {InputLength}",
        request.Model,
        request.Messages.Sum(m => m.Content?.Length ?? 0)
    );
    throw;
}
```

## Circuit Breaker Pattern

```csharp
public class CircuitBreaker
{
    private int _failureCount = 0;
    private DateTime? _lastFailureTime;
    private readonly int _threshold = 5;
    private readonly TimeSpan _timeout = TimeSpan.FromMinutes(1);

    public bool IsOpen()
    {
        if (_failureCount < _threshold)
            return false;

        if (_lastFailureTime.HasValue && 
            DateTime.UtcNow - _lastFailureTime.Value > _timeout)
        {
            Reset();
            return false;
        }

        return true;
    }

    public void RecordFailure()
    {
        _failureCount++;
        _lastFailureTime = DateTime.UtcNow;
    }

    public void RecordSuccess()
    {
        Reset();
    }

    private void Reset()
    {
        _failureCount = 0;
        _lastFailureTime = null;
    }
}
```

## Best Practices Summary

1. **Handle Specific Exceptions**: Catch specific exception types
2. **Implement Retries**: Use exponential backoff for transient errors
3. **Provide Fallbacks**: Have backup strategies
4. **Validate Input/Output**: Always validate requests and responses
5. **Log Everything**: Log errors with context
6. **Use Circuit Breakers**: Prevent cascading failures
7. **Set Timeouts**: Don't wait indefinitely
8. **Monitor**: Track error rates and patterns

## Resources

- [Polly Retry Policies](https://github.com/App-vNext/Polly)
- [.NET Error Handling](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/)
- [OpenAI Error Codes](https://platform.openai.com/docs/guides/error-codes)
