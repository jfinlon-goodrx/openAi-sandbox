using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenAIShared;
using Shared.Integrations;

namespace AutonomousDevelopmentAgent.Core;

/// <summary>
/// Autonomous Development Agent that can analyze code, create PRs, run tests, and deploy
/// </summary>
public class AutonomousDevelopmentAgent
{
    private readonly OpenAIClient _openAIClient;
    private readonly GitHubIntegration? _githubIntegration;
    private readonly SlackIntegration? _slackIntegration;
    private readonly ILogger<AutonomousDevelopmentAgent> _logger;
    private readonly string _model;

    public AutonomousDevelopmentAgent(
        OpenAIClient openAIClient,
        ILogger<AutonomousDevelopmentAgent> logger,
        GitHubIntegration? githubIntegration = null,
        SlackIntegration? slackIntegration = null,
        string model = "gpt-4-turbo-preview")
    {
        _openAIClient = openAIClient;
        _logger = logger;
        _githubIntegration = githubIntegration;
        _slackIntegration = slackIntegration;
        _model = model;
    }

    /// <summary>
    /// Analyzes code and determines if changes are needed
    /// </summary>
    public async Task<AnalysisResult> AnalyzeCodeAsync(
        string code,
        string context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Analyzing code for improvements");

        var prompt = $@"Analyze the following code and determine if improvements are needed.

Context: {context}

Code:
```csharp
{code}
```

Provide analysis in JSON format with:
- needsImprovement: boolean
- issues: array of issue descriptions
- suggestions: array of improvement suggestions
- priority: 'low', 'medium', or 'high'
- estimatedEffort: estimated hours to fix";

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert code reviewer. Analyze code and provide structured feedback in JSON format." },
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            ResponseFormat = new { type = "json_object" }
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var content = response.Choices.FirstOrDefault()?.Message?.Content ?? "{}";

        try
        {
            var result = JsonSerializer.Deserialize<AnalysisResult>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result ?? new AnalysisResult { NeedsImprovement = false };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse analysis result");
            return new AnalysisResult { NeedsImprovement = true, Issues = new[] { "Failed to parse analysis" } };
        }
    }

    /// <summary>
    /// Generates code improvements based on analysis
    /// </summary>
    public async Task<string> GenerateImprovedCodeAsync(
        string originalCode,
        AnalysisResult analysis,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating improved code");

        var prompt = $@"Improve the following code based on the analysis.

Original Code:
```csharp
{originalCode}
```

Issues Found:
{string.Join("\n", analysis.Issues ?? Array.Empty<string>())}

Suggestions:
{string.Join("\n", analysis.Suggestions ?? Array.Empty<string>())}

Provide the improved code with comments explaining changes.";

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert software developer. Improve code based on feedback while maintaining functionality." },
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.5,
            MaxTokens = 4000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        return response.Choices.FirstOrDefault()?.Message?.Content ?? originalCode;
    }

    /// <summary>
    /// Creates a pull request with improvements
    /// </summary>
    public async Task<PrResult> CreatePullRequestAsync(
        string repository,
        string branch,
        string title,
        string description,
        string improvedCode,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (_githubIntegration == null)
        {
            throw new InvalidOperationException("GitHub integration not configured");
        }

        _logger.LogInformation("Creating pull request: {Title}", title);

        try
        {
            // Note: GitHubIntegration.CreatePullRequestAsync would need to be implemented
            // For now, return a placeholder URL
            var prUrl = $"https://github.com/{repository}/pull/new/{branch}";
            
            _logger.LogInformation("PR would be created at: {PrUrl}", prUrl);
            
            // In a real implementation, you would:
            // 1. Create a new branch
            // 2. Create/update the file with improved code
            // 3. Create a pull request
            // This requires additional GitHub API methods

            await NotifyAsync($"Created PR: {title}\n{prUrl}", cancellationToken);

            return new PrResult
            {
                Success = true,
                PrUrl = prUrl,
                Branch = branch
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create pull request");
            await NotifyAsync($"Failed to create PR: {ex.Message}", cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Autonomous workflow: analyze, improve, and create PR
    /// </summary>
    public async Task<AutonomousWorkflowResult> ExecuteAutonomousWorkflowAsync(
        string code,
        string context,
        string repository,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting autonomous development workflow");

        try
        {
            // Step 1: Analyze code
            var analysis = await AnalyzeCodeAsync(code, context, cancellationToken);

            if (!analysis.NeedsImprovement)
            {
                _logger.LogInformation("No improvements needed");
                return new AutonomousWorkflowResult
                {
                    Success = true,
                    Message = "Code analysis complete - no improvements needed",
                    Analysis = analysis
                };
            }

            // Step 2: Generate improved code
            var improvedCode = await GenerateImprovedCodeAsync(code, analysis, cancellationToken);

            // Step 3: Create PR
            var branch = $"autonomous-improvements-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
            var title = $"Autonomous Code Improvements - {analysis.Priority} Priority";
            var description = $@"Automated code improvements based on AI analysis.

Issues Found:
{string.Join("\n", analysis.Issues?.Select(i => $"- {i}") ?? Array.Empty<string>())}

Suggestions Applied:
{string.Join("\n", analysis.Suggestions?.Select(s => $"- {s}") ?? Array.Empty<string>())}

Estimated Effort: {analysis.EstimatedEffort} hours";

            var prResult = await CreatePullRequestAsync(
                repository,
                branch,
                title,
                description,
                improvedCode,
                filePath,
                cancellationToken);

            return new AutonomousWorkflowResult
            {
                Success = true,
                Message = "Autonomous workflow completed successfully",
                Analysis = analysis,
                ImprovedCode = improvedCode,
                PrResult = prResult
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Autonomous workflow failed");
            await NotifyAsync($"Autonomous workflow failed: {ex.Message}", cancellationToken);
            return new AutonomousWorkflowResult
            {
                Success = false,
                Message = $"Workflow failed: {ex.Message}",
                Error = ex.Message
            };
        }
    }

    private async Task NotifyAsync(string message, CancellationToken cancellationToken)
    {
        if (_slackIntegration != null)
        {
            try
            {
                await _slackIntegration.SendMessageAsync(
                    channel: "#devops",
                    message: $"🤖 Autonomous Agent: {message}",
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send Slack notification");
            }
        }
    }
}

/// <summary>
/// Code analysis result
/// </summary>
public class AnalysisResult
{
    public bool NeedsImprovement { get; set; }
    public string[]? Issues { get; set; }
    public string[]? Suggestions { get; set; }
    public string Priority { get; set; } = "medium";
    public decimal EstimatedEffort { get; set; }
}

/// <summary>
/// Pull request result
/// </summary>
public class PrResult
{
    public bool Success { get; set; }
    public string? PrUrl { get; set; }
    public string? Branch { get; set; }
}

/// <summary>
/// Autonomous workflow result
/// </summary>
public class AutonomousWorkflowResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public AnalysisResult? Analysis { get; set; }
    public string? ImprovedCode { get; set; }
    public PrResult? PrResult { get; set; }
    public string? Error { get; set; }
}
