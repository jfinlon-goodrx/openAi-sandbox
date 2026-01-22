# Sprint Retrospective Analyzer

## Overview

The Sprint Retrospective Analyzer helps Scrum Masters and Product Managers analyze retrospective comments, extract action items, identify themes, and track team sentiment.

## Features

- **Action Item Extraction**: Automatically extract actionable items from comments
- **Theme Identification**: Identify recurring themes and patterns
- **Sentiment Analysis**: Analyze team sentiment and morale
- **Improvement Suggestions**: Generate actionable improvement suggestions
- **Jira Integration**: Create Jira tickets from action items

## Architecture

```
┌─────────────────┐
│  Blazor Web UI  │
└────────┬────────┘
         │
┌────────▼────────┐
│   Web API       │
└────────┬────────┘
         │
┌────────▼────────┐
│ Retro Analyzer  │
│ Service         │
└────────┬────────┘
         │
┌────────▼────────┐
│  OpenAI Client  │
└─────────────────┘
```

## Setup

1. Configure OpenAI API key (see [Setup Guide](../getting-started/01-setup.md))

2. Run the API:
```bash
cd src/RetroAnalyzer/RetroAnalyzer.Api
dotnet run
```

3. Run the Web UI:
```bash
cd src/RetroAnalyzer/RetroAnalyzer.Web
dotnet run
```

## API Endpoints

### POST /api/retro/extract-action-items

Extracts action items from retrospective comments.

**Request:**
```json
{
  "comments": [
    "We need better documentation",
    "John will create a wiki page",
    "Meetings are too long"
  ]
}
```

**Response:**
```json
[
  {
    "id": "guid",
    "description": "Create wiki page",
    "owner": "John",
    "priority": "Medium",
    "status": "Open"
  }
]
```

### POST /api/retro/identify-themes

Identifies recurring themes.

**Request:**
```json
{
  "comments": ["Comment 1", "Comment 2", ...]
}
```

**Response:**
```json
[
  "Communication issues",
  "Process improvements needed",
  "Tooling concerns"
]
```

### POST /api/retro/analyze-sentiment

Analyzes sentiment of comments.

**Request:**
```json
{
  "comments": ["Comment 1", "Comment 2", ...]
}
```

**Response:**
```json
{
  "overallSentiment": "positive",
  "sentimentScore": 0.7,
  "keyInsights": [
    "Team is generally positive",
    "Some concerns about workload"
  ]
}
```

### POST /api/retro/improvement-suggestions

Generates improvement suggestions.

**Request:**
```json
{
  "comments": ["Comment 1", "Comment 2", ...]
}
```

**Response:**
```json
{
  "suggestions": "1. Implement daily standups...\n2. Improve documentation..."
}
```

## Jira Integration

### Setup

1. Get Jira API token from [Atlassian Account Settings](https://id.atlassian.com/manage-profile/security/api-tokens)

2. Configure in `appsettings.json`:
```json
{
  "Jira": {
    "BaseUrl": "https://your-domain.atlassian.net",
    "Username": "your-email@example.com",
    "ApiToken": "your-api-token",
    "ProjectKey": "PROJ"
  }
}
```

### Usage

```csharp
var jiraIntegration = new JiraIntegration(
    httpClient,
    logger,
    baseUrl,
    username,
    apiToken
);

// Create tickets from action items
var ticketKeys = await jiraIntegration.CreateTicketsFromActionItemsAsync(
    actionItems,
    "PROJ"
);
```

## Example Usage

### Analyze Retrospective

```csharp
var retroService = new RetroAnalyzerService(openAIClient, logger);

var comments = new List<string>
{
    "We need better documentation",
    "John will create a wiki page by next sprint",
    "Meetings are too long, let's make them shorter",
    "Great collaboration this sprint!"
};

// Extract action items
var actionItems = await retroService.ExtractActionItemsAsync(comments);

// Identify themes
var themes = await retroService.IdentifyThemesAsync(comments);

// Analyze sentiment
var sentiment = await retroService.AnalyzeSentimentAsync(comments);

// Get suggestions
var suggestions = await retroService.GenerateImprovementSuggestionsAsync(comments);
```

## Workflow

1. **Collect Comments**: Gather retrospective comments from team
2. **Analyze**: Use Retro Analyzer to extract insights
3. **Create Tickets**: Automatically create Jira tickets for action items
4. **Track Progress**: Monitor action item completion over time

## Best Practices

1. **Review Output**: Always review extracted action items for accuracy
2. **Assign Owners**: Ensure action items have clear owners
3. **Follow Up**: Track action items in subsequent retrospectives
4. **Use Themes**: Address recurring themes at the team level

## Troubleshooting

**Issue:** Action items lack detail
- **Solution:** Provide more context in comments, include examples

**Issue:** Sentiment analysis seems inaccurate
- **Solution:** Review comments manually, sentiment analysis is a guide

**Issue:** Jira integration fails
- **Solution:** Verify API token and project key, check permissions

## Slack Integration

Share retrospective insights with your team via Slack:

```csharp
var slackIntegration = new SlackIntegration(httpClient, logger, slackWebhookUrl);

// After analysis
await slackIntegration.SendFormattedMessageAsync(
    title: "📊 Retrospective Analysis",
    text: $"Sentiment: {sentiment.OverallSentiment}\n\nThemes: {string.Join(", ", themes)}",
    fields: actionItems.Select(ai => new SlackField
    {
        Title = ai.Description,
        Value = $"Owner: {ai.Owner ?? "Unassigned"}"
    }).ToList(),
    channel: "#retrospectives"
);
```

See [Slack Integration Guide](../integrations/slack-integration.md) for more examples.

## Related Documentation

- [PM Guide](../role-guides/pm-guide.md)
- [Slack Integration Guide](../integrations/slack-integration.md) ⭐ NEW
- [Getting Started](../getting-started/)
