using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAIShared.Configuration;
using Polly;
using Polly.Extensions.Http;

namespace OpenAIShared;

/// <summary>
/// Main client wrapper for OpenAI API calls
/// Provides retry logic, rate limiting, logging, and error handling
/// </summary>
public class OpenAIClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly OpenAIConfiguration _config;
    private readonly ILogger<OpenAIClient> _logger;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;

    public OpenAIClient(
        HttpClient httpClient,
        IOptions<OpenAIConfiguration> config,
        ILogger<OpenAIClient> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;

        // Configure HTTP client
        _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "OpenAI-DotNet-Client/1.0");
        _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);

        // Configure retry policy with exponential backoff
        _retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                _config.MaxRetries,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Retry {RetryCount} after {Delay}ms. Status: {Status}",
                        retryCount,
                        timespan.TotalMilliseconds,
                        outcome.Result?.StatusCode);
                });
    }

    /// <summary>
    /// Sends a chat completion request
    /// </summary>
    public async Task<ChatCompletionResponse> GetChatCompletionAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (_config.EnableLogging)
        {
            _logger.LogInformation(
                "Sending chat completion request. Model: {Model}, Messages: {MessageCount}",
                request.Model,
                request.Messages?.Count ?? 0);
        }

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

        if (result == null)
        {
            throw new InvalidOperationException("Failed to deserialize OpenAI API response");
        }

        if (_config.EnableLogging && result.Usage != null)
        {
            var cost = CostCalculator.CalculateCost(
                request.Model,
                result.Usage.PromptTokens,
                result.Usage.CompletionTokens);

            _logger.LogInformation(
                "Chat completion completed. Tokens: {PromptTokens} input, {CompletionTokens} output. Estimated cost: {Cost}",
                result.Usage.PromptTokens,
                result.Usage.CompletionTokens,
                CostCalculator.FormatCost(cost));
        }

        return result;
    }

    /// <summary>
    /// Sends a chat completion request with streaming
    /// </summary>
    public async IAsyncEnumerable<string> GetChatCompletionStreamAsync(
        ChatCompletionRequest request,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        request.Stream = true;

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

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (line.StartsWith("data: "))
            {
                var data = line.Substring(6);
                if (data == "[DONE]")
                    yield break;

                try
                {
                    var streamResponse = JsonSerializer.Deserialize<StreamResponse>(data, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (streamResponse?.Choices?[0]?.Delta?.Content != null)
                    {
                        yield return streamResponse.Choices[0].Delta.Content;
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to parse streaming response line: {Line}", line);
                }
            }
        }
    }

    /// <summary>
    /// Creates embeddings for text
    /// </summary>
    public async Task<EmbeddingResponse> GetEmbeddingsAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _retryPolicy.ExecuteAsync(async () =>
        {
            var httpResponse = await _httpClient.PostAsync("/embeddings", content, cancellationToken);
            httpResponse.EnsureSuccessStatusCode();
            return httpResponse;
        });

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<EmbeddingResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (result == null)
        {
            throw new InvalidOperationException("Failed to deserialize OpenAI API response");
        }

        return result;
    }

    /// <summary>
    /// Transcribes audio using Whisper API
    /// </summary>
    public async Task<TranscriptionResponse> TranscribeAudioAsync(
        Stream audioStream,
        string fileName,
        string? language = null,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(audioStream), "file", fileName);
        content.Add(new StringContent("whisper-1"), "model");
        
        if (!string.IsNullOrEmpty(language))
        {
            content.Add(new StringContent(language), "language");
        }

        var response = await _retryPolicy.ExecuteAsync(async () =>
        {
            var httpResponse = await _httpClient.PostAsync("/audio/transcriptions", content, cancellationToken);
            httpResponse.EnsureSuccessStatusCode();
            return httpResponse;
        });

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<TranscriptionResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (result == null)
        {
            throw new InvalidOperationException("Failed to deserialize OpenAI API response");
        }

        return result;
    }

    /// <summary>
    /// Generates an image using DALL-E API
    /// </summary>
    public async Task<ImageGenerationResponse> GenerateImageAsync(
        string prompt,
        string model = "dall-e-3",
        string size = "1024x1024",
        string quality = "standard",
        int n = 1,
        CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            model = model,
            prompt = prompt,
            n = n,
            size = size,
            quality = quality
        };

        var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _retryPolicy.ExecuteAsync(async () =>
        {
            var httpResponse = await _httpClient.PostAsync("/images/generations", content, cancellationToken);
            httpResponse.EnsureSuccessStatusCode();
            return httpResponse;
        });

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<ImageGenerationResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (result == null)
        {
            throw new InvalidOperationException("Failed to deserialize OpenAI API response");
        }

        if (_config.EnableLogging)
        {
            _logger.LogInformation(
                "Image generation completed. Model: {Model}, Size: {Size}, Images: {Count}",
                model,
                size,
                result.Data?.Count ?? 0);
        }

        return result;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

// Request/Response models
public class ChatCompletionRequest
{
    public string Model { get; set; } = "gpt-4-turbo-preview";
    public List<ChatMessage> Messages { get; set; } = new();
    public double? Temperature { get; set; }
    public int? MaxTokens { get; set; }
    public bool? Stream { get; set; }
    public List<FunctionDefinition>? Functions { get; set; }
    public string? FunctionCall { get; set; }
    public object? ResponseFormat { get; set; } // For JSON mode: { type = "json_object" }
}

public class ChatCompletionResponse
{
    public string Id { get; set; } = string.Empty;
    public string Object { get; set; } = string.Empty;
    public long Created { get; set; }
    public string Model { get; set; } = string.Empty;
    public List<Choice> Choices { get; set; } = new();
    public Usage? Usage { get; set; }
}

public class Choice
{
    public int Index { get; set; }
    public Message? Message { get; set; }
    public string FinishReason { get; set; } = string.Empty;
}

public class Message
{
    public string Role { get; set; } = string.Empty;
    public string? Content { get; set; }
    public FunctionCall? FunctionCall { get; set; }
}

public class FunctionCall
{
    public string Name { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
}

public class FunctionDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public object? Parameters { get; set; }
}

public class Usage
{
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
}

public class StreamResponse
{
    public List<StreamChoice>? Choices { get; set; }
}

public class StreamChoice
{
    public StreamDelta? Delta { get; set; }
}

public class StreamDelta
{
    public string? Content { get; set; }
}

public class EmbeddingRequest
{
    public string Model { get; set; } = "text-embedding-ada-002";
    public List<string> Input { get; set; } = new();
}

public class EmbeddingResponse
{
    public string Object { get; set; } = string.Empty;
    public List<EmbeddingData> Data { get; set; } = new();
    public string Model { get; set; } = string.Empty;
    public Usage? Usage { get; set; }
}

public class EmbeddingData
{
    public string Object { get; set; } = string.Empty;
    public List<double> Embedding { get; set; } = new();
    public int Index { get; set; }
}

public class TranscriptionResponse
{
    public string Text { get; set; } = string.Empty;
}

public class ImageGenerationResponse
{
    public long Created { get; set; }
    public List<ImageData> Data { get; set; } = new();
}

public class ImageData
{
    public string? Url { get; set; }
    public string? B64Json { get; set; }
    public string? RevisedPrompt { get; set; }
}
