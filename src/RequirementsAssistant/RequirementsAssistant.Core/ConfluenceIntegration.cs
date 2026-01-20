using Microsoft.Extensions.Logging;
using RequirementsAssistant.Core;

namespace RequirementsAssistant.Core;

/// <summary>
/// Integration with Confluence for pulling requirement documents
/// </summary>
public class ConfluenceIntegration
{
    private readonly RequirementsService _requirementsService;
    private readonly ILogger<ConfluenceIntegration> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _username;
    private readonly string _apiToken;

    public ConfluenceIntegration(
        RequirementsService requirementsService,
        HttpClient httpClient,
        ILogger<ConfluenceIntegration> logger,
        string baseUrl,
        string username,
        string apiToken)
    {
        _requirementsService = requirementsService;
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
    /// Fetches a Confluence page and processes it as a requirements document
    /// </summary>
    public async Task<List<Models.UserStory>> ProcessConfluencePageAsync(
        string pageId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pageUrl = $"{_baseUrl}/rest/api/content/{pageId}?expand=body.storage";
            var response = await _httpClient.GetAsync(pageUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var pageData = System.Text.Json.JsonSerializer.Deserialize<ConfluencePageResponse>(content);

            if (pageData?.Body?.Storage?.Value == null)
            {
                _logger.LogWarning("No content found in Confluence page {PageId}", pageId);
                return new List<Models.UserStory>();
            }

            // Extract text from HTML/storage format (simplified - in production, use proper HTML parsing)
            var textContent = System.Text.RegularExpressions.Regex.Replace(
                pageData.Body.Storage.Value,
                "<[^>]*>",
                string.Empty);

            return await _requirementsService.GenerateUserStoriesAsync(textContent, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Confluence page {PageId}", pageId);
            throw;
        }
    }

    /// <summary>
    /// Searches Confluence for requirement documents
    /// </summary>
    public async Task<List<ConfluenceSearchResult>> SearchRequirementsAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var searchUrl = $"{_baseUrl}/rest/api/content/search?cql=text~\"{Uri.EscapeDataString(query)}\"&expand=body.storage";
            var response = await _httpClient.GetAsync(searchUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var searchResults = System.Text.Json.JsonSerializer.Deserialize<ConfluenceSearchResponse>(content);

            return searchResults?.Results?.Select(r => new ConfluenceSearchResult
            {
                Id = r.Id,
                Title = r.Title,
                Url = $"{_baseUrl}/pages/viewpage.action?pageId={r.Id}"
            }).ToList() ?? new List<ConfluenceSearchResult>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching Confluence for query: {Query}", query);
            throw;
        }
    }
}

public class ConfluencePageResponse
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public ConfluenceBody? Body { get; set; }
}

public class ConfluenceBody
{
    public ConfluenceStorage? Storage { get; set; }
}

public class ConfluenceStorage
{
    public string? Value { get; set; }
}

public class ConfluenceSearchResponse
{
    public List<ConfluencePageResponse>? Results { get; set; }
}

public class ConfluenceSearchResult
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
