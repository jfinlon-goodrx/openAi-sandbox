# Slack Integration Guide

## Overview

Slack integration enables automated notifications and updates from all OpenAI Platform projects. Send daily summaries, incident reports, deployment notifications, and more directly to Slack channels.

## Setup

### 1. Create Slack Webhook

1. Go to your Slack workspace
2. Navigate to [Apps → Incoming Webhooks](https://api.slack.com/messaging/webhooks)
3. Click "Add to Slack"
4. Choose the channel for notifications
5. Copy the webhook URL

### 2. Configure Webhook URL

**Option 1: Environment Variable**
```bash
export Slack__WebhookUrl="https://hooks.slack.com/services/YOUR/WEBHOOK/URL"
```

**Option 2: appsettings.json**
```json
{
  "Slack": {
    "WebhookUrl": "https://hooks.slack.com/services/YOUR/WEBHOOK/URL",
    "DefaultChannel": "#devops"
  }
}
```

## Usage Examples

### Daily Summary (SDM)

```csharp
var slackIntegration = new SlackIntegration(httpClient, logger, webhookUrl);
var sdmService = new SDMService(openAIClient, jiraIntegration, confluenceIntegration, logger);

// Get daily summary
var summary = await sdmService.GetDailyActivitySummaryAsync("PROJ", DateTime.UtcNow.AddDays(-1));

// Send to Slack
await slackIntegration.SendDailySummaryAsync(
    summary: summary.Summary,
    metrics: new Dictionary<string, string>
    {
        { "Tickets Analyzed", summary.TicketsAnalyzed.ToString() },
        { "Date", summary.Date.ToString("yyyy-MM-dd") }
    },
    channel: "#engineering"
);
```

### Incident Report (DevOps)

```csharp
var devOpsService = new DevOpsService(openAIClient, logger);
var slackIntegration = new SlackIntegration(httpClient, logger, webhookUrl);

// Analyze logs
var logAnalysis = await devOpsService.AnalyzeLogsAsync(errorLogs);
var incidentReport = await devOpsService.GenerateIncidentReportAsync(logAnalysis, "High");

// Send to Slack
await slackIntegration.SendIncidentReportAsync(
    title: incidentReport.Title,
    severity: incidentReport.Severity,
    summary: incidentReport.Details,
    affectedSystems: new List<string> { "API", "Database" },
    remediationSteps: new List<string> { "Restart service", "Check logs" },
    channel: "#incidents"
);
```

### Standup Preparation (SDM)

```csharp
var summary = await sdmService.GetDailyActivitySummaryAsync("PROJ", DateTime.UtcNow.AddDays(-1));
var talkingPoints = await sdmService.GenerateStandupTalkingPointsAsync(summary);

// Send to Slack
await slackIntegration.SendStandupPointsAsync(
    talkingPoints: talkingPoints,
    channel: "#standup"
);
```

### Deployment Notification (DevOps)

```csharp
// After deployment
await slackIntegration.SendDeploymentNotificationAsync(
    applicationName: "MyApp",
    environment: "Production",
    version: "1.2.3",
    status: "Success",
    details: "Deployment completed successfully. All health checks passed.",
    channel: "#deployments"
);
```

### PR Notification

```csharp
await slackIntegration.SendPrNotificationAsync(
    repository: "my-org/my-repo",
    prNumber: 123,
    title: "Add new feature",
    author: "john.doe",
    status: "Open",
    prUrl: "https://github.com/my-org/my-repo/pull/123",
    channel: "#code-review"
);
```

### Pipeline Status (DevOps)

```csharp
// After GitHub Actions workflow completes
await slackIntegration.SendPipelineStatusAsync(
    pipelineName: "CI/CD Pipeline",
    status: "completed",
    conclusion: "success",
    runUrl: "https://github.com/my-org/my-repo/actions/runs/123456",
    details: new Dictionary<string, string>
    {
        { "Duration", "5m 23s" },
        { "Jobs", "3/3 passed" }
    },
    channel: "#ci-cd"
);
```

## Integration with Projects

### SDM Assistant + Slack

```csharp
// Daily workflow: Get summary and send to Slack
var summary = await sdmService.GetDailyActivitySummaryAsync("PROJ", DateTime.UtcNow.AddDays(-1));
await slackIntegration.SendDailySummaryAsync(
    summary.Summary,
    metrics: new Dictionary<string, string>
    {
        { "Tickets", summary.TicketsAnalyzed.ToString() }
    },
    channel: "#engineering-daily"
);
```

### DevOps Assistant + Slack

```csharp
// Incident response workflow
var logAnalysis = await devOpsService.AnalyzeLogsAsync(logs);
var incidentReport = await devOpsService.GenerateIncidentReportAsync(logAnalysis, "High");

// Send to Slack
await slackIntegration.SendIncidentReportAsync(
    incidentReport.Title,
    incidentReport.Severity,
    incidentReport.Details,
    channel: "#incidents"
);

// Also create Jira ticket
await jiraIntegration.CreateTicketAsync("OPS", "Incident", incidentReport.Title);
```

### Retrospective Analyzer + Slack

```csharp
var retroService = new RetroAnalyzerService(openAIClient, logger);
var analysis = await retroService.AnalyzeSentimentAsync(comments);
var actionItems = await retroService.ExtractActionItemsAsync(comments);

// Send summary to Slack
await slackIntegration.SendFormattedMessageAsync(
    title: "📊 Retrospective Analysis",
    text: $"Sentiment: {analysis.OverallSentiment}\n\nAction Items: {actionItems.Count}",
    fields: actionItems.Select(ai => new SlackField
    {
        Title = ai.Description,
        Value = $"Owner: {ai.Owner ?? "Unassigned"}"
    }).ToList(),
    channel: "#retrospectives"
);
```

## GitHub Actions + Slack

### Workflow Example

```yaml
name: Deploy and Notify

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Deploy
        run: |
          # Deployment steps
          
      - name: Notify Slack
        if: always()
        env:
          SLACK_WEBHOOK_URL: ${{ secrets.SLACK_WEBHOOK_URL }}
        run: |
          curl -X POST $SLACK_WEBHOOK_URL \
            -H 'Content-Type: application/json' \
            -d '{
              "text": "Deployment to Production",
              "blocks": [{
                "type": "section",
                "text": {
                  "type": "mrkdwn",
                  "text": "*Deployment Status:* ${{ job.status }}\n*Commit:* ${{ github.sha }}"
                }
              }]
            }'
```

## Channel Recommendations

- `#engineering-daily` - Daily summaries and standup points
- `#incidents` - Incident reports and alerts
- `#deployments` - Deployment notifications
- `#ci-cd` - Pipeline status and build notifications
- `#code-review` - PR notifications and code review summaries
- `#retrospectives` - Retrospective analysis and action items

## Best Practices

1. **Use Appropriate Channels**: Route notifications to relevant channels
2. **Set Severity Colors**: Use color coding for quick visual scanning
3. **Include Links**: Always include links to PRs, issues, or runs
4. **Batch Notifications**: Group related notifications when possible
5. **Rate Limiting**: Be mindful of Slack rate limits (1 message per second per webhook)

## Troubleshooting

**Issue:** Messages not appearing in Slack
- **Solution:** Verify webhook URL is correct and channel exists

**Issue:** Rate limiting errors
- **Solution:** Implement queuing or batching for high-volume notifications

**Issue:** Formatting issues
- **Solution:** Use `SendFormattedMessageAsync` for rich formatting

## Resources

- [Slack Incoming Webhooks](https://api.slack.com/messaging/webhooks)
- [Slack Block Kit Builder](https://app.slack.com/block-kit-builder)
- [Slack API Documentation](https://api.slack.com/)
