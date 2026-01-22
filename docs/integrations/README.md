# Integration Guides

Guides for integrating OpenAI Platform projects with external tools and services.

## Available Integrations

### [Slack Integration](slack-integration.md) ⭐ NEW
Send notifications, daily summaries, incident reports, and deployment updates to Slack channels.

**Features:**
- Daily summaries and standup points
- Incident reports with severity levels
- Deployment notifications
- PR notifications
- Pipeline status updates
- Formatted messages with blocks

**Quick Start:**
1. Create Slack webhook
2. Configure webhook URL
3. Send notifications from any project

### GitHub Actions

Already integrated in:
- Code Review Assistant (automated PR reviews)
- DevOps Assistant (workflow analysis and optimization)

See [GitHub Actions Workflow Examples](../../samples/GitHubExamples/GitHubActionsWorkflows.md) for templates.

### Jira Integration

Available in:
- SDM Assistant (ticket creation, sprint data)
- Retrospective Analyzer (action items → tickets)

See [SDM Guide](../role-guides/sdm-guide.md) for setup.

### Confluence Integration

Available in:
- Requirements Assistant (pull requirements)
- SDM Assistant (create/update pages)

See [BA Guide](../role-guides/ba-guide.md) for setup.

## Common Integration Patterns

### Daily Workflow: Summary → Slack

```csharp
var summary = await sdmService.GetDailyActivitySummaryAsync("PROJ", DateTime.UtcNow.AddDays(-1));
await slackIntegration.SendDailySummaryAsync(summary.Summary, channel: "#engineering-daily");
```

### Incident Response: Logs → Analysis → Slack + Jira

```csharp
var logAnalysis = await devOpsService.AnalyzeLogsAsync(logs);
var incidentReport = await devOpsService.GenerateIncidentReportAsync(logAnalysis, "High");
await slackIntegration.SendIncidentReportAsync(incidentReport.Title, incidentReport.Severity, incidentReport.Details);
await jiraIntegration.CreateTicketAsync("OPS", "Incident", incidentReport.Title);
```

### PR Workflow: GitHub → Analysis → Slack

```csharp
var prAnalysis = await devOpsService.AnalyzePrForDeploymentAsync(owner, repo, prNumber);
await slackIntegration.SendPrNotificationAsync(repo, prNumber, pr.Title, pr.Author, "Open");
```

## Setup Requirements

| Integration | Setup Required | Documentation |
|------------|----------------|---------------|
| Slack | Webhook URL | [Slack Integration Guide](slack-integration.md) |
| GitHub Actions | GitHub Token | [GitHub Actions Examples](../../samples/GitHubExamples/GitHubActionsWorkflows.md) |
| Jira | API Token | [SDM Guide](../role-guides/sdm-guide.md) |
| Confluence | API Token | [BA Guide](../role-guides/ba-guide.md) |

## Examples

See [Slack Examples](../../samples/SlackExamples/) for complete workflow examples.

## Best Practices

1. **Use Appropriate Channels**: Route notifications to relevant Slack channels
2. **Include Links**: Always include links to PRs, issues, or runs
3. **Set Severity**: Use color coding for quick visual scanning
4. **Batch When Possible**: Group related notifications
5. **Respect Rate Limits**: Be mindful of API rate limits

## Resources

- [Slack API Documentation](https://api.slack.com/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Jira API Documentation](https://developer.atlassian.com/cloud/jira/platform/rest/v3/)
- [Confluence API Documentation](https://developer.atlassian.com/cloud/confluence/rest/v2/)
