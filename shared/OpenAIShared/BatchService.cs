using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAIShared.Configuration;
using Polly;
using Polly.Extensions.Http;

namespace OpenAIShared;

/// <summary>
/// Service for OpenAI Batch API - process multiple requests asynchronously
/// </summary>
public class BatchService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAIConfiguration _config;
    private readonly ILogger<BatchService> _logger;

    public BatchService(
        HttpClient httpClient,
        IOptions<OpenAIConfiguration> config,
        ILogger<BatchService> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.ApiKey);
    }

    /// <summary>
    /// Creates a batch request for processing multiple completions
    /// </summary>
    public async Task<BatchResponse> CreateBatchAsync(
        List<BatchRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var batchRequest = new
        {
            input_file_id = await UploadBatchFileAsync(requests, cancellationToken),
            endpoint = "/v1/chat/completions",
            completion_window = "24h"
        };

        var json = JsonSerializer.Serialize(batchRequest, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/batches", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<BatchResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (result == null)
        {
            throw new InvalidOperationException("Failed to deserialize batch response");
        }

        _logger.LogInformation("Batch created: {BatchId}", result.Id);
        return result;
    }

    /// <summary>
    /// Uploads a batch file
    /// </summary>
    private async Task<string> UploadBatchFileAsync(
        List<BatchRequest> requests,
        CancellationToken cancellationToken)
    {
        var jsonlContent = string.Join("\n", requests.Select(r => 
            JsonSerializer.Serialize(r, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })));

        var content = new StringContent(jsonlContent, Encoding.UTF8, "application/json");
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        var response = await _httpClient.PostAsync("/files", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var fileResponse = JsonSerializer.Deserialize<FileUploadResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return fileResponse?.Id ?? throw new InvalidOperationException("Failed to upload batch file");
    }

    /// <summary>
    /// Retrieves batch status
    /// </summary>
    public async Task<BatchResponse> GetBatchStatusAsync(
        string batchId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/batches/{batchId}", cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<BatchResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result ?? throw new InvalidOperationException("Failed to deserialize batch response");
    }

    /// <summary>
    /// Retrieves batch results
    /// </summary>
    public async Task<List<BatchResult>> GetBatchResultsAsync(
        string outputFileId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/files/{outputFileId}/content", cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var results = new List<BatchResult>();
        foreach (var line in lines)
        {
            try
            {
                var result = JsonSerializer.Deserialize<BatchResult>(line, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (result != null)
                    results.Add(result);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse batch result line");
            }
        }

        return results;
    }
}

public class BatchRequest
{
    public string CustomId { get; set; } = Guid.NewGuid().ToString();
    public string Method { get; set; } = "POST";
    public string Url { get; set; } = "/v1/chat/completions";
    public ChatCompletionRequest Body { get; set; } = new();
}

public class BatchResponse
{
    public string Id { get; set; } = string.Empty;
    public string Object { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // validating, failed, in_progress, finalizing, completed, expired, cancelled
    public int? TotalRequests { get; set; }
    public int? CompletedRequests { get; set; }
    public int? FailedRequests { get; set; }
    public long? CreatedAt { get; set; }
    public long? InProgressAt { get; set; }
    public long? FinalizingAt { get; set; }
    public long? CompletedAt { get; set; }
    public string? OutputFileId { get; set; }
    public string? ErrorFileId { get; set; }
    public object? Errors { get; set; }
}

public class BatchResult
{
    public string CustomId { get; set; } = string.Empty;
    public BatchResponse? Response { get; set; }
    public object? Error { get; set; }
}

public class FileUploadResponse
{
    public string Id { get; set; } = string.Empty;
    public string Object { get; set; } = string.Empty;
    public int Bytes { get; set; }
    public long CreatedAt { get; set; }
    public string Filename { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
}
