using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace DevOpsAssistant.Core;

/// <summary>
/// Enhanced GitHub integration for DevOps workflows
/// </summary>
public class GitHubIntegration
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GitHubIntegration> _logger;
    private readonly string _token;
    private readonly string _baseUrl = "https://api.github.com";

    public GitHubIntegration(
        HttpClient httpClient,
        ILogger<GitHubIntegration> logger,
        string token)
    {
        _httpClient = httpClient;
        _logger = logger;
        _token = token;

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "DevOpsAssistant");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
    }

    /// <summary>
    /// Gets workflow runs for a repository
    /// </summary>
    public async Task<List<WorkflowRun>> GetWorkflowRunsAsync(
        string owner,
        string repo,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/repos/{owner}/{repo}/actions/runs";
            if (limit.HasValue)
            {
                url += $"?per_page={limit}";
            }

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<WorkflowRunsResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.WorkflowRuns ?? new List<WorkflowRun>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching workflow runs for {Owner}/{Repo}", owner, repo);
            throw;
        }
    }

    /// <summary>
    /// Gets workflow run logs
    /// </summary>
    public async Task<string> GetWorkflowRunLogsAsync(
        string owner,
        string repo,
        long runId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/repos/{owner}/{repo}/actions/runs/{runId}/logs";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            // GitHub returns logs as a zip file, so we'd need to handle that
            // For now, return a placeholder - in production, extract and parse the zip
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching workflow run logs for run {RunId}", runId);
            throw;
        }
    }

    /// <summary>
    /// Gets pull request details
    /// </summary>
    public async Task<GitHubPullRequest> GetPullRequestAsync(
        string owner,
        string repo,
        int prNumber,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/repos/{owner}/{repo}/pulls/{prNumber}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<GitHubPullRequest>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException("Failed to deserialize pull request");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pull request {PrNumber}", prNumber);
            throw;
        }
    }

    /// <summary>
    /// Gets pull request files
    /// </summary>
    public async Task<List<PrFile>> GetPullRequestFilesAsync(
        string owner,
        string repo,
        int prNumber,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/repos/{owner}/{repo}/pulls/{prNumber}/files";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<List<PrFile>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<PrFile>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pull request files for PR {PrNumber}", prNumber);
            throw;
        }
    }

    /// <summary>
    /// Gets repository workflows
    /// </summary>
    public async Task<List<Workflow>> GetWorkflowsAsync(
        string owner,
        string repo,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/repos/{owner}/{repo}/actions/workflows";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<WorkflowsResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Workflows ?? new List<Workflow>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching workflows for {Owner}/{Repo}", owner, repo);
            throw;
        }
    }

    /// <summary>
    /// Gets workflow file content
    /// </summary>
    public async Task<string> GetWorkflowFileAsync(
        string owner,
        string repo,
        string workflowPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/repos/{owner}/{repo}/contents/{workflowPath}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var fileContent = JsonSerializer.Deserialize<GitHubFileContent>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (fileContent?.Content != null)
            {
                // GitHub returns base64 encoded content
                var bytes = Convert.FromBase64String(fileContent.Content);
                return System.Text.Encoding.UTF8.GetString(bytes);
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching workflow file {WorkflowPath}", workflowPath);
            throw;
        }
    }

    /// <summary>
    /// Creates or updates a workflow file
    /// </summary>
    public async Task CreateOrUpdateWorkflowAsync(
        string owner,
        string repo,
        string workflowPath,
        string content,
        string branch = "main",
        CancellationToken cancellationToken = default)
    {
        try
        {
            // First, check if file exists to get SHA for update
            var getUrl = $"{_baseUrl}/repos/{owner}/{repo}/contents/{workflowPath}?ref={branch}";
            var getResponse = await _httpClient.GetAsync(getUrl, cancellationToken);
            
            string? sha = null;
            if (getResponse.IsSuccessStatusCode)
            {
                var existingContent = await getResponse.Content.ReadAsStringAsync(cancellationToken);
                var existingFile = JsonSerializer.Deserialize<GitHubFileContent>(existingContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                sha = existingFile?.Sha;
            }

            // Create or update file
            var putUrl = $"{_baseUrl}/repos/{owner}/{repo}/contents/{workflowPath}";
            var contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
            var base64Content = Convert.ToBase64String(contentBytes);

            var requestBody = new
            {
                message = $"Update workflow: {workflowPath}",
                content = base64Content,
                branch = branch,
                sha = sha
            };

            var json = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(putUrl, httpContent, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating workflow file {WorkflowPath}", workflowPath);
            throw;
        }
    }

    /// <summary>
    /// Creates a pull request
    /// </summary>
    public async Task<GitHubPullRequest> CreatePullRequestAsync(
        string owner,
        string repo,
        string title,
        string body,
        string head,
        string @base,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/repos/{owner}/{repo}/pulls";
            var requestBody = new
            {
                title = title,
                body = body,
                head = head,
                @base = @base
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<GitHubPullRequest>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException("Failed to deserialize pull request");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating pull request");
            throw;
        }
    }

    /// <summary>
    /// Gets repository issues
    /// </summary>
    public async Task<List<GitHubIssue>> GetIssuesAsync(
        string owner,
        string repo,
        string? state = "open",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/repos/{owner}/{repo}/issues?state={state ?? "open"}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<List<GitHubIssue>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<GitHubIssue>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching issues for {Owner}/{Repo}", owner, repo);
            throw;
        }
    }
}

// Response models
public class WorkflowRunsResponse
{
    public int TotalCount { get; set; }
    public List<WorkflowRun> WorkflowRuns { get; set; } = new();
}

public class WorkflowRun
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Conclusion { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int RunNumber { get; set; }
    public string HeadBranch { get; set; } = string.Empty;
    public string HeadSha { get; set; } = string.Empty;
    public string HtmlUrl { get; set; } = string.Empty;
}


public class PrFile
{
    public string Filename { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Additions { get; set; }
    public int Deletions { get; set; }
    public int Changes { get; set; }
    public string Patch { get; set; } = string.Empty;
}

public class WorkflowsResponse
{
    public int TotalCount { get; set; }
    public List<Workflow> Workflows { get; set; } = new();
}

public class Workflow
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string HtmlUrl { get; set; } = string.Empty;
}

public class GitHubFileContent
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Sha { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Encoding { get; set; } = string.Empty;
}

public class GitHubIssue
{
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string HtmlUrl { get; set; } = string.Empty;
}
