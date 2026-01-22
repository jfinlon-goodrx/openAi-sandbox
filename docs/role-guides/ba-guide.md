# Business Analyst Guide

This guide helps Business Analysts leverage OpenAI Platform capabilities for requirements analysis and documentation.

## Use Cases for Business Analysts

### 1. Requirements Processing

Process requirement documents and generate user stories:

```csharp
var requirementsService = new RequirementsService(openAIClient, logger);

// Summarize requirements
var summary = await requirementsService.SummarizeDocumentAsync(documentContent);

// Generate user stories
var userStories = await requirementsService.GenerateUserStoriesAsync(documentContent);

foreach (var story in userStories)
{
    Console.WriteLine($"As a {story.AsA}, I want {story.IWant}, so that {story.SoThat}");
    Console.WriteLine("Acceptance Criteria:");
    foreach (var criteria in story.AcceptanceCriteria)
    {
        Console.WriteLine($"  - {criteria}");
    }
}
```

### 2. Confluence Integration

Pull requirements from Confluence:

```csharp
var confluence = new ConfluenceIntegration(
    requirementsService,
    httpClient,
    logger,
    "https://your-domain.atlassian.net/wiki",
    "your-email@example.com",
    "api-token"
);

// Process a Confluence page
var stories = await confluence.ProcessConfluencePageAsync("123456");

// Search for requirements
var results = await confluence.SearchRequirementsAsync("payment processing");
```

### 3. Gap Analysis

Identify gaps and conflicts in requirements:

```csharp
var analysis = await requirementsService.IdentifyGapsAndConflictsAsync(documentContent);
Console.WriteLine(analysis);
```

### 4. Q&A Over Requirements

Answer questions about requirements documents:

```csharp
var answer = await requirementsService.AnswerQuestionAsync(
    "What are the security requirements?",
    documentContent
);
Console.WriteLine(answer);
```

## Daily Workflow Examples

### Morning: Process New Requirements

1. Pull requirements from Confluence
2. Generate summary
3. Extract user stories
4. Identify gaps

### During Sprints: Answer Questions

Use the Q&A interface to quickly answer stakeholder questions about requirements.

### Sprint Planning: Generate User Stories

Convert stakeholder input into well-structured user stories with acceptance criteria.

## Best Practices

1. **Review Generated Stories**: Always review and refine AI-generated user stories
2. **Provide Context**: Include domain knowledge and business context in prompts
3. **Iterate**: Use AI as a starting point, then refine based on team feedback
4. **Maintain Traceability**: Link generated stories back to source documents
5. **Validate**: Ensure acceptance criteria are testable

## Common Patterns

### Requirements Analysis Pattern

```csharp
public async Task<RequirementsAnalysis> AnalyzeRequirements(string documentContent)
{
    // Step 1: Summarize
    var summary = await _requirementsService.SummarizeDocumentAsync(documentContent);
    
    // Step 2: Generate stories
    var stories = await _requirementsService.GenerateUserStoriesAsync(documentContent);
    
    // Step 3: Identify gaps
    var gaps = await _requirementsService.IdentifyGapsAndConflictsAsync(documentContent);
    
    return new RequirementsAnalysis
    {
        Summary = summary,
        UserStories = stories,
        GapsAndConflicts = gaps
    };
}
```

### Confluence Workflow Pattern

```csharp
public async Task ProcessConfluenceRequirements(string pageId)
{
    // Pull from Confluence
    var stories = await _confluence.ProcessConfluencePageAsync(pageId);
    
    // Export to Jira (if integrated)
    foreach (var story in stories)
    {
        // Create Jira ticket
        await CreateJiraTicket(story);
    }
}
```

## Integration with Jira

After generating user stories, create Jira tickets:

```csharp
// This would integrate with your Jira instance
foreach (var story in userStories)
{
    var ticket = new JiraTicket
    {
        Summary = story.Title,
        Description = story.Description,
        IssueType = "Story",
        AcceptanceCriteria = story.AcceptanceCriteria
    };
    
    await CreateJiraTicket(ticket);
}
```

## Troubleshooting

**Issue:** Generated stories don't match business needs
- **Solution:** Provide more context in the document, include examples of desired format

**Issue:** Missing requirements in analysis
- **Solution:** Break large documents into sections, analyze each separately

**Issue:** Stories lack detail
- **Solution:** Use more specific prompts, provide examples of well-written stories

## Slack Integration

Share requirements summaries and user story generation results with stakeholders via Slack:

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

## Resources

- [Requirements Assistant Documentation](../project-docs/requirements-assistant.md)
- [Slack Integration Guide](../integrations/slack-integration.md) ⭐ NEW
- [Getting Started Guide](../getting-started/)
- [Best Practices](../best-practices/)
