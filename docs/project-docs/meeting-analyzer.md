# Meeting Transcript Analyzer

## Overview

The Meeting Transcript Analyzer helps Product Managers, Business Analysts, and Scrum Masters transcribe meetings, generate summaries, extract action items, and create follow-up emails.

## Features

- **Audio Transcription**: Transcribe audio/video files using Whisper API
- **Meeting Summaries**: Generate concise meeting summaries
- **Action Item Extraction**: Automatically extract action items with owners
- **Follow-up Emails**: Generate professional follow-up emails
- **Search**: Search meeting history using embeddings

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
│ Meeting Service │
└────────┬────────┘
         │
┌────────▼────────┐
│  OpenAI Client  │
│  (Whisper + GPT)│
└─────────────────┘
```

## Setup

1. Configure OpenAI API key (see [Setup Guide](../getting-started/01-setup.md))

2. Run the API:
```bash
cd src/MeetingAnalyzer/MeetingAnalyzer.Api
dotnet run
```

3. Run the Web UI:
```bash
cd src/MeetingAnalyzer/MeetingAnalyzer.Web
dotnet run
```

## API Endpoints

### POST /api/meeting/transcribe

Transcribes audio file.

**Request:** Multipart form data with audio file

**Response:**
```json
{
  "transcript": "Full transcript text..."
}
```

### POST /api/meeting/summarize

Generates meeting summary from transcript.

**Request:**
```json
{
  "transcript": "Meeting transcript text..."
}
```

**Response:**
```json
{
  "summary": "Meeting summary..."
}
```

### POST /api/meeting/extract-action-items

Extracts action items from transcript.

**Request:**
```json
{
  "transcript": "Meeting transcript text..."
}
```

**Response:**
```json
[
  {
    "id": "guid",
    "description": "Action item description",
    "owner": "John Doe",
    "dueDate": "2024-12-31",
    "priority": "High"
  }
]
```

### POST /api/meeting/generate-email

Generates follow-up email.

**Request:**
```json
{
  "transcript": "Meeting transcript text...",
  "attendees": ["person1@example.com", "person2@example.com"]
}
```

**Response:**
```json
{
  "email": "Email content..."
}
```

## Usage Examples

### Transcribe Audio

```csharp
var meetingService = new MeetingService(openAIClient, logger);

using var audioStream = File.OpenRead("meeting.mp3");
var transcript = await meetingService.TranscribeAudioAsync(
    audioStream,
    "meeting.mp3",
    language: "en"
);

Console.WriteLine(transcript);
```

### Generate Summary

```csharp
var summary = await meetingService.GenerateSummaryAsync(transcript);
Console.WriteLine(summary);
```

### Extract Action Items

```csharp
var actionItems = await meetingService.ExtractActionItemsAsync(transcript);

foreach (var item in actionItems)
{
    Console.WriteLine($"{item.Description} - Owner: {item.Owner}");
    if (item.DueDate.HasValue)
    {
        Console.WriteLine($"Due: {item.DueDate.Value:yyyy-MM-dd}");
    }
}
```

### Generate Follow-up Email

```csharp
var attendees = new List<string> { "person1@example.com", "person2@example.com" };
var email = await meetingService.GenerateFollowUpEmailAsync(transcript, attendees);
Console.WriteLine(email);
```

## Supported Audio Formats

Whisper API supports:
- MP3
- MP4
- M4A
- WAV
- WebM

## Workflow

1. **Record Meeting**: Record audio/video of meeting
2. **Upload**: Upload audio file to Meeting Analyzer
3. **Transcribe**: Automatically transcribe using Whisper API
4. **Analyze**: Generate summary and extract action items
5. **Follow-up**: Send follow-up email to attendees
6. **Create Tickets**: Optionally create Jira tickets for action items

## Integration with Jira

After extracting action items, create Jira tickets:

```csharp
var jiraIntegration = new JiraIntegration(httpClient, logger, baseUrl, username, token);
var ticketKeys = await jiraIntegration.CreateTicketsFromActionItemsAsync(
    actionItems,
    "PROJ"
);
```

## Best Practices

1. **Good Audio Quality**: Ensure clear audio for better transcription
2. **Specify Language**: Provide language parameter for better accuracy
3. **Review Transcripts**: Always review transcripts for accuracy
4. **Validate Action Items**: Verify extracted action items are correct
5. **Follow Up**: Send follow-up emails promptly after meetings

## Troubleshooting

**Issue:** Transcription errors
- **Solution:** Ensure good audio quality, specify correct language

**Issue:** Action items missing owners
- **Solution:** Ensure names are mentioned clearly in meeting

**Issue:** Summary lacks detail
- **Solution:** Longer meetings may need section-by-section summarization

**Issue:** File upload fails
- **Solution:** Check file size limits, ensure supported format

## Slack Integration

Share meeting summaries and action items with attendees via Slack:

```csharp
var slackIntegration = new SlackIntegration(httpClient, logger, slackWebhookUrl);

// After meeting analysis
await slackIntegration.SendFormattedMessageAsync(
    title: "📝 Meeting Summary",
    text: summary,
    fields: actionItems.Select(ai => new SlackField
    {
        Title = ai.Description,
        Value = $"Owner: {ai.Owner ?? "Unassigned"}"
    }).ToList(),
    channel: "#meetings"
);
```

See [Slack Integration Guide](../integrations/slack-integration.md) for more examples.

## Related Documentation

- [PM Guide](../role-guides/pm-guide.md)
- [BA Guide](../role-guides/ba-guide.md)
- [Slack Integration Guide](../integrations/slack-integration.md) ⭐ NEW
- [Getting Started](../getting-started/)
