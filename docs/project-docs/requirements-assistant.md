# Requirements Processing Assistant

**For:** Business Analysts and Product Managers who need to process requirements documents and generate user stories.

**What you'll learn:** How to use AI to summarize requirements, generate user stories with acceptance criteria, perform gap analysis, and answer questions about requirements documents.

## Overview

The Requirements Processing Assistant helps Business Analysts and Product Managers process requirement documents, generate user stories, and answer questions about requirements.

**Why use AI for requirements processing?**
- **Speed:** Process large requirement documents quickly
- **Consistency:** Generate standardized user stories with consistent format
- **Completeness:** AI can identify gaps and missing requirements
- **Q&A:** Quickly answer questions about requirements without re-reading entire documents
- **Integration:** Pull requirements directly from Confluence for seamless workflow

## Features

- **Document Summarization**: Extract key points from requirement documents
- **User Story Generation**: Automatically generate user stories with acceptance criteria
- **Gap Analysis**: Identify missing requirements and conflicts
- **Q&A Interface**: Ask questions about requirements documents
- **Confluence Integration**: Pull requirements directly from Confluence

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
│ Requirements    │
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
cd src/RequirementsAssistant/RequirementsAssistant.Api
dotnet run
```

3. Run the Web UI:
```bash
cd src/RequirementsAssistant/RequirementsAssistant.Web
dotnet run
```

4. Navigate to `https://localhost:5001`

## API Endpoints

### POST /api/requirements/summarize

Summarizes a requirements document.

**Request:**
```json
{
  "content": "Requirements document text..."
}
```

**Response:**
```json
{
  "summary": "Summary text..."
}
```

### POST /api/requirements/generate-user-stories

Generates user stories from requirements.

**Request:**
```json
{
  "content": "Requirements document text..."
}
```

**Response:**
```json
[
  {
    "id": "guid",
    "title": "User Story Title",
    "asA": "As a user",
    "iWant": "I want to...",
    "soThat": "so that...",
    "acceptanceCriteria": ["Criteria 1", "Criteria 2"],
    "priority": "High",
    "tags": ["tag1", "tag2"]
  }
]
```

### POST /api/requirements/ask

Answers questions about requirements.

**Request:**
```json
{
  "question": "What are the security requirements?",
  "documentContent": "Requirements document text..."
}
```

**Response:**
```json
{
  "answer": "Answer text..."
}
```

### POST /api/requirements/analyze

Identifies gaps and conflicts.

**Request:**
```json
{
  "content": "Requirements document text..."
}
```

**Response:**
```json
{
  "analysis": "Analysis text..."
}
```

## Confluence Integration

### Setup

1. Get Confluence API token from [Atlassian Account Settings](https://id.atlassian.com/manage-profile/security/api-tokens)

2. Configure in `appsettings.json`:
```json
{
  "Confluence": {
    "BaseUrl": "https://your-domain.atlassian.net/wiki",
    "Username": "your-email@example.com",
    "ApiToken": "your-api-token"
  }
}
```

### Usage

```csharp
var confluence = new ConfluenceIntegration(
    requirementsService,
    httpClient,
    logger,
    baseUrl,
    username,
    apiToken
);

// Process a Confluence page
var stories = await confluence.ProcessConfluencePageAsync("123456");

// Search for requirements
var results = await confluence.SearchRequirementsAsync("payment processing");
```

## Example Usage

### Generate User Stories

```csharp
var requirementsService = new RequirementsService(openAIClient, logger);

var documentContent = @"
The system shall allow users to create accounts.
Users must provide email and password.
Passwords must be at least 8 characters.
";

var stories = await requirementsService.GenerateUserStoriesAsync(documentContent);

foreach (var story in stories)
{
    Console.WriteLine($"As a {story.AsA}");
    Console.WriteLine($"I want {story.IWant}");
    Console.WriteLine($"So that {story.SoThat}");
    Console.WriteLine("Acceptance Criteria:");
    foreach (var criteria in story.AcceptanceCriteria)
    {
        Console.WriteLine($"  - {criteria}");
    }
}
```

## Troubleshooting

**Issue:** User stories lack detail
- **Solution:** Provide more context in the requirements document, include examples

**Issue:** Confluence integration fails
- **Solution:** Verify API token and base URL, check permissions

**Issue:** API timeout
- **Solution:** Large documents may take time. Consider breaking into sections

## Best Practices

1. **Provide Context**: Include domain knowledge and business context
2. **Review Output**: Always review and refine generated user stories
3. **Iterate**: Use AI as a starting point, then refine manually
4. **Maintain Traceability**: Link stories back to source documents

## Slack Integration

Share requirements summaries and user stories with stakeholders via Slack:

```csharp
var slackIntegration = new SlackIntegration(httpClient, logger, slackWebhookUrl);

// After processing requirements
await slackIntegration.SendFormattedMessageAsync(
    title: "📋 Requirements Processed",
    text: $"Summary: {summary}\n\nUser Stories Generated: {userStories.Count}",
    fields: userStories.Select(us => new SlackField
    {
        Title = us.Title,
        Value = $"As a {us.AsA}, I want {us.IWant}"
    }).ToList(),
    channel: "#requirements"
);
```

See [Slack Integration Guide](../integrations/slack-integration.md) for more examples.

## Related Documentation

- [BA Guide](../role-guides/ba-guide.md)
- [PM Guide](../role-guides/pm-guide.md)
- [Slack Integration Guide](../integrations/slack-integration.md) ⭐ NEW
- [Getting Started](../getting-started/)
