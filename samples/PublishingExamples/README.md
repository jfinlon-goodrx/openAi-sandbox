# Publishing Assistant Examples

This directory contains examples for using the Publishing Assistant API for various publishing workflows.

## Examples

### 1. Book Review Generation

Generate comprehensive book reviews from manuscript content.

```csharp
var publishingService = new PublishingService(openAIClient, logger);

var manuscript = File.ReadAllText("manuscript.md");
var review = await publishingService.GenerateReviewAsync(manuscript, genre: "Science Fiction");

Console.WriteLine($"Rating: {review.OverallRating}/5");
Console.WriteLine($"Plot Summary: {review.PlotSummary}");
Console.WriteLine("\nStrengths:");
foreach (var strength in review.Strengths)
{
    Console.WriteLine($"- {strength}");
}
```

### 2. Marketing Blurb Generation

Create marketing copy for book promotion.

```csharp
var blurb = await publishingService.GenerateMarketingBlurbAsync(
    manuscript,
    targetAudience: "Young Adult Fiction Readers"
);

Console.WriteLine($"Headline: {blurb.Headline}");
Console.WriteLine($"Tagline: {blurb.Tagline}");
Console.WriteLine($"\nShort Blurb:\n{blurb.ShortBlurb}");
Console.WriteLine($"\nKey Selling Points:");
foreach (var point in blurb.KeySellingPoints)
{
    Console.WriteLine($"- {point}");
}
```

### 3. Cover Image Description

Generate cover image descriptions for DALL-E or other image generators.

```csharp
var coverDescription = await publishingService.GenerateCoverImageDescriptionAsync(
    manuscript,
    genre: "Mystery"
);

Console.WriteLine($"Cover Description: {coverDescription.Description}");
Console.WriteLine($"Style: {coverDescription.Style}");
Console.WriteLine($"Mood: {coverDescription.Mood}");
Console.WriteLine($"Colors: {string.Join(", ", coverDescription.ColorPalette)}");
```

### 4. Markdown to Format Conversion

Convert markdown manuscripts to various formats.

```csharp
var markdown = File.ReadAllText("manuscript.md");

// Convert to HTML
var html = await publishingService.ConvertMarkdownToFormatAsync(markdown, "html");

// Convert to plain text
var plainText = await publishingService.ConvertMarkdownToFormatAsync(markdown, "plaintext");

// Convert to EPUB (structure)
var epub = await publishingService.ConvertMarkdownToFormatAsync(markdown, "epub");
```

### 5. Editorial Notes

Generate editorial feedback and suggestions.

```csharp
var notes = await publishingService.GenerateEditorialNotesAsync(manuscript);
Console.WriteLine(notes);
```

### 6. Complete Publishing Workflow

End-to-end workflow example:

```csharp
// 1. Generate review
var review = await publishingService.GenerateReviewAsync(manuscript);

// 2. Generate summary
var summary = await publishingService.GenerateSummaryAsync(manuscript, maxLength: 200);

// 3. Generate marketing blurb
var marketing = await publishingService.GenerateMarketingBlurbAsync(manuscript);

// 4. Generate cover description
var coverDesc = await publishingService.GenerateCoverImageDescriptionAsync(manuscript);

// 5. Generate editorial notes
var notes = await publishingService.GenerateEditorialNotesAsync(manuscript);

// 6. Convert to formats
var html = await publishingService.ConvertMarkdownToFormatAsync(manuscript, "html");
```

## API Usage

### Using the REST API

Start the API:
```bash
cd src/PublishingAssistant/PublishingAssistant.Api
dotnet run
```

Then use the endpoints:

**Generate Review:**
```bash
curl -X POST https://localhost:7003/api/publishing/review \
  -H "Content-Type: application/json" \
  -d '{"content": "Manuscript content...", "genre": "Fiction"}'
```

**Generate Summary:**
```bash
curl -X POST https://localhost:7003/api/publishing/summary \
  -H "Content-Type: application/json" \
  -d '{"content": "Manuscript content...", "maxLength": 250}'
```

**Generate Marketing Blurb:**
```bash
curl -X POST https://localhost:7003/api/publishing/marketing-blurb \
  -H "Content-Type: application/json" \
  -d '{"content": "Manuscript content...", "targetAudience": "Young Adults"}'
```

**Generate Cover Description:**
```bash
curl -X POST https://localhost:7003/api/publishing/cover-description \
  -H "Content-Type: application/json" \
  -d '{"content": "Manuscript content...", "genre": "Mystery"}'
```

## Integration with DALL-E

To generate actual cover images, integrate with DALL-E API:

```csharp
// Get cover description
var coverDesc = await publishingService.GenerateCoverImageDescriptionAsync(manuscript);

// Generate image using DALL-E (requires DALL-E API access)
// var imageUrl = await dalleClient.GenerateImageAsync(coverDesc.Description);
```

## Best Practices

1. **Provide Context**: Include genre and target audience for better results
2. **Review Output**: Always review AI-generated content before publishing
3. **Iterate**: Refine prompts based on your publishing standards
4. **Combine**: Use multiple outputs together (review + summary + marketing)
5. **Format**: Use markdown for easy conversion to various formats
