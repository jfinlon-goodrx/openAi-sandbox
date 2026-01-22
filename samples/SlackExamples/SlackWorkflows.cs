using Shared.Integrations;
using SDMAssistant.Core;
using DevOpsAssistant.Core;

namespace Samples.SlackExamples;

/// <summary>
/// Complete workflows demonstrating Slack integration with various projects
/// </summary>
public class SlackWorkflows
{
    private readonly SlackIntegration _slackIntegration;
    private readonly SDMService _sdmService;
    private readonly DevOpsService _devOpsService;

    public SlackWorkflows(
        SlackIntegration slackIntegration,
        SDMService sdmService,
        DevOpsService devOpsService)
    {
        _slackIntegration = slackIntegration;
        _sdmService = sdmService;
        _devOpsService = devOpsService;
    }

    /// <summary>
    /// Daily workflow: Get summary and send to Slack
    /// </summary>
    public async Task DailySummaryWorkflowAsync(
        string projectKey,
        string slackChannel = "#engineering-daily",
        CancellationToken cancellationToken = default)
    {
        // Get yesterday's summary
        var summary = await _sdmService.GetDailyActivitySummaryAsync(
            projectKey,
            DateTime.UtcNow.AddDays(-1),
            cancellationToken);

        // Generate talking points
        var talkingPoints = await _sdmService.GenerateStandupTalkingPointsAsync(summary, cancellationToken);

        // Send to Slack
        await _slackIntegration.SendDailySummaryAsync(
            summary: summary.Summary,
            metrics: new Dictionary<string, string>
            {
                { "Tickets Analyzed", summary.TicketsAnalyzed.ToString() },
                { "Date", summary.Date.ToString("yyyy-MM-dd") }
            },
            channel: slackChannel,
            cancellationToken: cancellationToken);

        // Send standup points separately
        await _slackIntegration.SendStandupPointsAsync(
            talkingPoints: talkingPoints,
            channel: slackChannel,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Incident response workflow with Slack notifications
    /// </summary>
    public async Task IncidentResponseWorkflowAsync(
        string logs,
        string severity = "High",
        CancellationToken cancellationToken = default)
    {
        // Analyze logs
        var logAnalysis = await _devOpsService.AnalyzeLogsAsync(logs, cancellationToken: cancellationToken);

        // Generate incident report
        var incidentReport = await _devOpsService.GenerateIncidentReportAsync(
            logAnalysis,
            severity,
            cancellationToken: cancellationToken);

        // Send to Slack
        await _slackIntegration.SendIncidentReportAsync(
            title: incidentReport.Title,
            severity: incidentReport.Severity,
            summary: incidentReport.Details,
            channel: "#incidents",
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Deployment workflow with Slack notifications
    /// </summary>
    public async Task DeploymentWorkflowAsync(
        string applicationName,
        string environment,
        string version,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Perform deployment (your deployment logic here)
            // ...

            // Send success notification
            await _slackIntegration.SendDeploymentNotificationAsync(
                applicationName: applicationName,
                environment: environment,
                version: version,
                status: "Success",
                details: $"Deployment to {environment} completed successfully",
                channel: "#deployments",
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            // Send failure notification
            await _slackIntegration.SendDeploymentNotificationAsync(
                applicationName: applicationName,
                environment: environment,
                version: version,
                status: "Failure",
                details: $"Deployment failed: {ex.Message}",
                channel: "#deployments",
                cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// PR review workflow with Slack notification
    /// </summary>
    public async Task PrReviewWorkflowAsync(
        string repository,
        int prNumber,
        string title,
        string author,
        CancellationToken cancellationToken = default)
    {
        // Analyze PR for deployment readiness
        var prAnalysis = await _devOpsService.AnalyzePrForDeploymentAsync(
            repository.Split('/')[0],
            repository.Split('/')[1],
            prNumber,
            cancellationToken: cancellationToken);

        // Send notification
        await _slackIntegration.SendPrNotificationAsync(
            repository: repository,
            prNumber: prNumber,
            title: title,
            author: author,
            status: "Open",
            prUrl: $"https://github.com/{repository}/pull/{prNumber}",
            channel: "#code-review",
            cancellationToken: cancellationToken);

        // If high risk, send additional alert
        if (prAnalysis.Analysis.Contains("high risk", StringComparison.OrdinalIgnoreCase))
        {
            await _slackIntegration.SendFormattedMessageAsync(
                title: "⚠️ High Risk PR Detected",
                text: prAnalysis.Analysis,
                color: "warning",
                channel: "#code-review",
                cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Pipeline status workflow
    /// </summary>
    public async Task PipelineStatusWorkflowAsync(
        string pipelineName,
        string status,
        string? conclusion = null,
        string? runUrl = null,
        CancellationToken cancellationToken = default)
    {
        await _slackIntegration.SendPipelineStatusAsync(
            pipelineName: pipelineName,
            status: status,
            conclusion: conclusion,
            runUrl: runUrl,
            channel: "#ci-cd",
            cancellationToken: cancellationToken);
    }
}
