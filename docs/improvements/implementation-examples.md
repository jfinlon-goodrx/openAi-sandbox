# Implementation Examples

Examples showing how to use the new middleware and features.

## Quick Wins Implementation

### 1. Correlation IDs

Add to `Program.cs`:

```csharp
using Shared.Common;

var app = builder.Build();

// Add correlation ID middleware (early in pipeline)
app.UseCorrelationId();

// Access in controllers/services
app.MapGet("/api/test", (HttpContext context) =>
{
    var correlationId = context.GetCorrelationId();
    return Results.Ok(new { CorrelationId = correlationId });
});
```

### 2. Request/Response Logging

```csharp
app.UseRequestResponseLogging();
```

### 3. CORS Configuration

```csharp
using Shared.Common;

// In Program.cs
builder.Services.AddOpenAICors(builder.Configuration);

// After app.Build()
app.UseOpenAICors();
```

### 4. Response Compression

```csharp
app.UseResponseCompression();
```

### 5. Health Checks

```csharp
using Shared.Common;

builder.Services.AddOpenAIHealthChecks();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

### 6. Rate Limiting

```csharp
app.UseRateLimiting(options =>
{
    options.MaxRequests = 100;
    options.WindowSeconds = 60;
    options.KeySelector = context => context.GetCorrelationId() ?? "anonymous";
});
```

### 7. API Key Authentication

```csharp
using Shared.Common;

builder.Services.AddAuthentication("ApiKey")
    .AddApiKeyAuthentication(builder.Configuration);

builder.Services.AddAuthorization();

app.UseAuthentication();
app.UseAuthorization();

// Protect endpoints
app.MapGet("/api/protected", () => "Protected")
    .RequireAuthorization();
```

## Complete Example Program.cs

```csharp
using Shared.Common;
using OpenAIShared;
using RequirementsAssistant.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddOpenAIServices(builder.Configuration);
builder.Services.AddOpenAIHealthChecks();
builder.Services.AddOpenAICors(builder.Configuration);

// Authentication
builder.Services.AddAuthentication("ApiKey")
    .AddApiKeyAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware pipeline (order matters!)
app.UseCorrelationId();
app.UseRequestResponseLogging();
app.UseResponseCompression();
app.UseOpenAICors();
app.UseRateLimiting();
app.UseAuthentication();
app.UseAuthorization();

// Health checks
app.MapHealthChecks("/health");

// Controllers
app.MapControllers();

app.Run();
```

## Configuration (appsettings.json)

```json
{
  "ApiKeys": {
    "Default": "your-api-key-here"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5000"
    ]
  }
}
```
