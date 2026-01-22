using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAIShared.Configuration;
using Polly;
using Polly.Extensions.Http;

namespace OpenAIShared;

/// <summary>
/// Service for OpenAI Moderation API - content filtering and safety
/// </summary>
public class ModerationService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAIConfiguration _config;
    private readonly ILogger<ModerationService> _logger;

    public ModerationService(
        HttpClient httpClient,
        IOptions<OpenAIConfiguration> config,
        ILogger<ModerationService> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config.ApiKey);
    }

    /// <summary>
    /// Checks if content violates OpenAI's usage policies
    /// </summary>
    public async Task<ModerationResponse> ModerateContentAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            input = input
        };

        var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/moderations", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<ModerationResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (result == null)
        {
            throw new InvalidOperationException("Failed to deserialize moderation response");
        }

        if (result.Results != null && result.Results.Any())
        {
            var flagged = result.Results.First().Flagged;
            _logger.LogInformation("Content moderation check: Flagged = {Flagged}", flagged);
        }

        return result;
    }
}

public class ModerationResponse
{
    public string Id { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public List<ModerationResult> Results { get; set; } = new();
}

public class ModerationResult
{
    public bool Flagged { get; set; }
    public ModerationCategories Categories { get; set; } = new();
    public ModerationCategoryScores CategoryScores { get; set; } = new();
}

public class ModerationCategories
{
    public bool Hate { get; set; }
    public bool HateThreatening { get; set; }
    public bool Harassment { get; set; }
    public bool HarassmentThreatening { get; set; }
    public bool SelfHarm { get; set; }
    public bool SelfHarmIntent { get; set; }
    public bool SelfHarmInstructions { get; set; }
    public bool Sexual { get; set; }
    public bool SexualMinors { get; set; }
    public bool Violence { get; set; }
    public bool ViolenceGraphic { get; set; }
}

public class ModerationCategoryScores
{
    public double Hate { get; set; }
    public double HateThreatening { get; set; }
    public double Harassment { get; set; }
    public double HarassmentThreatening { get; set; }
    public double SelfHarm { get; set; }
    public double SelfHarmIntent { get; set; }
    public double SelfHarmInstructions { get; set; }
    public double Sexual { get; set; }
    public double SexualMinors { get; set; }
    public double Violence { get; set; }
    public double ViolenceGraphic { get; set; }
}
