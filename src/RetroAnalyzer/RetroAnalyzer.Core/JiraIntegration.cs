using Microsoft.Extensions.Logging;
using Models;
using RetroAnalyzer.Core;

namespace RetroAnalyzer.Core;

/// <summary>
/// Integration with Jira for creating action items as tickets
/// </summary>
public class JiraIntegration
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<JiraIntegration> _logger;
    private readonly string _baseUrl;
    private readonly string _username;
    private readonly string _apiToken;

    public JiraIntegration(
        HttpClient httpClient,
        ILogger<JiraIntegration> logger,
        string baseUrl,
        string username,
        string apiToken)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = baseUrl.TrimEnd('/');
        _username = username;
        _apiToken = apiToken;

        var authValue = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes($"{_username}:{_apiToken}"));
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// Creates Jira tickets from action items
    /// </summary>
    public async Task<List<string>> CreateTicketsFromActionItemsAsync(
        List<ActionItem> actionItems,
        string projectKey,
        string issueType = "Task",
        CancellationToken cancellationToken = default)
    {
        var createdTicketKeys = new List<string>();

        foreach (var actionItem in actionItems)
        {
            try
            {
                var ticketKey = await CreateTicketAsync(
                    projectKey,
                    issueType,
                    actionItem.Description,
                    actionItem.Owner,
                    actionItem.Priority,
                    cancellationToken);

                createdTicketKeys.Add(ticketKey);
                _logger.LogInformation("Created Jira ticket {TicketKey} for action item: {Description}", 
                    ticketKey, actionItem.Description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Jira ticket for action item: {Description}", 
                    actionItem.Description);
            }
        }

        return createdTicketKeys;
    }

    /// <summary>
    /// Creates a single Jira ticket
    /// </summary>
    public async Task<string> CreateTicketAsync(
        string projectKey,
        string issueType,
        string summary,
        string? assignee = null,
        string? priority = null,
        CancellationToken cancellationToken = default)
    {
        var createUrl = $"{_baseUrl}/rest/api/3/issue";

        var requestBody = new
        {
            fields = new
            {
                project = new { key = projectKey },
                summary = summary,
                issuetype = new { name = issueType },
                assignee = !string.IsNullOrEmpty(assignee) ? new { name = assignee } : null,
                priority = !string.IsNullOrEmpty(priority) ? new { name = priority } : null
            }
        };

        var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(createUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = System.Text.Json.JsonSerializer.Deserialize<JiraCreateResponse>(responseContent);

        return result?.Key ?? throw new InvalidOperationException("Failed to create Jira ticket");
    }

    /// <summary>
    /// Fetches retrospective comments from Jira comments (if stored as issues)
    /// </summary>
    public async Task<List<string>> GetRetrospectiveCommentsAsync(
        string issueKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var commentsUrl = $"{_baseUrl}/rest/api/3/issue/{issueKey}/comment";
            var response = await _httpClient.GetAsync(commentsUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var commentsData = System.Text.Json.JsonSerializer.Deserialize<JiraCommentsResponse>(content);

            return commentsData?.Comments?.Select(c => c.Body?.ToString() ?? string.Empty)
                .Where(c => !string.IsNullOrEmpty(c))
                .ToList() ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching comments from Jira issue {IssueKey}", issueKey);
            throw;
        }
    }
}

public class JiraCreateResponse
{
    public string? Key { get; set; }
    public string? Id { get; set; }
}

public class JiraCommentsResponse
{
    public List<JiraComment>? Comments { get; set; }
}

public class JiraComment
{
    public object? Body { get; set; }
    public string? Author { get; set; }
    public DateTime? Created { get; set; }
}
