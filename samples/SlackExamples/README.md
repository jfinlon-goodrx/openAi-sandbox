# Slack Integration Examples

Examples demonstrating Slack integration with OpenAI Platform projects for notifications and workflows.

## Setup

1. **Create Slack Webhook:**
   - Go to [Slack Apps → Incoming Webhooks](https://api.slack.com/messaging/webhooks)
   - Add to your workspace
   - Copy the webhook URL

2. **Configure:**
```json
{
  "Slack": {
    "WebhookUrl": "https://hooks.slack.com/services/YOUR/WEBHOOK/URL",
    "DefaultChannel": "#devops"
  }
}
```

## Direct API Examples (No Dev Environment)

### Send Simple Message

```bash
curl -X POST https://hooks.slack.com/services/YOUR/WEBHOOK/URL \
  -H "Content-Type: application/json" \
  -d '{
    "text": "Hello from OpenAI Platform Learning Portfolio!"
  }'
```

### Send Formatted Message

```bash
curl -X POST https://hooks.slack.com/services/YOUR/WEBHOOK/URL \
  -H "Content-Type: application/json" \
  -d '{
    "blocks": [
      {
        "type": "header",
        "text": {
          "type": "plain_text",
          "text": "Daily Summary"
        }
      },
      {
        "type": "section",
        "text": {
          "type": "mrkdwn",
          "text": "Summary of today'\''s activities..."
        }
      }
    ]
  }'
```

## Integration Examples

### Daily Summary (SDM)

```csharp
var slackIntegration = new SlackIntegration(httpClient, logger, webhookUrl);
var sdmService = new SDMService(openAIClient, jiraIntegration, confluenceIntegration, logger);

var summary = await sdmService.GetDailyActivitySummaryAsync("PROJ", DateTime.UtcNow.AddDays(-1));

await slackIntegration.SendDailySummaryAsync(
    summary: summary.Summary,
    metrics: new Dictionary<string, string>
    {
        { "Tickets", summary.TicketsAnalyzed.ToString() }
    },
    channel: "#engineering-daily"
);
```

### Incident Report (DevOps)

```csharp
var logAnalysis = await devOpsService.AnalyzeLogsAsync(errorLogs);
var incidentReport = await devOpsService.GenerateIncidentReportAsync(logAnalysis, "High");

await slackIntegration.SendIncidentReportAsync(
    title: incidentReport.Title,
    severity: incidentReport.Severity,
    summary: incidentReport.Details,
    channel: "#incidents"
);
```

### Standup Points

```csharp
var talkingPoints = await sdmService.GenerateStandupTalkingPointsAsync(summary);

await slackIntegration.SendStandupPointsAsync(
    talkingPoints: talkingPoints,
    channel: "#standup"
);
```

### Deployment Notification

```csharp
await slackIntegration.SendDeploymentNotificationAsync(
    applicationName: "MyApp",
    environment: "Production",
    version: "1.2.3",
    status: "Success",
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

### Pipeline Status

```csharp
await slackIntegration.SendPipelineStatusAsync(
    pipelineName: "CI/CD Pipeline",
    status: "completed",
    conclusion: "success",
    runUrl: "https://github.com/my-org/my-repo/actions/runs/123456",
    channel: "#ci-cd"
);
```

## GitHub Actions + Slack

### Workflow Example

```yaml
name: Deploy and Notify Slack

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
          # Your deployment steps
          
      - name: Notify Slack on Success
        if: success()
        env:
          SLACK_WEBHOOK_URL: ${{ secrets.SLACK_WEBHOOK_URL }}
        run: |
          curl -X POST $SLACK_WEBHOOK_URL \
            -H 'Content-Type: application/json' \
            -d '{
              "text": "✅ Deployment Successful",
              "blocks": [{
                "type": "section",
                "text": {
                  "type": "mrkdwn",
                  "text": "*Deployment to Production*\nCommit: ${{ github.sha }}\nAuthor: ${{ github.actor }}"
                }
              }]
            }'
            
      - name: Notify Slack on Failure
        if: failure()
        env:
          SLACK_WEBHOOK_URL: ${{ secrets.SLACK_WEBHOOK_URL }}
        run: |
          curl -X POST $SLACK_WEBHOOK_URL \
            -H 'Content-Type: application/json' \
            -d '{
              "text": "❌ Deployment Failed",
              "blocks": [{
                "type": "section",
                "text": {
                  "type": "mrkdwn",
                  "text": "*Deployment Failed*\nCheck logs: ${{ github.server_url }}/${{ github.repository }}/actions/runs/${{ github.run_id }}"
                }
              }]
            }'
```

## Python Example

```python
import requests
import json

def send_slack_message(webhook_url, text, channel=None):
    payload = {
        "text": text
    }
    if channel:
        payload["channel"] = channel
    
    response = requests.post(webhook_url, json=payload)
    response.raise_for_status()

# Usage
send_slack_message(
    "https://hooks.slack.com/services/YOUR/WEBHOOK/URL",
    "Daily summary: 12 tickets completed, 2 blockers identified",
    "#engineering-daily"
)
```

## Channel Recommendations

- `#engineering-daily` - Daily summaries
- `#standup` - Standup talking points
- `#incidents` - Incident reports
- `#deployments` - Deployment notifications
- `#ci-cd` - Pipeline status
- `#code-review` - PR notifications
- `#retrospectives` - Retrospective analysis

## Best Practices

1. Use appropriate channels for different notification types
2. Include links to PRs, issues, or runs for easy access
3. Use color coding (good/warning/danger) for quick scanning
4. Batch related notifications when possible
5. Respect Slack rate limits (1 msg/sec per webhook)

See [Slack Integration Guide](../../docs/integrations/slack-integration.md) for complete documentation.
