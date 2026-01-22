using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenAIShared;
using Shared.Integrations;
using DevOpsAssistant.Core;

namespace Samples.AgentExamples;

/// <summary>
/// Autonomous Incident Response Agent
/// 
/// This agent demonstrates the power of AI agents by autonomously handling
/// incident response from detection to resolution. It orchestrates multiple
/// services and makes intelligent decisions throughout the process.
/// 
/// This is a CONCEPTUAL EXAMPLE showing what's possible with AI agents.
/// </summary>
public class AutonomousIncidentResponseAgent
{
    private readonly OpenAIClient _openAIClient;
    private readonly DevOpsService _devOpsService;
    private readonly GitHubIntegration? _githubIntegration;
    private readonly SlackIntegration? _slackIntegration;
    private readonly ILogger<AutonomousIncidentResponseAgent> _logger;
    private readonly string _model = "gpt-4-turbo-preview";

    public AutonomousIncidentResponseAgent(
        OpenAIClient openAIClient,
        DevOpsService devOpsService,
        GitHubIntegration? githubIntegration,
        SlackIntegration? slackIntegration,
        ILogger<AutonomousIncidentResponseAgent> logger)
    {
        _openAIClient = openAIClient;
        _devOpsService = devOpsService;
        _githubIntegration = githubIntegration;
        _slackIntegration = slackIntegration;
        _logger = logger;
    }

    /// <summary>
    /// Main entry point: Agent receives incident alert and handles it autonomously
    /// </summary>
    public async Task<IncidentResponseResult> HandleIncidentAsync(
        string incidentAlert,
        Dictionary<string, string> context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🤖 Agent: Starting autonomous incident response for: {Alert}", incidentAlert);

        // Step 1: Agent analyzes the incident
        var analysis = await AnalyzeIncidentAsync(incidentAlert, context, cancellationToken);
        _logger.LogInformation("🤖 Agent: Incident analyzed. Severity: {Severity}, Type: {Type}", 
            analysis.Severity, analysis.IncidentType);

        // Step 2: Agent decides on response strategy
        var strategy = await DetermineResponseStrategyAsync(analysis, cancellationToken);
        _logger.LogInformation("🤖 Agent: Response strategy determined: {Strategy}", strategy.Strategy);

        // Step 3: Agent executes response plan
        var executionResult = await ExecuteResponsePlanAsync(analysis, strategy, cancellationToken);

        // Step 4: Agent monitors resolution
        var resolution = await MonitorResolutionAsync(executionResult, cancellationToken);

        // Step 5: Agent documents and reports
        await DocumentIncidentAsync(analysis, strategy, executionResult, resolution, cancellationToken);

        return new IncidentResponseResult
        {
            IncidentId = analysis.IncidentId,
            Severity = analysis.Severity,
            Status = resolution.Status,
            ResolutionTime = resolution.ResolutionTime,
            ActionsTaken = executionResult.ActionsTaken,
            RootCause = resolution.RootCause
        };
    }

    /// <summary>
    /// Step 1: Agent analyzes the incident using AI reasoning
    /// </summary>
    private async Task<IncidentAnalysis> AnalyzeIncidentAsync(
        string incidentAlert,
        Dictionary<string, string> context,
        CancellationToken cancellationToken)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert incident response analyst. Analyze incidents and determine severity, type, and impact.")
            .WithInstruction($"Analyze this incident alert:\n\n" +
                           $"Alert: {incidentAlert}\n\n" +
                           $"Context:\n{JsonSerializer.Serialize(context, new JsonSerializerOptions { WriteIndented = true })}\n\n" +
                           "Provide analysis in JSON format with:\n" +
                           "- incidentId: Unique identifier\n" +
                           "- severity: Critical/High/Medium/Low\n" +
                           "- incidentType: Performance/Security/Error/Availability\n" +
                           "- affectedSystems: List of affected systems\n" +
                           "- estimatedImpact: Description of impact\n" +
                           "- urgency: Immediate/High/Medium/Low")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            ResponseFormat = new ResponseFormat { Type = "json_object" },
            Temperature = 0.3,
            MaxTokens = 1000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var analysisJson = response.Choices.First().Message.Content;
        
        return JsonSerializer.Deserialize<IncidentAnalysis>(analysisJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("Failed to parse incident analysis");
    }

    /// <summary>
    /// Step 2: Agent determines the best response strategy
    /// </summary>
    private async Task<ResponseStrategy> DetermineResponseStrategyAsync(
        IncidentAnalysis analysis,
        CancellationToken cancellationToken)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert incident response coordinator. Determine the best response strategy.")
            .WithInstruction($"Based on this incident analysis, determine the response strategy:\n\n" +
                           $"{JsonSerializer.Serialize(analysis, new JsonSerializerOptions { WriteIndented = true })}\n\n" +
                           "Consider:\n" +
                           "- Severity and urgency\n" +
                           "- Available resources\n" +
                           "- Business impact\n" +
                           "- Time to resolution\n\n" +
                           "Provide strategy in JSON format with:\n" +
                           "- strategy: Rollback/Hotfix/Escalation/Monitoring\n" +
                           "- priority: Immediate/High/Medium/Low\n" +
                           "- requiredActions: List of actions needed\n" +
                           "- estimatedResolutionTime: Minutes\n" +
                           "- escalationRequired: Boolean")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            ResponseFormat = new ResponseFormat { Type = "json_object" },
            Temperature = 0.3,
            MaxTokens = 1000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var strategyJson = response.Choices.First().Message.Content;
        
        return JsonSerializer.Deserialize<ResponseStrategy>(strategyJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("Failed to parse response strategy");
    }

    /// <summary>
    /// Step 3: Agent executes the response plan autonomously
    /// </summary>
    private async Task<ExecutionResult> ExecuteResponsePlanAsync(
        IncidentAnalysis analysis,
        ResponseStrategy strategy,
        CancellationToken cancellationToken)
    {
        var actionsTaken = new List<string>();
        var startTime = DateTime.UtcNow;

        _logger.LogInformation("🤖 Agent: Executing response plan: {Strategy}", strategy.Strategy);

        // Agent decides which actions to take based on strategy
        foreach (var action in strategy.RequiredActions)
        {
            try
            {
                var result = await ExecuteActionAsync(action, analysis, cancellationToken);
                actionsTaken.Add($"{action}: {result}");
                _logger.LogInformation("🤖 Agent: Executed action: {Action} - {Result}", action, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🤖 Agent: Failed to execute action: {Action}", action);
                actionsTaken.Add($"{action}: Failed - {ex.Message}");
            }
        }

        return new ExecutionResult
        {
            Strategy = strategy.Strategy,
            ActionsTaken = actionsTaken,
            ExecutionTime = DateTime.UtcNow - startTime,
            Success = actionsTaken.Count(a => a.Contains("Failed")) == 0
        };
    }

    /// <summary>
    /// Agent executes individual actions based on type
    /// </summary>
    private async Task<string> ExecuteActionAsync(
        string action,
        IncidentAnalysis analysis,
        CancellationToken cancellationToken)
    {
        // Agent intelligently routes actions to appropriate services
        if (action.Contains("analyze", StringComparison.OrdinalIgnoreCase))
        {
            // Use DevOps Service to analyze logs
            var logAnalysis = await _devOpsService.AnalyzeLogsAsync(
                analysis.IncidentAlert,
                logType: analysis.IncidentType,
                cancellationToken: cancellationToken);
            return $"Log analysis completed. Issues found: {logAnalysis.IssuesFound}";
        }
        else if (action.Contains("notify", StringComparison.OrdinalIgnoreCase) && _slackIntegration != null)
        {
            // Notify team via Slack
            await _slackIntegration.SendIncidentReportAsync(
                title: $"🚨 Incident: {analysis.IncidentId}",
                severity: analysis.Severity,
                summary: analysis.EstimatedImpact,
                affectedSystems: analysis.AffectedSystems,
                channel: "#incidents",
                cancellationToken: cancellationToken);
            return "Team notified via Slack";
        }
        else if (action.Contains("create", StringComparison.OrdinalIgnoreCase) && action.Contains("ticket"))
        {
            // Create Jira ticket (would need Jira integration)
            return "Ticket creation simulated";
        }
        else if (action.Contains("rollback", StringComparison.OrdinalIgnoreCase) && _githubIntegration != null)
        {
            // Create rollback PR
            return "Rollback PR creation simulated";
        }
        else if (action.Contains("monitor", StringComparison.OrdinalIgnoreCase))
        {
            // Set up monitoring
            return "Monitoring configured";
        }

        return $"Action executed: {action}";
    }

    /// <summary>
    /// Step 4: Agent monitors resolution
    /// </summary>
    private async Task<ResolutionStatus> MonitorResolutionAsync(
        ExecutionResult executionResult,
        CancellationToken cancellationToken)
    {
        // Agent checks if incident is resolved
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are monitoring incident resolution. Determine if the incident is resolved.")
            .WithInstruction($"Review execution results:\n\n" +
                           $"{JsonSerializer.Serialize(executionResult, new JsonSerializerOptions { WriteIndented = true })}\n\n" +
                           "Determine:\n" +
                           "- status: Resolved/InProgress/Escalated\n" +
                           "- rootCause: Identified root cause\n" +
                           "- resolutionTime: Time taken to resolve\n" +
                           "- requiresFollowUp: Boolean")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            ResponseFormat = new ResponseFormat { Type = "json_object" },
            Temperature = 0.3
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var resolutionJson = response.Choices.First().Message.Content;
        
        return JsonSerializer.Deserialize<ResolutionStatus>(resolutionJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new ResolutionStatus { Status = "InProgress" };
    }

    /// <summary>
    /// Step 5: Agent documents the incident
    /// </summary>
    private async Task DocumentIncidentAsync(
        IncidentAnalysis analysis,
        ResponseStrategy strategy,
        ExecutionResult executionResult,
        ResolutionStatus resolution,
        CancellationToken cancellationToken)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are documenting an incident for post-mortem analysis.")
            .WithInstruction($"Create comprehensive incident documentation:\n\n" +
                           $"Analysis: {JsonSerializer.Serialize(analysis)}\n" +
                           $"Strategy: {JsonSerializer.Serialize(strategy)}\n" +
                           $"Execution: {JsonSerializer.Serialize(executionResult)}\n" +
                           $"Resolution: {JsonSerializer.Serialize(resolution)}\n\n" +
                           "Include:\n" +
                           "- Incident summary\n" +
                           "- Timeline of events\n" +
                           "- Root cause analysis\n" +
                           "- Actions taken\n" +
                           "- Lessons learned\n" +
                           "- Prevention measures")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 2000
        };

        var documentation = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var docContent = documentation.Choices.First().Message.Content;

        // Agent could save to Confluence, create wiki page, etc.
        _logger.LogInformation("🤖 Agent: Incident documented. Length: {Length} characters", docContent.Length);

        // Notify team of resolution
        if (_slackIntegration != null && resolution.Status == "Resolved")
        {
            await _slackIntegration.SendFormattedMessageAsync(
                title: $"✅ Incident Resolved: {analysis.IncidentId}",
                text: $"Incident has been resolved.\n\nRoot Cause: {resolution.RootCause}\nResolution Time: {resolution.ResolutionTime}",
                color: "good",
                channel: "#incidents",
                cancellationToken: cancellationToken);
        }
    }
}

// Data models
public class IncidentAnalysis
{
    public string IncidentId { get; set; } = Guid.NewGuid().ToString();
    public string Severity { get; set; } = string.Empty;
    public string IncidentType { get; set; } = string.Empty;
    public List<string> AffectedSystems { get; set; } = new();
    public string EstimatedImpact { get; set; } = string.Empty;
    public string Urgency { get; set; } = string.Empty;
    public string IncidentAlert { get; set; } = string.Empty;
}

public class ResponseStrategy
{
    public string Strategy { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public List<string> RequiredActions { get; set; } = new();
    public int EstimatedResolutionTime { get; set; }
    public bool EscalationRequired { get; set; }
}

public class ExecutionResult
{
    public string Strategy { get; set; } = string.Empty;
    public List<string> ActionsTaken { get; set; } = new();
    public TimeSpan ExecutionTime { get; set; }
    public bool Success { get; set; }
}

public class ResolutionStatus
{
    public string Status { get; set; } = string.Empty;
    public string RootCause { get; set; } = string.Empty;
    public string ResolutionTime { get; set; } = string.Empty;
    public bool RequiresFollowUp { get; set; }
}

public class IncidentResponseResult
{
    public string IncidentId { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ResolutionTime { get; set; } = string.Empty;
    public List<string> ActionsTaken { get; set; } = new();
    public string RootCause { get; set; } = string.Empty;
}
