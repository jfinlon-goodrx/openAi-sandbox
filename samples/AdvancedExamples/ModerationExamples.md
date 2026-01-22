# Moderation API Examples

Examples demonstrating content moderation for safety and compliance.

## Content Moderation

```csharp
var moderationService = new ModerationService(httpClient, config, logger);

// Check user-generated content
var result = await moderationService.ModerateContentAsync(
    "This is a test message"
);

if (result.Results.First().Flagged)
{
    Console.WriteLine("Content flagged for moderation");
    var categories = result.Results.First().Categories;
    // Check specific categories
}
```

## Pharmacy: Patient Communication Filtering

```csharp
// Filter patient messages before processing
var moderation = await moderationService.ModerateContentAsync(patientMessage);

if (moderation.Results.First().Flagged)
{
    // Flag for manual review
    return new { requiresReview = true };
}
```

## Publishing: Manuscript Content Check

```csharp
// Check manuscript for inappropriate content
var moderation = await moderationService.ModerateContentAsync(manuscriptContent);

if (moderation.Results.First().Flagged)
{
    var categories = moderation.Results.First().Categories;
    // Log and flag for review
}
```

## Advertising: Ad Copy Safety Check

```csharp
// Verify ad copy meets safety standards
var moderation = await moderationService.ModerateContentAsync(adCopy);

if (!moderation.Results.First().Flagged)
{
    // Safe to publish
}
```
