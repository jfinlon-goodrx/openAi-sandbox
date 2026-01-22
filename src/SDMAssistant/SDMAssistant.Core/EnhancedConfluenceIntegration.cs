using Microsoft.Extensions.Logging;

namespace SDMAssistant.Core;

/// <summary>
/// Enhanced Confluence integration with additional methods for SDM workflows
/// </summary>
public class EnhancedConfluenceIntegration : ConfluenceIntegration
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EnhancedConfluenceIntegration> _logger;
    private readonly string _baseUrl;
    private readonly string _username;
    private readonly string _apiToken;

    public EnhancedConfluenceIntegration(
        RequirementsService requirementsService,
        HttpClient httpClient,
        ILogger<EnhancedConfluenceIntegration> logger,
        string baseUrl,
        string username,
        string apiToken)
        : base(requirementsService, httpClient, logger, baseUrl, username, apiToken)
    {
        _confluenceIntegration = confluenceIntegration;
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = baseUrl.TrimEnd('/');
        _username = username;
        _apiToken = apiToken;

        var authValue = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes($"{_username}:{_apiToken}"));
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);
    }

    /// <summary>
    /// Creates a page in Confluence
    /// </summary>
    public async Task<string> CreatePageAsync(
        string spaceKey,
        string title,
        string content,
        string? parentPageId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/rest/api/content";
            var body = new
            {
                type = "page",
                title = title,
                space = new { key = spaceKey },
                body = new
                {
                    storage = new
                    {
                        value = content,
                        representation = "storage"
                    }
                },
                ancestors = !string.IsNullOrEmpty(parentPageId) 
                    ? new[] { new { id = parentPageId } }
                    : null
            };

            var json = System.Text.Json.JsonSerializer.Serialize(body);
            var httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, httpContent, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = System.Text.Json.JsonSerializer.Deserialize<ConfluencePageResponse>(responseContent);

            return result?.Id ?? throw new InvalidOperationException("Failed to create Confluence page");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Confluence page");
            throw;
        }
    }

    /// <summary>
    /// Updates a Confluence page
    /// </summary>
    public async Task UpdatePageAsync(
        string pageId,
        string content,
        int version,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/rest/api/content/{pageId}";
            var body = new
            {
                version = new { number = version + 1 },
                body = new
                {
                    storage = new
                    {
                        value = content,
                        representation = "storage"
                    }
                }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(body);
            var httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(url, httpContent, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Confluence page {PageId}", pageId);
            throw;
        }
    }

    /// <summary>
    /// Gets a page by ID
    /// </summary>
    public async Task<ConfluencePageResponse> GetPageAsync(
        string pageId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/rest/api/content/{pageId}?expand=body.storage,version";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return System.Text.Json.JsonSerializer.Deserialize<ConfluencePageResponse>(content)
                ?? throw new InvalidOperationException("Failed to deserialize Confluence page");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Confluence page {PageId}", pageId);
            throw;
        }
    }
}

public class ConfluencePageResponse
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public ConfluenceBody? Body { get; set; }
    public ConfluenceVersion? Version { get; set; }
}

public class ConfluenceVersion
{
    public int Number { get; set; }
}

public class ConfluenceBody
{
    public ConfluenceStorage? Storage { get; set; }
}

public class ConfluenceStorage
{
    public string? Value { get; set; }
}
