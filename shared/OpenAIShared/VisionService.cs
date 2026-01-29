using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAIShared.Configuration;
using Polly;
using Polly.Extensions.Http;

namespace OpenAIShared;

/// <summary>
/// Service for OpenAI Vision API (GPT-4 Vision) - analyzes images
/// </summary>
public class VisionService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAIConfiguration _config;
    private readonly ILogger<VisionService> _logger;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;

    public VisionService(
        HttpClient httpClient,
        IOptions<OpenAIConfiguration> config,
        ILogger<VisionService> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;

        // Configure HTTP client - ensure BaseAddress ends with / for proper path combination
        var baseUrl = _config.BaseUrl.TrimEnd('/');
        var expectedBaseUrl = new Uri(baseUrl + "/");
        if (_httpClient.BaseAddress == null || _httpClient.BaseAddress != expectedBaseUrl)
        {
            _httpClient.BaseAddress = expectedBaseUrl;
        }
        
        // Set authorization header (override if already set to ensure correct key)
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.ApiKey);
        
        // Set timeout
        _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
        
        // Log configuration for debugging
        if (_config.EnableLogging)
        {
            _logger.LogInformation("Vision Service configured. BaseAddress: {BaseAddress}, Timeout: {Timeout}s", 
                _httpClient.BaseAddress, _config.TimeoutSeconds);
        }

        _retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                _config.MaxRetries,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    /// <summary>
    /// Analyzes an image using GPT-4 Vision
    /// </summary>
    public async Task<string> AnalyzeImageAsync(
        string imageUrl,
        string prompt,
        string? detail = null,
        CancellationToken cancellationToken = default)
    {
        // Build request as dictionary to ensure exact JSON format for OpenAI API (snake_case)
        var requestDict = new Dictionary<string, object>
        {
            ["model"] = "gpt-4o",
            ["messages"] = new[]
            {
                new Dictionary<string, object>
                {
                    ["role"] = "user",
                    ["content"] = new object[]
                    {
                        new Dictionary<string, object> { ["type"] = "text", ["text"] = prompt },
                        new Dictionary<string, object>
                        {
                            ["type"] = "image_url",
                            ["image_url"] = new Dictionary<string, object>
                            {
                                ["url"] = imageUrl,
                                ["detail"] = detail ?? "auto"
                            }
                        }
                    }
                }
            },
            ["max_tokens"] = 300
        };

        var json = JsonSerializer.Serialize(requestDict);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _retryPolicy.ExecuteAsync(async () =>
        {
            // Use relative path (without leading /) since BaseAddress ends with /
            var endpoint = "chat/completions";
            if (_config.EnableLogging)
            {
                var fullUrl = new Uri(_httpClient.BaseAddress!, endpoint).ToString();
                _logger.LogInformation("Vision API request to: {FullUrl}, Model: gpt-4o", fullUrl);
            }
            var httpResponse = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            
            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Vision API error {StatusCode}: {ErrorContent}", httpResponse.StatusCode, errorContent);
            }
            
            httpResponse.EnsureSuccessStatusCode();
            return httpResponse;
        });

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<ChatCompletionResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result?.Choices?.FirstOrDefault()?.Message?.Content ?? "Unable to analyze image.";
    }

    /// <summary>
    /// Analyzes an image from base64 encoded data
    /// </summary>
    public async Task<string> AnalyzeImageFromBase64Async(
        string base64Image,
        string prompt,
        CancellationToken cancellationToken = default)
    {
        var imageUrl = $"data:image/jpeg;base64,{base64Image}";
        return await AnalyzeImageAsync(imageUrl, prompt, cancellationToken: cancellationToken);
    }
}

public class VisionRequest
{
    public string Model { get; set; } = "gpt-4o";  // Updated to current vision-capable model
    public List<VisionMessage> Messages { get; set; } = new();
    public int? MaxTokens { get; set; }
}

public class VisionMessage
{
    public string Role { get; set; } = string.Empty;
    public List<VisionContent> Content { get; set; } = new();
}

public class VisionContent
{
    public string Type { get; set; } = string.Empty; // "text" or "image_url"
    public string? Text { get; set; }
    public ImageUrl? ImageUrl { get; set; }
}

public class ImageUrl
{
    public string Url { get; set; } = string.Empty;
    public string? Detail { get; set; } // "low", "high", or "auto"
}
