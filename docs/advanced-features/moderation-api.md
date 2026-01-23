# Moderation API

**For:** Developers building applications that accept user-generated content.

**What you'll learn:** How to use OpenAI's Moderation API to check content for safety, filter inappropriate content, and ensure compliance with content policies.

## Overview

The Moderation API helps you identify potentially harmful or inappropriate content before it's displayed to users or stored in your system.

**Why use the Moderation API?**
- **Safety:** Protect users from harmful or inappropriate content
- **Compliance:** Ensure content meets platform policies and regulations
- **Automation:** Automatically filter content without manual review
- **Trust:** Build user trust by maintaining safe content standards
- **Legal protection:** Reduce liability from inappropriate content

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
