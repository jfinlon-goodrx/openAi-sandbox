# Cost Optimization

**For:** All users of OpenAI Platform who want to minimize costs while maintaining quality.

**What you'll learn:** How to optimize costs through model selection, token management, caching, batch processing, and monitoring usage to stay within budget.

## Overview

OpenAI Platform usage is charged per token. This guide shows you how to optimize costs while maintaining the quality of your AI-powered applications.

**Why optimize costs?**
- **Budget management:** Stay within allocated budgets
- **Scalability:** Make AI applications cost-effective at scale
- **Efficiency:** Get the same results for less cost
- **ROI:** Maximize return on investment in AI capabilities
- **Sustainability:** Enable long-term use of AI in your organization

## Understanding Costs

### Model Pricing (as of 2024)

| Model | Input (per 1M tokens) | Output (per 1M tokens) |
|-------|----------------------|-------------------------|
| GPT-4 Turbo | $10 | $30 |
| GPT-4 | $30 | $60 |
| GPT-3.5 Turbo | $0.50 | $1.50 |
| Whisper | $0.006 per minute | - |
| Embeddings | $0.10 | - |

### Token Estimation

```csharp
using OpenAIShared;

var text = "Your text here";
var tokens = TokenCounter.EstimateTokenCount(text);
var cost = CostCalculator.CalculateCost("gpt-4-turbo-preview", tokens);
Console.WriteLine($"Estimated cost: {CostCalculator.FormatCost(cost)}");
```

## Cost Optimization Strategies

### 1. Choose the Right Model

**Use GPT-3.5 Turbo for:**
- Simple tasks
- High-volume operations
- Non-critical applications

**Use GPT-4 for:**
- Complex reasoning
- Code generation
- Critical applications

```csharp
// Simple summarization - use GPT-3.5
var simpleRequest = new ChatCompletionRequest
{
    Model = "gpt-3.5-turbo", // Cheaper
    Messages = messages
};

// Complex code review - use GPT-4
var complexRequest = new ChatCompletionRequest
{
    Model = "gpt-4-turbo-preview", // More accurate
    Messages = messages
};
```

### 2. Limit Max Tokens

Set appropriate `MaxTokens` to control costs:

```csharp
var request = new ChatCompletionRequest
{
    Model = "gpt-4-turbo-preview",
    Messages = messages,
    MaxTokens = 500 // Limit response length
};
```

### 3. Cache Responses

Cache responses for identical inputs:

```csharp
public class ResponseCache
{
    private readonly IMemoryCache _cache;

    public async Task<string> GetCachedResponse(string input)
    {
        var cacheKey = $"response_{input.GetHashCode()}";
        
        if (_cache.TryGetValue(cacheKey, out string cached))
        {
            return cached;
        }

        var response = await GetAIResponse(input);
        _cache.Set(cacheKey, response, TimeSpan.FromHours(24));
        
        return response;
    }
}
```

### 4. Batch Requests

Batch similar requests:

```csharp
// Instead of multiple calls
foreach (var item in items)
{
    await ProcessItem(item); // Expensive
}

// Batch process
var batchPrompt = string.Join("\n", items.Select(i => $"Item: {i}"));
var batchResponse = await ProcessBatch(batchPrompt); // More efficient
```

### 5. Use Streaming for Long Responses

Stream responses to show progress and potentially cancel early:

```csharp
await foreach (var chunk in openAIClient.GetChatCompletionStreamAsync(request))
{
    Console.Write(chunk);
    // Can cancel if user stops reading
}
```

### 6. Optimize Prompts

Shorter, more focused prompts = lower costs:

**Bad (verbose):**
```
Please analyze the following code very carefully and provide a comprehensive review
covering all aspects including security, performance, code style, best practices,
potential bugs, and suggestions for improvement. Be thorough and detailed.
```

**Good (concise):**
```
Review this code for security, performance, and bugs:
```

### 7. Use Function Calling for Structured Output

Function calling can be more efficient than parsing text:

```csharp
var functionDefinition = new FunctionDefinition
{
    Name = "extract_data",
    Description = "Extracts structured data",
    Parameters = new { /* schema */ }
};

var request = new ChatCompletionRequest
{
    Functions = new List<FunctionDefinition> { functionDefinition },
    FunctionCall = "auto"
};
```

## Monitoring Costs

### Track Usage

```csharp
public class CostTracker
{
    private readonly Dictionary<string, decimal> _costs = new();

    public void TrackUsage(string model, int inputTokens, int outputTokens)
    {
        var cost = CostCalculator.CalculateCost(model, inputTokens, outputTokens);
        
        if (!_costs.ContainsKey(model))
        {
            _costs[model] = 0;
        }
        
        _costs[model] += cost;
    }

    public decimal GetTotalCost() => _costs.Values.Sum();
    
    public void LogCosts()
    {
        foreach (var kvp in _costs)
        {
            _logger.LogInformation("Model: {Model}, Cost: {Cost}", 
                kvp.Key, CostCalculator.FormatCost(kvp.Value));
        }
    }
}
```

### Set Budget Alerts

```csharp
public class BudgetMonitor
{
    private readonly decimal _dailyBudget;
    private decimal _dailySpent;

    public bool IsWithinBudget(decimal estimatedCost)
    {
        return _dailySpent + estimatedCost <= _dailyBudget;
    }

    public void RecordSpending(decimal cost)
    {
        _dailySpent += cost;
        
        if (_dailySpent >= _dailyBudget * 0.9m)
        {
            _logger.LogWarning("Approaching daily budget: {Spent} / {Budget}", 
                _dailySpent, _dailyBudget);
        }
    }
}
```

## Cost Optimization Checklist

- [ ] Use GPT-3.5 for simple tasks
- [ ] Set appropriate MaxTokens limits
- [ ] Cache responses when possible
- [ ] Batch similar requests
- [ ] Optimize prompts (shorter = cheaper)
- [ ] Monitor costs regularly
- [ ] Set budget alerts
- [ ] Use streaming for long responses
- [ ] Review token usage logs
- [ ] Consider fine-tuning for repeated patterns

## Example: Cost Comparison

**Scenario:** Summarize 100 documents

**Option 1: GPT-4, individual calls**
- 100 calls × $0.03 = $3.00

**Option 2: GPT-3.5, individual calls**
- 100 calls × $0.001 = $0.10

**Option 3: GPT-3.5, batched**
- 10 batches × $0.005 = $0.05

**Savings:** 98% reduction using GPT-3.5 + batching

## Resources

- [OpenAI Pricing](https://openai.com/pricing)
- [Token Usage Guide](https://platform.openai.com/docs/guides/rate-limits)
- [Cost Calculator](../shared/OpenAIShared/CostCalculator.cs)
