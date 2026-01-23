# Serilog Structured Logging Guide

**For:** Developers who need production-ready logging with structured data and easy querying.

**What you'll learn:** How to implement structured logging with Serilog, configure log sinks, format logs as JSON, and query logs effectively.

## Overview

Complete guide to implementing structured logging with Serilog.

Serilog provides structured logging with JSON output, making logs easier to query and analyze.

**Why use structured logging?**
- **Queryability:** Search and filter logs by specific fields
- **Analysis:** Easy to analyze log data programmatically
- **Debugging:** Rich context in logs helps identify issues quickly
- **Monitoring:** Integrate with log aggregation tools (ELK, Splunk, etc.)
- **Production-ready:** Industry-standard logging solution

## Setup

### 1. Add Package Reference

Already included in `shared/Common/Common.csproj`:
- `Serilog.AspNetCore`
- `Serilog.Formatting.Compact`
- `Serilog.Sinks.File`

### 2. Configure in Program.cs

```csharp
using Shared.Common;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.AddSerilogLogging();

// ... rest of configuration

var app = builder.Build();

// Add Serilog request logging
app.UseSerilogRequestLogging();

app.Run();
```

### 3. Configuration (appsettings.json)

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

## Usage

### Basic Logging

```csharp
_logger.LogInformation("User {UserId} performed action {Action}", userId, action);
```

### Structured Logging

```csharp
_logger.LogInformation(
    "Processing request. UserId: {UserId}, Action: {Action}, Duration: {Duration}ms",
    userId,
    action,
    duration);
```

### Log Levels

```csharp
_logger.LogTrace("Very detailed information");
_logger.LogDebug("Debug information");
_logger.LogInformation("General information");
_logger.LogWarning("Warning message");
_logger.LogError(exception, "Error occurred");
_logger.LogCritical(exception, "Critical error");
```

### With Correlation IDs

```csharp
_logger.LogInformation(
    "Request processed. CorrelationId: {CorrelationId}, Status: {Status}",
    HttpContext.GetCorrelationId(),
    statusCode);
```

## Output Formats

### Console (Compact JSON)

```json
{
  "@t": "2024-01-15T10:30:00Z",
  "@l": "Information",
  "@mt": "User {UserId} performed action {Action}",
  "UserId": "user-123",
  "Action": "login",
  "CorrelationId": "abc-123"
}
```

### File Output

Logs are written to `logs/log-YYYYMMDD.txt` with compact JSON format.

## Querying Logs

### Using jq (Command Line)

```bash
# Find all errors
cat logs/log-20240115.txt | jq 'select(.@l == "Error")'

# Find requests taking longer than 1 second
cat logs/log-20240115.txt | jq 'select(.Elapsed > 1000)'

# Find requests by correlation ID
cat logs/log-20240115.txt | jq 'select(.CorrelationId == "abc-123")'
```

### Using Application Insights

Serilog can be configured to send logs to Application Insights:

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces)
    .CreateLogger();
```

## Best Practices

1. **Use Structured Properties**: Always use structured properties, not string interpolation
2. **Include Context**: Add correlation IDs, user IDs, request IDs
3. **Appropriate Levels**: Use correct log levels
4. **Don't Log Sensitive Data**: Never log passwords, API keys, or PII
5. **Performance**: Use appropriate log levels in production

## Example: Complete Logging Setup

```csharp
public class MyService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public async Task ProcessAsync(string userId, string action)
    {
        _logger.LogInformation("Starting processing. UserId: {UserId}, Action: {Action}", userId, action);

        try
        {
            // Process...
            _logger.LogInformation("Processing completed successfully. UserId: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Processing failed. UserId: {UserId}, Action: {Action}", userId, action);
            throw;
        }
    }
}
```

## Resources

- [Serilog Documentation](https://serilog.net/)
- [Structured Logging Concepts](https://github.com/serilog/serilog/wiki/Structured-Data)
