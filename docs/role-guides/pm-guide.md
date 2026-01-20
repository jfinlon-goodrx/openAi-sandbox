# Product Manager Guide

This guide helps Product Managers leverage OpenAI Platform capabilities for product planning and team management.

## Use Cases for Product Managers

### 1. Sprint Retrospective Analysis

Analyze retrospective comments to extract insights:

```csharp
var retroService = new RetroAnalyzerService(openAIClient, logger);

// Extract action items
var actionItems = await retroService.ExtractActionItemsAsync(comments);

// Identify themes
var themes = await retroService.IdentifyThemesAsync(comments);

// Analyze sentiment
var sentiment = await retroService.AnalyzeSentimentAsync(comments);

// Get improvement suggestions
var suggestions = await retroService.GenerateImprovementSuggestionsAsync(comments);
```

### 2. Meeting Analysis

Process meeting transcripts:

```csharp
var meetingService = new MeetingService(openAIClient, logger);

// Transcribe audio
var transcript = await meetingService.TranscribeAudioAsync(audioStream, "meeting.mp3");

// Generate summary
var summary = await meetingService.GenerateSummaryAsync(transcript);

// Extract action items
var actionItems = await meetingService.ExtractActionItemsAsync(transcript);

// Generate follow-up email
var email = await meetingService.GenerateFollowUpEmailAsync(transcript, attendees);
```

### 3. Requirements Analysis

Review and analyze requirements documents:

```csharp
var requirementsService = new RequirementsService(openAIClient, logger);

// Summarize requirements
var summary = await requirementsService.SummarizeDocumentAsync(documentContent);

// Generate user stories
var stories = await requirementsService.GenerateUserStoriesAsync(documentContent);
```

### 4. Jira Integration

Create Jira tickets from action items:

```csharp
var jiraIntegration = new JiraIntegration(httpClient, logger, baseUrl, username, token);

// Create tickets from retro action items
var ticketKeys = await jiraIntegration.CreateTicketsFromActionItemsAsync(
    actionItems,
    "PROJ"
);
```

## Daily Workflow Examples

### After Sprint Retrospective

1. Collect retrospective comments
2. Analyze with Retro Analyzer
3. Extract action items
4. Create Jira tickets automatically
5. Share insights with team

### After Meetings

1. Upload meeting recording
2. Transcribe with Whisper API
3. Generate summary and action items
4. Send follow-up email to attendees

### Sprint Planning

1. Pull requirements from Confluence
2. Generate user stories
3. Review and refine
4. Create Jira tickets

## Best Practices

1. **Review AI Output**: Always review and validate AI-generated content
2. **Provide Context**: Include business context in prompts
3. **Iterate**: Use AI as a starting point, refine based on team input
4. **Track Metrics**: Monitor team sentiment and action item completion
5. **Integrate Tools**: Connect with Jira, Confluence, and other tools

## Common Patterns

### Retrospective Analysis Pattern

```csharp
public async Task<RetrospectiveAnalysis> AnalyzeRetrospective(List<string> comments)
{
    var retroService = new RetroAnalyzerService(openAIClient, logger);
    
    var analysis = new RetrospectiveAnalysis
    {
        ActionItems = await retroService.ExtractActionItemsAsync(comments),
        Themes = await retroService.IdentifyThemesAsync(comments),
        Sentiment = await retroService.AnalyzeSentimentAsync(comments),
        Suggestions = await retroService.GenerateImprovementSuggestionsAsync(comments)
    };
    
    // Create Jira tickets
    var jira = new JiraIntegration(httpClient, logger, baseUrl, username, token);
    await jira.CreateTicketsFromActionItemsAsync(analysis.ActionItems, "PROJ");
    
    return analysis;
}
```

### Meeting Follow-up Pattern

```csharp
public async Task ProcessMeeting(Stream audioFile, List<string> attendees)
{
    var meetingService = new MeetingService(openAIClient, logger);
    
    // Transcribe
    var transcript = await meetingService.TranscribeAudioAsync(audioFile, "meeting.mp3");
    
    // Generate outputs
    var summary = await meetingService.GenerateSummaryAsync(transcript);
    var actionItems = await meetingService.ExtractActionItemsAsync(transcript);
    var email = await meetingService.GenerateFollowUpEmailAsync(transcript, attendees);
    
    // Send email
    await SendEmail(attendees, "Meeting Summary", email);
    
    // Create Jira tickets for action items
    var jira = new JiraIntegration(httpClient, logger, baseUrl, username, token);
    await jira.CreateTicketsFromActionItemsAsync(actionItems, "PROJ");
}
```

## Integration Workflows

### Confluence → Requirements → Jira

1. Pull requirements from Confluence
2. Generate user stories
3. Create Jira tickets automatically

### Retro → Jira

1. Analyze retrospective comments
2. Extract action items
3. Create Jira tickets with owners

### Meeting → Email → Jira

1. Transcribe meeting
2. Generate summary and action items
3. Send follow-up email
4. Create Jira tickets

## Troubleshooting

**Issue:** Action items lack detail
- **Solution:** Provide more context in prompts, include examples

**Issue:** Sentiment analysis seems inaccurate
- **Solution:** Review comments manually, sentiment analysis is a guide, not definitive

**Issue:** Meeting transcription errors
- **Solution:** Ensure good audio quality, use appropriate language parameter

## Resources

- [Retrospective Analyzer Documentation](../project-docs/retro-analyzer.md)
- [Meeting Analyzer Documentation](../project-docs/meeting-analyzer.md)
- [Requirements Assistant Documentation](../project-docs/requirements-assistant.md)
- [Best Practices](../best-practices/)
