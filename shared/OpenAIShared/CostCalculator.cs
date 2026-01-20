namespace OpenAIShared;

/// <summary>
/// Calculates estimated costs for OpenAI API usage
/// Prices are per 1M tokens (as of 2024, subject to change)
/// </summary>
public static class CostCalculator
{
    // Pricing per 1M tokens (input/output)
    private static readonly Dictionary<string, (decimal Input, decimal Output)> ModelPricing = new()
    {
        { "gpt-4-turbo-preview", (0.01m, 0.03m) },
        { "gpt-4", (0.03m, 0.06m) },
        { "gpt-4-32k", (0.06m, 0.12m) },
        { "gpt-3.5-turbo", (0.0005m, 0.0015m) },
        { "text-embedding-ada-002", (0.0001m, 0.0001m) },
        { "whisper-1", (0.006m, 0.006m) } // per minute of audio
    };

    /// <summary>
    /// Calculates the estimated cost for a given model and token counts
    /// </summary>
    public static decimal CalculateCost(string model, int inputTokens, int outputTokens)
    {
        if (!ModelPricing.TryGetValue(model, out var pricing))
        {
            // Default to GPT-4 pricing if model not found
            pricing = ModelPricing["gpt-4"];
        }

        var inputCost = (inputTokens / 1_000_000m) * pricing.Input;
        var outputCost = (outputTokens / 1_000_000m) * pricing.Output;

        return inputCost + outputCost;
    }

    /// <summary>
    /// Calculates the estimated cost for a single request
    /// </summary>
    public static decimal CalculateCost(string model, int totalTokens)
    {
        // Assume 70% input, 30% output for single token count
        var inputTokens = (int)(totalTokens * 0.7);
        var outputTokens = totalTokens - inputTokens;
        return CalculateCost(model, inputTokens, outputTokens);
    }

    /// <summary>
    /// Formats cost as currency string
    /// </summary>
    public static string FormatCost(decimal cost)
    {
        return cost < 0.01m ? $"${cost * 1000:F3} cents" : $"${cost:F4}";
    }
}
