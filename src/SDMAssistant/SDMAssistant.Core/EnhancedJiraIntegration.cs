using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SDMAssistant.Core;

/// <summary>
/// Enhanced Jira integration with additional methods for SDM workflows
/// </summary>
public class EnhancedJiraIntegration
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EnhancedJiraIntegration> _logger;
    private readonly string _baseUrl;
    private readonly string _username;
    private readonly string _apiToken;

    public EnhancedJiraIntegration(
        HttpClient httpClient,
        ILogger<EnhancedJiraIntegration> logger,
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
    /// Gets tickets updated/completed on a specific date
    /// </summary>
    public async Task<List<JiraTicket>> GetTicketsByDateAsync(
        string projectKey,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var jql = $"project = {projectKey} AND (updated >= {date:yyyy-MM-dd} AND updated < {date.AddDays(1):yyyy-MM-dd} OR resolved >= {date:yyyy-MM-dd} AND resolved < {date.AddDays(1):yyyy-MM-dd})";
            var url = $"{_baseUrl}/rest/api/3/search?jql={Uri.EscapeDataString(jql)}&fields=key,summary,status,assignee,storyPoints";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<JiraSearchResponse>(content);

            return result?.Issues?.Select(i => new JiraTicket
            {
                Key = i.Key,
                Summary = i.Fields?.Summary ?? "",
                Status = i.Fields?.Status?.Name ?? "",
                Assignee = i.Fields?.Assignee?.DisplayName,
                StoryPoints = i.Fields?.StoryPoints
            }).ToList() ?? new List<JiraTicket>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching tickets for date {Date}", date);
            throw;
        }
    }

    /// <summary>
    /// Gets sprint history
    /// </summary>
    public async Task<List<SprintHistory>> GetSprintHistoryAsync(
        string projectKey,
        int sprintCount = 5,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/rest/agile/1.0/board?projectKeyOrId={projectKey}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var boards = JsonSerializer.Deserialize<JiraBoardsResponse>(content);

            if (boards?.Values == null || !boards.Values.Any())
                return new List<SprintHistory>();

            var boardId = boards.Values.First().Id;
            var sprintsUrl = $"{_baseUrl}/rest/agile/1.0/board/{boardId}/sprint?state=closed&maxResults={sprintCount}";
            var sprintsResponse = await _httpClient.GetAsync(sprintsUrl, cancellationToken);
            sprintsResponse.EnsureSuccessStatusCode();

            var sprintsContent = await sprintsResponse.Content.ReadAsStringAsync(cancellationToken);
            var sprints = JsonSerializer.Deserialize<JiraSprintsResponse>(sprintsContent);

            var history = new List<SprintHistory>();
            foreach (var sprint in sprints?.Values ?? new List<JiraSprint>())
            {
                var sprintIssues = await GetSprintTicketsAsync(projectKey, sprint.Id.ToString(), cancellationToken);
                var completedPoints = sprintIssues
                    .Where(t => t.Status == "Done")
                    .Sum(t => t.StoryPoints ?? 0);
                var plannedPoints = sprintIssues.Sum(t => t.StoryPoints ?? 0);

                history.Add(new SprintHistory
                {
                    SprintId = sprint.Id.ToString(),
                    Name = sprint.Name,
                    CompletedPoints = completedPoints,
                    PlannedPoints = plannedPoints,
                    StartDate = sprint.StartDate,
                    EndDate = sprint.EndDate
                });
            }

            return history;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sprint history for project {ProjectKey}", projectKey);
            throw;
        }
    }

    /// <summary>
    /// Gets backlog tickets
    /// </summary>
    public async Task<List<JiraTicket>> GetBacklogAsync(
        string projectKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var jql = $"project = {projectKey} AND status != Done AND status != Closed ORDER BY priority DESC";
            var url = $"{_baseUrl}/rest/api/3/search?jql={Uri.EscapeDataString(jql)}&fields=key,summary,status,assignee,storyPoints&maxResults=100";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<JiraSearchResponse>(content);

            return result?.Issues?.Select(i => new JiraTicket
            {
                Key = i.Key,
                Summary = i.Fields?.Summary ?? "",
                Status = i.Fields?.Status?.Name ?? "",
                Assignee = i.Fields?.Assignee?.DisplayName,
                StoryPoints = i.Fields?.StoryPoints
            }).ToList() ?? new List<JiraTicket>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching backlog for project {ProjectKey}", projectKey);
            throw;
        }
    }

    /// <summary>
    /// Gets active tickets
    /// </summary>
    public async Task<List<JiraTicket>> GetActiveTicketsAsync(
        string projectKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var jql = $"project = {projectKey} AND status IN (In Progress, To Do, \"In Review\")";
            var url = $"{_baseUrl}/rest/api/3/search?jql={Uri.EscapeDataString(jql)}&fields=key,summary,status,assignee,storyPoints";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<JiraSearchResponse>(content);

            return result?.Issues?.Select(i => new JiraTicket
            {
                Key = i.Key,
                Summary = i.Fields?.Summary ?? "",
                Status = i.Fields?.Status?.Name ?? "",
                Assignee = i.Fields?.Assignee?.DisplayName,
                StoryPoints = i.Fields?.StoryPoints
            }).ToList() ?? new List<JiraTicket>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching active tickets for project {ProjectKey}", projectKey);
            throw;
        }
    }

    /// <summary>
    /// Gets sprint tickets
    /// </summary>
    public async Task<List<JiraTicket>> GetSprintTicketsAsync(
        string projectKey,
        string sprintId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var jql = $"project = {projectKey} AND sprint = {sprintId}";
            var url = $"{_baseUrl}/rest/api/3/search?jql={Uri.EscapeDataString(jql)}&fields=key,summary,status,assignee,storyPoints";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<JiraSearchResponse>(content);

            return result?.Issues?.Select(i => new JiraTicket
            {
                Key = i.Key,
                Summary = i.Fields?.Summary ?? "",
                Status = i.Fields?.Status?.Name ?? "",
                Assignee = i.Fields?.Assignee?.DisplayName,
                StoryPoints = i.Fields?.StoryPoints
            }).ToList() ?? new List<JiraTicket>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sprint tickets for sprint {SprintId}", sprintId);
            throw;
        }
    }

    /// <summary>
    /// Gets sprint data for a period
    /// </summary>
    public async Task<List<SprintData>> GetSprintDataForPeriodAsync(
        string projectKey,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var sprints = await GetSprintHistoryAsync(projectKey, 20, cancellationToken);
        var relevantSprints = sprints.Where(s => 
            s.StartDate >= startDate && s.EndDate <= endDate).ToList();

        var sprintDataList = new List<SprintData>();
        foreach (var sprint in relevantSprints)
        {
            var tickets = await GetSprintTicketsAsync(projectKey, sprint.SprintId, cancellationToken);
            sprintDataList.Add(new SprintData
            {
                SprintId = sprint.SprintId,
                Name = sprint.Name,
                StartDate = sprint.StartDate,
                EndDate = sprint.EndDate,
                Tickets = tickets
            });
        }

        return sprintDataList;
    }

    /// <summary>
    /// Gets project metrics
    /// </summary>
    public async Task<ProjectMetrics> GetMetricsAsync(
        string projectKey,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var tickets = await GetTicketsByDateAsync(projectKey, startDate, cancellationToken);
        var allTickets = new List<JiraTicket>();
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var dayTickets = await GetTicketsByDateAsync(projectKey, date, cancellationToken);
            allTickets.AddRange(dayTickets);
        }

        return new ProjectMetrics
        {
            TotalTickets = allTickets.Count,
            CompletedTickets = allTickets.Count(t => t.Status == "Done"),
            InProgressTickets = allTickets.Count(t => t.Status == "In Progress"),
            TotalStoryPoints = allTickets.Sum(t => t.StoryPoints ?? 0),
            CompletedStoryPoints = allTickets.Where(t => t.Status == "Done").Sum(t => t.StoryPoints ?? 0)
        };
    }
}

// Response models
public class JiraSearchResponse
{
    public List<JiraIssue>? Issues { get; set; }
}

public class JiraIssue
{
    public string Key { get; set; } = string.Empty;
    public JiraFields? Fields { get; set; }
}

public class JiraFields
{
    public string? Summary { get; set; }
    public JiraStatus? Status { get; set; }
    public JiraUser? Assignee { get; set; }
    public int? StoryPoints { get; set; }
}

public class JiraStatus
{
    public string Name { get; set; } = string.Empty;
}

public class JiraUser
{
    public string? DisplayName { get; set; }
}

public class SprintHistory
{
    public string SprintId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int CompletedPoints { get; set; }
    public int PlannedPoints { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class JiraBoardsResponse
{
    public List<JiraBoard>? Values { get; set; }
}

public class JiraBoard
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class JiraSprintsResponse
{
    public List<JiraSprint>? Values { get; set; }
}

public class JiraSprint
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class ProjectMetrics
{
    public int TotalTickets { get; set; }
    public int CompletedTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int TotalStoryPoints { get; set; }
    public int CompletedStoryPoints { get; set; }
}
