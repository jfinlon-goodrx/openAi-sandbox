# Publishing Assistant

## Overview

The Publishing Assistant helps publishing companies automate various workflows including manuscript reviews, summary generation, marketing copy creation, cover image descriptions, and file format conversions.

## Features

- **Book Review Generation**: Comprehensive manuscript reviews with ratings, strengths, weaknesses, and recommendations
- **Summary Generation**: Create compelling book summaries/blurbs for back covers
- **Marketing Blurb Creation**: Generate marketing copy including headlines, taglines, and social media posts
- **Cover Image Description**: Generate detailed cover image descriptions for DALL-E or other image generators
- **File Format Conversion**: Convert markdown manuscripts to HTML, plain text, EPUB, and PDF
- **Editorial Notes**: Generate detailed editorial feedback and suggestions

## Architecture

```
┌─────────────────┐
│   Web API       │
└────────┬────────┘
         │
┌────────▼────────┐
│ Publishing      │
│ Service         │
└────────┬────────┘
         │
┌────────▼────────┐
│  OpenAI Client  │
│  (GPT-4 + DALL-E)│
└─────────────────┘
```

## Setup

1. Configure OpenAI API key (see [Setup Guide](../getting-started/01-setup.md))

2. Run the API:
```bash
cd src/PublishingAssistant/PublishingAssistant.Api
dotnet run
```

3. Navigate to `https://localhost:7003/swagger` for API documentation

## API Endpoints

### POST /api/publishing/review

Generates a comprehensive book review.

**Request:**
```json
{
  "content": "Manuscript content...",
  "genre": "Science Fiction"
}
```

**Response:**
```json
{
  "overallRating": 4.5,
  "plotSummary": "Summary text...",
  "strengths": ["Strong character development", "Engaging plot"],
  "weaknesses": ["Pacing issues in middle section"],
  "characterAnalysis": "Characters are well-developed...",
  "writingStyle": "Clear and engaging prose...",
  "recommendations": "Consider tightening the middle section...",
  "targetAudience": "Young Adult to Adult readers"
}
```

### POST /api/publishing/summary

Generates a book summary/blurb.

**Request:**
```json
{
  "content": "Manuscript content...",
  "maxLength": 250
}
```

**Response:**
```json
{
  "summary": "Compelling summary text..."
}
```

### POST /api/publishing/marketing-blurb

Generates marketing copy.

**Request:**
```json
{
  "content": "Manuscript content...",
  "targetAudience": "Young Adult Fiction Readers"
}
```

**Response:**
```json
{
  "headline": "A Thrilling Journey Through Time",
  "shortBlurb": "Short marketing blurb...",
  "longBlurb": "Longer marketing blurb...",
  "tagline": "One compelling tagline",
  "keySellingPoints": ["Point 1", "Point 2", "Point 3"],
  "socialMediaPosts": {
    "twitter": "Tweet text...",
    "facebook": "Facebook post...",
    "instagram": "Instagram post..."
  }
}
```

### POST /api/publishing/cover-description

Generates cover image description.

**Request:**
```json
{
  "content": "Manuscript content...",
  "genre": "Mystery"
}
```

**Response:**
```json
{
  "description": "Detailed image description for DALL-E...",
  "style": "Minimalist",
  "mood": "Mysterious",
  "colorPalette": ["#2C3E50", "#E74C3C", "#FFFFFF"],
  "keyElements": ["Silhouette", "Mysterious figure", "Fog"],
  "typography": "Bold sans-serif"
}
```

### POST /api/publishing/convert

Converts markdown to various formats.

**Request:**
```json
{
  "markdownContent": "# Title\n\nContent...",
  "targetFormat": "html"
}
```

**Response:**
```json
{
  "content": "<h1>Title</h1><p>Content...</p>",
  "format": "html"
}
```

### POST /api/publishing/editorial-notes

Generates editorial notes.

**Request:**
```json
{
  "content": "Manuscript content..."
}
```

**Response:**
```json
{
  "notes": "Detailed editorial notes..."
}
```

## Usage Examples

### Complete Publishing Workflow

```csharp
var publishingService = new PublishingService(openAIClient, logger);

// 1. Generate review
var review = await publishingService.GenerateReviewAsync(manuscript, "Fiction");
Console.WriteLine($"Rating: {review.OverallRating}/5");

// 2. Generate summary
var summary = await publishingService.GenerateSummaryAsync(manuscript, maxLength: 200);
Console.WriteLine($"Summary: {summary}");

// 3. Generate marketing blurb
var marketing = await publishingService.GenerateMarketingBlurbAsync(
    manuscript,
    targetAudience: "Young Adults"
);
Console.WriteLine($"Headline: {marketing.Headline}");
Console.WriteLine($"Tagline: {marketing.Tagline}");

// 4. Generate cover description
var coverDesc = await publishingService.GenerateCoverImageDescriptionAsync(
    manuscript,
    genre: "Mystery"
);
Console.WriteLine($"Cover Description: {coverDesc.Description}");

// 5. Generate editorial notes
var notes = await publishingService.GenerateEditorialNotesAsync(manuscript);
Console.WriteLine($"Editorial Notes:\n{notes}");

// 6. Convert to HTML
var html = await publishingService.ConvertMarkdownToFormatAsync(manuscript, "html");
```

### Cover Image Generation from Markdown

```csharp
// Read markdown repository
var markdownFiles = Directory.GetFiles("manuscript/", "*.md");
var fullManuscript = string.Join("\n\n", markdownFiles.Select(File.ReadAllText));

// Generate cover description
var coverDesc = await publishingService.GenerateCoverImageDescriptionAsync(
    fullManuscript,
    genre: "Science Fiction"
);

// Use description with DALL-E or other image generator
var imageGenerator = new CoverImageGenerator(openAIClient, logger);
var imageUrl = await imageGenerator.GenerateCoverImageAsync(coverDesc.Description);
```

## Integration with DALL-E

To generate actual cover images, you'll need DALL-E API access:

```csharp
// Generate cover description
var coverDesc = await publishingService.GenerateCoverImageDescriptionAsync(manuscript);

// Use with DALL-E API (requires DALL-E API integration)
// var imageUrl = await dalleClient.GenerateImageAsync(coverDesc.Description);
```

## File Format Conversion

Supported formats:
- **HTML**: Full HTML output
- **Plaintext**: Text-only version
- **EPUB**: EPUB structure (XML)
- **PDF**: HTML output (can be converted to PDF using libraries)

```csharp
var markdown = File.ReadAllText("manuscript.md");

// Convert to HTML
var html = await publishingService.ConvertMarkdownToFormatAsync(markdown, "html");

// Convert to plain text
var text = await publishingService.ConvertMarkdownToFormatAsync(markdown, "plaintext");
```

## Best Practices

1. **Provide Context**: Always include genre and target audience for better results
2. **Review Output**: Always review AI-generated content before publishing
3. **Iterate**: Refine prompts based on your publishing standards
4. **Combine Outputs**: Use review + summary + marketing together
5. **Format Consistency**: Use markdown for easy conversion

## Use Cases

### For Editors
- Generate initial reviews and editorial notes
- Identify strengths and weaknesses
- Get recommendations for improvements

### For Marketing Teams
- Create marketing blurbs and taglines
- Generate social media content
- Develop key selling points

### For Design Teams
- Get cover image descriptions
- Understand mood and style requirements
- Receive color palette suggestions

### For Production
- Convert manuscripts to various formats
- Generate HTML for web publishing
- Create EPUB structures

## Troubleshooting

**Issue:** Reviews lack detail
- **Solution:** Provide more context, include genre and target audience

**Issue:** Cover descriptions too generic
- **Solution:** Include genre, provide more manuscript context

**Issue:** Format conversion errors
- **Solution:** Ensure markdown is well-formed, check for special characters

## Related Documentation

- [Getting Started](../getting-started/)
- [Best Practices](../best-practices/)
- [Examples](../../samples/PublishingExamples/)
