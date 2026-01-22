# Metrics and Monitoring Guide

Complete guide to metrics collection and monitoring.

## Overview

The MetricsService tracks OpenAI API usage including:
- Total API calls per model
- Token usage (prompt + completion)
- Cost tracking
- Response times
- Model usage statistics

## Setup

MetricsService is automatically registered as a singleton:

```csharp
builder.Services.AddOpenAIServices(builder.Configuration);
// MetricsService is automatically registered
```

## Usage

### Recording Metrics

Metrics are automatically recorded when using OpenAIClient (if configured). You can also record manually:

```csharp
_metricsService.RecordApiCall(
    model: "gpt-4-turbo-preview",
    promptTokens: 1000,
    completionTokens: 500,
    duration: TimeSpan.FromMilliseconds(1500),
    cost: 0.05m
);
```

### Getting Metrics

```csharp
// Get summary
var summary = _metricsService.GetSummary();
Console.WriteLine($"Total Calls: {summary.TotalCalls}");
Console.WriteLine($"Total Cost: ${summary.TotalCost}");
Console.WriteLine($"Total Tokens: {summary.TotalTokens}");

// Get metrics for specific model
var gpt4Metrics = _metricsService.GetMetrics("gpt-4-turbo-preview");
if (gpt4Metrics != null)
{
    Console.WriteLine($"GPT-4 Calls: {gpt4Metrics.TotalCalls}");
    Console.WriteLine($"GPT-4 Cost: ${gpt4Metrics.TotalCost}");
}

// Get all metrics
var allMetrics = _metricsService.GetAllMetrics();
```

### API Endpoints

Access metrics via HTTP:

```bash
# Get all metrics
curl http://localhost:5001/api/metrics

# Get metrics for specific model
curl http://localhost:5001/api/metrics/gpt-4-turbo-preview

# Reset metrics
curl -X POST http://localhost:5001/api/metrics/reset
```

## Metrics Data Structure

### Summary

```json
{
  "summary": {
    "totalCalls": 150,
    "totalCost": 12.50,
    "totalTokens": 50000,
    "averageDuration": "00:00:01.500",
    "modelsUsed": ["gpt-4-turbo-preview", "gpt-3.5-turbo"]
  },
  "byModel": {
    "gpt-4-turbo-preview": {
      "model": "gpt-4-turbo-preview",
      "totalCalls": 100,
      "totalPromptTokens": 30000,
      "totalCompletionTokens": 15000,
      "totalCost": 10.00,
      "totalDuration": "00:02:30",
      "totalTokens": 45000,
      "averageCost": 0.10,
      "averageDuration": "00:00:01.500"
    }
  }
}
```

## Integration with Health Checks

You can add metrics to health checks:

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("openai-metrics", () =>
    {
        var metrics = metricsService.GetSummary();
        if (metrics.TotalCalls > 1000)
        {
            return HealthCheckResult.Degraded("High API call volume");
        }
        return HealthCheckResult.Healthy();
    });
```

## Best Practices

1. **Monitor Costs**: Track total cost to stay within budget
2. **Track Usage**: Monitor token usage to optimize prompts
3. **Performance**: Track response times to identify slow operations
4. **Model Selection**: Compare metrics across models to optimize costs
5. **Reset Periodically**: Reset metrics daily/weekly for reporting periods

## Exporting Metrics

### To Application Insights

```csharp
// In your service
var summary = _metricsService.GetSummary();
_telemetryClient.TrackMetric("OpenAI.TotalCalls", summary.TotalCalls);
_telemetryClient.TrackMetric("OpenAI.TotalCost", (double)summary.TotalCost);
```

### To Prometheus

```csharp
// Create Prometheus metrics
var totalCallsGauge = Metrics.CreateGauge("openai_total_calls", "Total OpenAI API calls");
var totalCostGauge = Metrics.CreateGauge("openai_total_cost", "Total OpenAI API cost");

// Update metrics
var summary = _metricsService.GetSummary();
totalCallsGauge.Set(summary.TotalCalls);
totalCostGauge.Set((double)summary.TotalCost);
```

## Resources

- [Application Insights](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)
- [Prometheus](https://prometheus.io/)
