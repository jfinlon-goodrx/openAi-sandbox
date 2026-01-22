using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenAIShared;

namespace SDMAssistant.Core;

/// <summary>
/// Service for Software Development Manager workflows with Jira and Confluence integration
/// </summary>
public class SDMService
{
    private readonly OpenAIClient _openAIClient;
    private readonly JiraIntegration _jiraIntegration;
    private readonly ConfluenceIntegration _confluenceIntegration;
    private readonly ILogger<SDMService> _logger;
    private readonly string _model;

    public SDMService(
        OpenAIClient openAIClient,
        JiraIntegration jiraIntegration,
        ConfluenceIntegration confluenceIntegration,
        ILogger<SDMService> logger,
        string model = "gpt-4-turbo-preview")
    {
        _openAIClient = openAIClient;
        _jiraIntegration = jiraIntegration;
        _confluenceIntegration = confluenceIntegration;
        _logger = logger;
        _model = model;
    }

    /// <summary>
    /// Generates daily activity summary from Jira
    /// </summary>
    public async Task<DailyActivitySummary> GetDailyActivitySummaryAsync(
        string projectKey,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        // Get tickets updated/completed on the date
        var tickets = await _jiraIntegration.GetTicketsByDateAsync(projectKey, date, cancellationToken);

        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert software development manager analyzing team activity.")
            .WithInstruction($"Analyze the following Jira tickets updated/completed on {date:yyyy-MM-dd}. " +
                           "Provide: completed work summary, blockers identified, risks detected, and team achievements.")
            .WithInput($"Tickets: {JsonSerializer.Serialize(tickets)}")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert SDM." },
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 1000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var summary = response.Choices.FirstOrDefault()?.Message?.Content ?? "";

        return new DailyActivitySummary
        {
            Date = date,
            Summary = summary,
            TicketsAnalyzed = tickets.Count
        };
    }

    /// <summary>
    /// Generates standup talking points from daily activity
    /// </summary>
    public async Task<List<string>> GenerateStandupTalkingPointsAsync(
        DailyActivitySummary summary,
        CancellationToken cancellationToken = default)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are preparing talking points for a daily standup meeting.")
            .WithInstruction("Based on the following daily activity summary, generate 5-7 concise talking points " +
                           "covering: achievements, blockers, risks, and action items.")
            .WithInput(summary.Summary)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 500
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var content = response.Choices.FirstOrDefault()?.Message?.Content ?? "";

        // Parse bullet points
        return content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.Trim().StartsWith("-") || line.Trim().StartsWith("•") || char.IsDigit(line.Trim()[0]))
            .Select(line => line.Trim().TrimStart('-', '•', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '.', ' '))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();
    }

    /// <summary>
    /// Analyzes team velocity from historical sprint data
    /// </summary>
    public async Task<VelocityAnalysis> AnalyzeTeamVelocityAsync(
        string projectKey,
        int sprintCount = 5,
        CancellationToken cancellationToken = default)
    {
        var sprints = await _jiraIntegration.GetSprintHistoryAsync(projectKey, sprintCount, cancellationToken);

        var sprintData = string.Join("\n", sprints.Select(s => 
            $"Sprint {s.Name}: {s.CompletedPoints} points completed, {s.PlannedPoints} planned"));

        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert at analyzing team velocity and sprint performance.")
            .WithInstruction("Analyze the following sprint data and provide: average velocity, trend analysis, " +
                           "capacity recommendations, and velocity stability assessment.")
            .WithInput(sprintData)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 800
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var analysis = response.Choices.FirstOrDefault()?.Message?.Content ?? "";

        return new VelocityAnalysis
        {
            SprintsAnalyzed = sprints.Count,
            Analysis = analysis,
            AverageVelocity = sprints.Any() ? sprints.Average(s => s.CompletedPoints) : 0
        };
    }

    /// <summary>
    /// Generates sprint plan based on capacity and goals
    /// </summary>
    public async Task<SprintPlan> GenerateSprintPlanAsync(
        string projectKey,
        string sprintGoal,
        int teamCapacity,
        CancellationToken cancellationToken = default)
    {
        var backlog = await _jiraIntegration.GetBacklogAsync(projectKey, cancellationToken);

        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert at sprint planning.")
            .WithInstruction($"Sprint Goal: {sprintGoal}\nTeam Capacity: {teamCapacity} story points\n\n" +
                           "Analyze the backlog and suggest: which tickets to include, prioritization, " +
                           "dependencies to watch, and sprint goal breakdown.")
            .WithInput($"Backlog: {JsonSerializer.Serialize(backlog.Take(50))}")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 1500
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var plan = response.Choices.FirstOrDefault()?.Message?.Content ?? "";

        return new SprintPlan
        {
            SprintGoal = sprintGoal,
            TeamCapacity = teamCapacity,
            Plan = plan,
            GeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Identifies risks from sprint data
    /// </summary>
    public async Task<List<Risk>> IdentifyRisksAsync(
        string projectKey,
        string? sprintId = null,
        CancellationToken cancellationToken = default)
    {
        var tickets = sprintId != null
            ? await _jiraIntegration.GetSprintTicketsAsync(projectKey, sprintId, cancellationToken)
            : await _jiraIntegration.GetActiveTicketsAsync(projectKey, cancellationToken);

        var ticketSummaries = tickets.Select(t => 
            $"Key: {t.Key}, Status: {t.Status}, Assignee: {t.Assignee}, Summary: {t.Summary}")
            .ToList();

        var prompt = new PromptBuilder()
            .WithSystemMessage("You are a risk management expert analyzing software development risks.")
            .WithInstruction("Analyze the following tickets and identify: blockers, at-risk tickets, " +
                           "dependencies, resource constraints, and timeline risks. For each risk, provide severity and mitigation suggestions.")
            .WithInput(string.Join("\n", ticketSummaries))
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 1500
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var analysis = response.Choices.FirstOrDefault()?.Message?.Content ?? "";

        // Parse risks (simplified - in production, use structured output)
        return ParseRisksFromAnalysis(analysis);
    }

    /// <summary>
    /// Generates status report from project data
    /// </summary>
    public async Task<StatusReport> GenerateStatusReportAsync(
        string projectKey,
        DateTime startDate,
        DateTime endDate,
        bool includeMetrics = true,
        CancellationToken cancellationToken = default)
    {
        var sprintData = await _jiraIntegration.GetSprintDataForPeriodAsync(
            projectKey, startDate, endDate, cancellationToken);

        var metrics = includeMetrics
            ? await _jiraIntegration.GetMetricsAsync(projectKey, startDate, endDate, cancellationToken)
            : null;

        var prompt = new PromptBuilder()
            .WithSystemMessage("You are preparing an executive status report.")
            .WithInstruction($"Generate a professional status report for the period {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}. " +
                           "Include: accomplishments, progress metrics, blockers, risks, and next steps. Format for executive audience.")
            .WithInput($"Sprint Data: {JsonSerializer.Serialize(sprintData)}\nMetrics: {JsonSerializer.Serialize(metrics)}")
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

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var report = response.Choices.FirstOrDefault()?.Message?.Content ?? "";

        return new StatusReport
        {
            ProjectKey = projectKey,
            StartDate = startDate,
            EndDate = endDate,
            Report = report,
            Metrics = metrics,
            GeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Analyzes sprint health
    /// </summary>
    public async Task<SprintHealthAnalysis> AnalyzeSprintHealthAsync(
        SprintData sprintData,
        CancellationToken cancellationToken = default)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are analyzing sprint health and progress.")
            .WithInstruction("Analyze sprint health based on: burndown progress, ticket status distribution, " +
                           "blockers, team velocity, and risk factors. Provide health score (1-10) and recommendations.")
            .WithInput(JsonSerializer.Serialize(sprintData))
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 1000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var analysis = response.Choices.FirstOrDefault()?.Message?.Content ?? "";

        return new SprintHealthAnalysis
        {
            SprintId = sprintData.SprintId,
            Analysis = analysis,
            AnalyzedAt = DateTime.UtcNow
        };
    }

    private List<Risk> ParseRisksFromAnalysis(string analysis)
    {
        // Simplified parsing - in production, use structured output or more sophisticated parsing
        var risks = new List<Risk>();
        var lines = analysis.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        Risk? currentRisk = null;
        foreach (var line in lines)
        {
            if (line.Contains("Risk:") || line.Contains("Severity:"))
            {
                if (currentRisk != null) risks.Add(currentRisk);
                currentRisk = new Risk { Description = line };
            }
            else if (currentRisk != null)
            {
                if (line.Contains("High") || line.Contains("Critical")) currentRisk.Severity = "High";
                else if (line.Contains("Medium")) currentRisk.Severity = "Medium";
                else if (line.Contains("Low")) currentRisk.Severity = "Low";

                if (line.Contains("Mitigation:")) currentRisk.Mitigation = line;
            }
        }
        if (currentRisk != null) risks.Add(currentRisk);

        return risks;
    }
}

// Data models
public class DailyActivitySummary
{
    public DateTime Date { get; set; }
    public string Summary { get; set; } = string.Empty;
    public int TicketsAnalyzed { get; set; }
}

public class VelocityAnalysis
{
    public int SprintsAnalyzed { get; set; }
    public double AverageVelocity { get; set; }
    public string Analysis { get; set; } = string.Empty;
}

public class SprintPlan
{
    public string SprintGoal { get; set; } = string.Empty;
    public int TeamCapacity { get; set; }
    public string Plan { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}

public class Risk
{
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium";
    public string? Mitigation { get; set; }
}

public class StatusReport
{
    public string ProjectKey { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Report { get; set; } = string.Empty;
    public object? Metrics { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class SprintHealthAnalysis
{
    public string SprintId { get; set; } = string.Empty;
    public string Analysis { get; set; } = string.Empty;
    public DateTime AnalyzedAt { get; set; }
}

public class SprintData
{
    public string SprintId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<JiraTicket> Tickets { get; set; } = new();
}

public class JiraTicket
{
    public string Key { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Assignee { get; set; }
    public int? StoryPoints { get; set; }
}
