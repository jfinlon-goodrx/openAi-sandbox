using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace OpenAIShared;

/// <summary>
/// Service for tracking OpenAI API metrics
/// </summary>
public class MetricsService
{
    private readonly ILogger<MetricsService> _logger;
    private readonly Dictionary<string, ApiCallMetrics> _metrics = new();

    public MetricsService(ILogger<MetricsService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Records an API call
    /// </summary>
    public void RecordApiCall(string model, int promptTokens, int completionTokens, TimeSpan duration, decimal cost)
    {
        var key = model;
        if (!_metrics.ContainsKey(key))
        {
            _metrics[key] = new ApiCallMetrics { Model = model };
        }

        var metric = _metrics[key];
        metric.TotalCalls++;
        metric.TotalPromptTokens += promptTokens;
        metric.TotalCompletionTokens += completionTokens;
        metric.TotalCost += cost;
        metric.TotalDuration += duration;

        _logger.LogInformation(
            "API Call - Model: {Model}, Tokens: {PromptTokens}+{CompletionTokens}, Cost: {Cost}, Duration: {Duration}ms",
            model,
            promptTokens,
            completionTokens,
            CostCalculator.FormatCost(cost),
            duration.TotalMilliseconds);
    }

    /// <summary>
    /// Gets metrics for a specific model
    /// </summary>
    public ApiCallMetrics? GetMetrics(string model)
    {
        return _metrics.TryGetValue(model, out var metric) ? metric : null;
    }

    /// <summary>
    /// Gets all metrics
    /// </summary>
    public Dictionary<string, ApiCallMetrics> GetAllMetrics()
    {
        return new Dictionary<string, ApiCallMetrics>(_metrics);
    }

    /// <summary>
    /// Gets summary statistics
    /// </summary>
    public MetricsSummary GetSummary()
    {
        var totalCalls = _metrics.Values.Sum(m => m.TotalCalls);
        var totalCost = _metrics.Values.Sum(m => m.TotalCost);
        var totalTokens = _metrics.Values.Sum(m => m.TotalPromptTokens + m.TotalCompletionTokens);
        var avgDuration = _metrics.Values.Any() 
            ? TimeSpan.FromMilliseconds(_metrics.Values.Average(m => m.TotalDuration.TotalMilliseconds))
            : TimeSpan.Zero;

        return new MetricsSummary
        {
            TotalCalls = totalCalls,
            TotalCost = totalCost,
            TotalTokens = totalTokens,
            AverageDuration = avgDuration,
            ModelsUsed = _metrics.Keys.ToList()
        };
    }

    /// <summary>
    /// Resets all metrics
    /// </summary>
    public void Reset()
    {
        _metrics.Clear();
        _logger.LogInformation("Metrics reset");
    }
}

/// <summary>
/// API call metrics for a specific model
/// </summary>
public class ApiCallMetrics
{
    public string Model { get; set; } = string.Empty;
    public int TotalCalls { get; set; }
    public int TotalPromptTokens { get; set; }
    public int TotalCompletionTokens { get; set; }
    public decimal TotalCost { get; set; }
    public TimeSpan TotalDuration { get; set; }

    public int TotalTokens => TotalPromptTokens + TotalCompletionTokens;
    public decimal AverageCost => TotalCalls > 0 ? TotalCost / TotalCalls : 0;
    public TimeSpan AverageDuration => TotalCalls > 0 ? TimeSpan.FromMilliseconds(TotalDuration.TotalMilliseconds / TotalCalls) : TimeSpan.Zero;
}

/// <summary>
/// Summary of all metrics
/// </summary>
public class MetricsSummary
{
    public int TotalCalls { get; set; }
    public decimal TotalCost { get; set; }
    public int TotalTokens { get; set; }
    public TimeSpan AverageDuration { get; set; }
    public List<string> ModelsUsed { get; set; } = new();
}
