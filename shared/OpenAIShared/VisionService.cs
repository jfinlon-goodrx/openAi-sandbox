using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAIShared.Configuration;

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

        _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.ApiKey);
        _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);

        _retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.ToManyRequests)
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
        var request = new VisionRequest
        {
            Model = "gpt-4-vision-preview",
            Messages = new List<VisionMessage>
            {
                new()
                {
                    Role = "user",
                    Content = new List<VisionContent>
                    {
                        new() { Type = "text", Text = prompt },
                        new() { Type = "image_url", ImageUrl = new ImageUrl { Url = imageUrl, Detail = detail ?? "auto" } }
                    }
                }
            },
            MaxTokens = 300
        };

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _retryPolicy.ExecuteAsync(async () =>
        {
            var httpResponse = await _httpClient.PostAsync("/chat/completions", content, cancellationToken);
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
    public string Model { get; set; } = "gpt-4-vision-preview";
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
