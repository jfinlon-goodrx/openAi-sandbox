# SDM Assistant

## Overview

The SDM Assistant helps Software Development Managers streamline daily workflows with Jira and Confluence, automate status reports, identify risks, and make data-driven decisions.

## Features

- **Daily Activity Summary**: Analyze yesterday's Jira activity
- **Standup Preparation**: Generate talking points for daily standups
- **Velocity Analysis**: Analyze team velocity trends
- **Sprint Planning**: AI-assisted sprint planning
- **Risk Identification**: Proactively identify project risks
- **Status Reports**: Automated executive status reports
- **Sprint Health Analysis**: Monitor sprint progress and health

## Architecture

```
┌─────────────────┐
│   Web API       │
└────────┬────────┘
         │
┌────────▼────────┐
│  SDM Service    │
└────────┬────────┘
         │
    ┌────┴────┐
    │         │
┌───▼───┐ ┌───▼────────┐
│ Jira  │ │ Confluence │
│ API   │ │ API        │
└───────┘ └────────────┘
```

## Setup

1. Configure OpenAI API key (see [Setup Guide](../getting-started/01-setup.md))

2. Configure Jira:
```json
{
  "Jira": {
    "BaseUrl": "https://your-domain.atlassian.net",
    "Username": "your-email@example.com",
    "ApiToken": "your-jira-api-token",
    "ProjectKey": "PROJ"
  }
}
```

3. Configure Confluence:
```json
{
  "Confluence": {
    "BaseUrl": "https://your-domain.atlassian.net/wiki",
    "Username": "your-email@example.com",
    "ApiToken": "your-confluence-api-token",
    "DefaultSpace": "ENG"
  }
}
```

4. Run the API:
```bash
cd src/SDMAssistant/SDMAssistant.Api
dotnet run
```

5. Navigate to `https://localhost:7006/swagger` for API documentation

## API Endpoints

### POST /api/sdm/daily-summary

Gets daily activity summary from Jira.

**Request:**
```json
{
  "projectKey": "PROJ",
  "date": "2024-01-15"
}
```

**Response:**
```json
{
  "date": "2024-01-15T00:00:00Z",
  "summary": "Summary of activities...",
  "ticketsAnalyzed": 12
}
```

### POST /api/sdm/standup-talking-points

Generates talking points for standup.

**Request:**
```json
{
  "date": "2024-01-15T00:00:00Z",
  "summary": "Daily activity summary...",
  "ticketsAnalyzed": 12
}
```

**Response:**
```json
[
  "Completed 5 tickets yesterday",
  "2 blockers identified: PROJ-123, PROJ-456",
  "Risk: PROJ-789 may slip due to dependency"
]
```

### POST /api/sdm/analyze-velocity

Analyzes team velocity.

**Request:**
```json
{
  "projectKey": "PROJ",
  "sprintCount": 5
}
```

**Response:**
```json
{
  "sprintsAnalyzed": 5,
  "averageVelocity": 42.5,
  "analysis": "Team velocity analysis..."
}
```

### POST /api/sdm/sprint-plan

Generates sprint plan.

**Request:**
```json
{
  "projectKey": "PROJ",
  "sprintGoal": "Complete payment integration",
  "teamCapacity": 40
}
```

**Response:**
```json
{
  "sprintGoal": "Complete payment integration",
  "teamCapacity": 40,
  "plan": "Sprint planning details...",
  "generatedAt": "2024-01-15T10:00:00Z"
}
```

### POST /api/sdm/identify-risks

Identifies project risks.

**Request:**
```json
{
  "projectKey": "PROJ",
  "sprintId": "123"
}
```

**Response:**
```json
[
  {
    "description": "PROJ-123 blocked by external dependency",
    "severity": "High",
    "mitigation": "Escalate to dependency owner"
  }
]
```

### POST /api/sdm/status-report

Generates status report.

**Request:**
```json
{
  "projectKey": "PROJ",
  "startDate": "2024-01-08T00:00:00Z",
  "endDate": "2024-01-15T00:00:00Z",
  "includeMetrics": true
}
```

## Example Usage

### Daily Standup Preparation

```csharp
var sdmService = new SDMService(openAIClient, jiraIntegration, confluenceIntegration, logger);

// Get yesterday's summary
var summary = await sdmService.GetDailyActivitySummaryAsync("PROJ", DateTime.UtcNow.AddDays(-1));

// Generate talking points
var points = await sdmService.GenerateStandupTalkingPointsAsync(summary);

foreach (var point in points)
{
    Console.WriteLine($"• {point}");
}
```

### Sprint Planning

```csharp
// Analyze velocity
var velocity = await sdmService.AnalyzeTeamVelocityAsync("PROJ", sprintCount: 5);

// Generate sprint plan
var plan = await sdmService.GenerateSprintPlanAsync(
    "PROJ",
    sprintGoal: "Complete payment integration",
    teamCapacity: (int)velocity.AverageVelocity
);
```

### Risk Management

```csharp
// Identify risks
var risks = await sdmService.IdentifyRisksAsync("PROJ", sprintId: "123");

// Filter high severity risks
var highRisks = risks.Where(r => r.Severity == "High").ToList();

foreach (var risk in highRisks)
{
    Console.WriteLine($"Risk: {risk.Description}");
    Console.WriteLine($"Mitigation: {risk.Mitigation}");
}
```

## Integration with Jira and Confluence

The SDM Assistant integrates deeply with Jira and Confluence:

- **Jira**: Fetches tickets, sprint data, metrics, backlog
- **Confluence**: Creates/updates pages, generates documentation

See [SDM Guide](../role-guides/sdm-guide.md) for detailed integration workflows.

## Troubleshooting

**Issue:** Jira API rate limits
- **Solution:** Implement caching, batch operations

**Issue:** Confluence page creation fails
- **Solution:** Verify space key and permissions

**Issue:** Velocity analysis inaccurate
- **Solution:** Ensure sprint data is complete, check story point fields

## Resources

- [SDM Guide](../role-guides/sdm-guide.md) - Comprehensive SDM workflows
- [Jira API Documentation](https://developer.atlassian.com/cloud/jira/platform/rest/v3/)
- [Confluence API Documentation](https://developer.atlassian.com/cloud/confluence/rest/v2/)
