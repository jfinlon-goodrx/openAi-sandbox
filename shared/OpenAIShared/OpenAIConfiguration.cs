namespace OpenAIShared.Configuration;

/// <summary>
/// Configuration settings for OpenAI API
/// </summary>
public class OpenAIConfiguration
{
    public const string SectionName = "OpenAI";

    /// <summary>
    /// OpenAI API key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// OpenAI API base URL (defaults to https://api.openai.com/v1)
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";

    /// <summary>
    /// Default model to use (e.g., gpt-4, gpt-4-turbo-preview)
    /// </summary>
    public string DefaultModel { get; set; } = "gpt-4-turbo-preview";

    /// <summary>
    /// Maximum number of retries for API calls
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Timeout in seconds for API calls
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Enable request/response logging
    /// </summary>
    public bool EnableLogging { get; set; } = true;
}
