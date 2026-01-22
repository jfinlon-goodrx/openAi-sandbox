# Middleware Guide

Complete guide to using the new middleware and features.

## Available Middleware

### 1. Correlation ID Middleware

Adds correlation IDs to requests for tracking across services.

**Usage:**
```csharp
app.UseCorrelationId();
```

**Access in code:**
```csharp
var correlationId = HttpContext.GetCorrelationId();
```

**Headers:**
- Request: `X-Correlation-ID` (optional)
- Response: `X-Correlation-ID` (always added)

### 2. Request/Response Logging Middleware

Logs all HTTP requests and responses with correlation IDs.

**Usage:**
```csharp
app.UseRequestResponseLogging();
```

**Logs:**
- Request method, path, query string
- Response status code, duration
- Request/response bodies (for small requests < 1KB)

### 3. Response Compression Middleware

Compresses responses using GZip or Brotli.

**Usage:**
```csharp
app.UseResponseCompression();
```

**Supports:**
- GZip compression
- Brotli compression
- Automatic selection based on `Accept-Encoding` header

### 4. CORS Configuration

Configures Cross-Origin Resource Sharing.

**Usage:**
```csharp
builder.Services.AddOpenAICors(builder.Configuration);
app.UseOpenAICors();
```

**Configuration (appsettings.json):**
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5000"
    ]
  }
}
```

### 5. Rate Limiting Middleware

Limits requests per time window using token bucket algorithm.

**Usage:**
```csharp
app.UseRateLimiting(options =>
{
    options.MaxRequests = 100;
    options.WindowSeconds = 60;
    options.KeySelector = context => context.GetCorrelationId() ?? "anonymous";
});
```

**Headers:**
- `X-RateLimit-Limit`: Maximum requests
- `X-RateLimit-Remaining`: Remaining requests
- `Retry-After`: Seconds until refill (when limit exceeded)

**Response:** 429 Too Many Requests when limit exceeded

### 6. API Key Authentication

Simple API key authentication middleware.

**Usage:**
```csharp
builder.Services.AddAuthentication("ApiKey")
    .AddApiKeyAuthentication(builder.Configuration);

app.UseAuthentication();
app.UseAuthorization();
```

**Configuration (appsettings.json):**
```json
{
  "ApiKeys": {
    "Default": "your-api-key-here"
  }
}
```

**Request Header:**
```
X-API-Key: your-api-key-here
```

**Protect endpoints:**
```csharp
app.MapGet("/api/protected", () => "Protected")
    .RequireAuthorization();
```

### 7. Health Checks

Health check endpoints for monitoring.

**Usage:**
```csharp
builder.Services.AddOpenAIHealthChecks();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

**Endpoints:**
- `/health` - Basic health check
- `/health/ready` - Readiness check (with tags)

## Complete Example

See [MiddlewareExamples/Program.cs](../../samples/MiddlewareExamples/Program.cs) for a complete example.

## Middleware Order

The order of middleware matters. Recommended order:

1. Correlation ID (first - adds ID for all subsequent middleware)
2. Request/Response Logging (early - logs everything)
3. Response Compression (before other middleware that write responses)
4. CORS (before authentication)
5. Rate Limiting (before expensive operations)
6. HTTPS Redirection
7. Authentication
8. Authorization
9. Controllers/Endpoints

## Best Practices

1. **Order Matters**: Place middleware in the correct order
2. **Configuration**: Use `appsettings.json` for configuration
3. **Environment-Specific**: Some middleware should only run in production
4. **Performance**: Consider performance impact of logging middleware
5. **Security**: Always use HTTPS in production

## Troubleshooting

**Issue:** Correlation ID not appearing
- **Solution:** Ensure `UseCorrelationId()` is first in the pipeline

**Issue:** Rate limiting too aggressive
- **Solution:** Adjust `MaxRequests` and `WindowSeconds` in configuration

**Issue:** CORS not working
- **Solution:** Check `AllowedOrigins` configuration and ensure `UseOpenAICors()` is before `UseAuthorization()`

**Issue:** Authentication not working
- **Solution:** Ensure `UseAuthentication()` is before `UseAuthorization()`
