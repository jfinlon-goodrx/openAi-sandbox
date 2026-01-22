# Moderation API

## Overview

The Moderation API checks content against OpenAI's usage policies to ensure safety and compliance.

## Usage

```csharp
var moderationService = new ModerationService(httpClient, config, logger);

var result = await moderationService.ModerateContentAsync("User-generated content");

if (result.Results.First().Flagged)
{
    // Content violates policies
    var categories = result.Results.First().Categories;
    // Check specific categories: Hate, Harassment, SelfHarm, Sexual, Violence
}
```

## Use Cases

- Filter user-generated content
- Pre-screen patient communications (Pharmacy)
- Verify ad copy safety (Advertising)
- Check manuscript content (Publishing)

## Categories

- Hate / HateThreatening
- Harassment / HarassmentThreatening
- SelfHarm / SelfHarmIntent / SelfHarmInstructions
- Sexual / SexualMinors
- Violence / ViolenceGraphic

## Best Practices

1. Moderate user-generated content before processing
2. Log flagged content for review
3. Use category scores for nuanced decisions
4. Combine with business logic for final decisions
