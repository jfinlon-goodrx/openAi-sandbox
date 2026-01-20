# Security Best Practices

## API Key Management

### Never Commit API Keys

**Bad:**
```json
{
  "OpenAI": {
    "ApiKey": "sk-1234567890abcdef"
  }
}
```

**Good:**
- Use environment variables
- Use .NET User Secrets for development
- Use Azure Key Vault for production
- Use GitHub Secrets for CI/CD

### Environment Variables

```bash
export OpenAI__ApiKey="sk-your-key-here"
```

### User Secrets (Development)

```bash
dotnet user-secrets init
dotnet user-secrets set "OpenAI:ApiKey" "sk-your-key-here"
```

### Azure Key Vault (Production)

```csharp
builder.Configuration.AddAzureKeyVault(
    keyVaultUrl,
    new DefaultAzureCredential()
);
```

## Data Privacy

### Don't Send Sensitive Data

**Bad:**
```csharp
var request = new ChatCompletionRequest
{
    Messages = new List<ChatMessage>
    {
        new() { Content = $"User SSN: {ssn}, Credit Card: {cardNumber}" }
    }
};
```

**Good:**
```csharp
// Sanitize data before sending
var sanitizedContent = SanitizeInput(userInput);
var request = new ChatCompletionRequest
{
    Messages = new List<ChatMessage>
    {
        new() { Content = sanitizedContent }
    }
};
```

### Content Filtering

Implement content filtering before sending to API:

```csharp
public static string SanitizeInput(string input)
{
    // Remove PII
    input = Regex.Replace(input, @"\b\d{3}-\d{2}-\d{4}\b", "[SSN]");
    input = Regex.Replace(input, @"\b\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}\b", "[CARD]");
    
    return input;
}
```

## Rate Limiting

Implement rate limiting to prevent abuse:

```csharp
public class RateLimiter
{
    private readonly Dictionary<string, Queue<DateTime>> _requests = new();
    private readonly int _maxRequests;
    private readonly TimeSpan _window;

    public bool IsAllowed(string key)
    {
        var now = DateTime.UtcNow;
        
        if (!_requests.ContainsKey(key))
        {
            _requests[key] = new Queue<DateTime>();
        }

        var requests = _requests[key];
        
        // Remove old requests
        while (requests.Count > 0 && now - requests.Peek() > _window)
        {
            requests.Dequeue();
        }

        if (requests.Count >= _maxRequests)
        {
            return false;
        }

        requests.Enqueue(now);
        return true;
    }
}
```

## Input Validation

Always validate input:

```csharp
public async Task<string> ProcessInput(string input)
{
    if (string.IsNullOrWhiteSpace(input))
    {
        throw new ArgumentException("Input cannot be empty");
    }

    if (input.Length > 100000) // Token limit
    {
        throw new ArgumentException("Input too long");
    }

    // Sanitize
    input = SanitizeInput(input);
    
    // Process
    return await ProcessWithAI(input);
}
```

## Audit Logging

Log all API calls for audit purposes:

```csharp
public class AuditLogger
{
    public void LogApiCall(string endpoint, string userId, int tokensUsed)
    {
        _logger.LogInformation(
            "API Call - Endpoint: {Endpoint}, User: {UserId}, Tokens: {Tokens}, Time: {Time}",
            endpoint,
            userId,
            tokensUsed,
            DateTime.UtcNow
        );
    }
}
```

## HTTPS Only

Always use HTTPS in production:

```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
```

## CORS Configuration

Configure CORS properly:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

## Error Handling

Don't expose sensitive information in errors:

**Bad:**
```csharp
catch (Exception ex)
{
    return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
}
```

**Good:**
```csharp
catch (Exception ex)
{
    _logger.LogError(ex, "Error processing request");
    return StatusCode(500, new { error = "An error occurred. Please try again." });
}
```

## Compliance

### GDPR Compliance

- Don't store personal data unnecessarily
- Implement data retention policies
- Allow users to delete their data
- Get consent before processing

### Enterprise Data Policies

- Ensure data is not used for training (Enterprise API)
- Use appropriate data retention
- Follow company data handling policies

## Resources

- [OpenAI Security Best Practices](https://platform.openai.com/docs/guides/safety-best-practices)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [.NET Security Documentation](https://learn.microsoft.com/en-us/dotnet/standard/security/)
