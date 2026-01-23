# Advertising Agency Assistant

**For:** Advertising professionals who want to automate creative workflows and campaign development.

**What you'll learn:** How to use AI for ad copy generation, campaign strategy, audience analysis, brand voice development, creative briefs, A/B test hypotheses, and ad creative analysis with Vision API.

## Overview

The Advertising Agency Assistant helps advertising agencies automate ad copy generation, campaign strategy development, target audience analysis, brand voice development, creative brief creation, and A/B test hypothesis generation.

**Why use AI for advertising workflows?**
- **Creativity:** Generate multiple ad copy variations quickly
- **Strategy:** Develop comprehensive campaign strategies with AI insights
- **Brand consistency:** Maintain consistent brand voice across all content
- **Visual analysis:** Use Vision API to analyze ad creatives for effectiveness
- **A/B testing:** Generate test hypotheses and analyze results
- **Efficiency:** Speed up the creative development process

## Features

- **Ad Copy Generation**: Create compelling ad copy for various channels (print, digital, social media)
- **Campaign Strategy Development**: Develop comprehensive campaign strategies with channels, timelines, and metrics
- **Target Audience Analysis**: Analyze demographics, psychographics, pain points, and motivations
- **Brand Voice Development**: Create brand voice and tone guidelines
- **Creative Brief Generation**: Generate comprehensive creative briefs
- **A/B Test Hypotheses**: Generate testable hypotheses for optimization

## Architecture

```
┌─────────────────┐
│   Web API       │
└────────┬────────┘
         │
┌────────▼────────┐
│ Advertising     │
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
cd src/AdvertisingAgency/AdvertisingAgency.Api
dotnet run
```

3. Navigate to `https://localhost:7005/swagger` for API documentation

## API Endpoints

### POST /api/advertising/ad-copy

Generates ad copy for various channels.

**Request:**
```json
{
  "productName": "EcoClean Laundry Detergent",
  "productDescription": "Eco-friendly laundry detergent made from plant-based ingredients",
  "targetAudience": "Environmentally conscious consumers aged 25-45",
  "channel": "Social Media",
  "tone": "Friendly and inspiring"
}
```

**Response:**
```json
{
  "productName": "EcoClean Laundry Detergent",
  "channel": "Social Media",
  "headline": "Clean Clothes, Clean Planet",
  "subheadline": "Plant-based power for your laundry",
  "bodyCopy": "Make a difference with every load...",
  "callToAction": "Try EcoClean today",
  "tagline": "Nature's clean, naturally",
  "socialMediaPosts": {
    "twitter": "Clean clothes, clean planet 🌱 Try EcoClean...",
    "instagram": "✨ Clean that's kind to the planet..."
  },
  "keyMessages": ["Eco-friendly", "Effective cleaning", "Plant-based"]
}
```

### POST /api/advertising/campaign-strategy

Develops campaign strategy.

**Request:**
```json
{
  "brandName": "EcoClean",
  "productDescription": "Eco-friendly laundry detergent",
  "targetAudience": "Environmentally conscious consumers",
  "campaignObjective": "Increase brand awareness and trial",
  "budget": 50000
}
```

**Response:**
```json
{
  "brandName": "EcoClean",
  "campaignObjective": "Increase brand awareness and trial",
  "campaignTheme": "Sustainable cleaning for modern families",
  "keyMessages": ["Eco-friendly", "Effective", "Affordable"],
  "channels": [
    {
      "channel": "Social Media",
      "rationale": "Target audience is highly active on social platforms",
      "budgetAllocation": "40%"
    }
  ],
  "successMetrics": ["Brand awareness", "Trial rate", "Social engagement"]
}
```

### POST /api/advertising/analyze-audience

Analyzes target audience.

**Request:**
```json
{
  "productDescription": "Premium fitness tracking smartwatch",
  "demographic": "Ages 25-45, income $50k+",
  "psychographic": "Health-conscious, tech-savvy, active lifestyle"
}
```

**Response:**
```json
{
  "primaryAudience": {
    "demographics": "Ages 25-45, income $50k+, urban/suburban",
    "psychographics": "Health-conscious, tech-savvy, values convenience",
    "painPoints": ["Lack of motivation", "Difficulty tracking progress"],
    "motivations": ["Health improvement", "Achievement", "Social sharing"]
  },
  "messagingRecommendations": ["Focus on results", "Emphasize convenience"],
  "channelRecommendations": ["Social media", "Fitness apps", "Influencer partnerships"]
}
```

### POST /api/advertising/brand-voice

Develops brand voice.

**Request:**
```json
{
  "brandName": "TechFlow",
  "brandDescription": "Streamlined project management software for teams",
  "existingContent": "Some existing marketing materials..."
}
```

**Response:**
```json
{
  "brandName": "TechFlow",
  "voiceDescription": "Professional yet approachable, clear and confident",
  "toneGuidelines": [
    {
      "tone": "Professional",
      "whenToUse": "B2B communications, product documentation",
      "examples": ["Streamline your workflow", "Increase team productivity"]
    }
  ],
  "doAndDonts": {
    "do": ["Use active voice", "Be specific"],
    "dont": ["Use jargon", "Be vague"]
  }
}
```

### POST /api/advertising/creative-brief

Generates creative brief.

**Request:**
```json
{
  "clientName": "TechFlow",
  "productDescription": "Project management software",
  "campaignObjective": "Increase trial sign-ups",
  "targetAudience": "Small to medium business teams",
  "budget": "$25,000"
}
```

**Response:**
```json
{
  "clientName": "TechFlow",
  "background": "TechFlow is launching a new feature...",
  "objective": "Increase trial sign-ups by 30%",
  "targetAudience": "Small to medium business teams",
  "keyMessage": "Simplify your workflow",
  "toneOfVoice": "Professional and approachable",
  "deliverables": ["Social media ads", "Email campaign", "Landing page"],
  "successCriteria": ["30% increase in trials", "5% conversion rate"]
}
```

### POST /api/advertising/ab-test-hypotheses

Generates A/B test hypotheses.

**Request:**
```json
{
  "campaignDescription": "Email campaign for product launch",
  "metric": "Click-through rate"
}
```

**Response:**
```json
[
  {
    "hypothesis": "Personalized subject lines increase CTR",
    "variantA": "Generic subject line",
    "variantB": "Personalized with recipient name",
    "expectedOutcome": "Variant B increases CTR by 15%",
    "rationale": "Personalization increases engagement"
  }
]
```

## Usage Examples

### Generate Ad Copy

```csharp
var advertisingService = new AdvertisingService(openAIClient, logger);

var adCopy = await advertisingService.GenerateAdCopyAsync(
    productName: "EcoClean Laundry Detergent",
    productDescription: "Eco-friendly laundry detergent",
    targetAudience: "Environmentally conscious consumers",
    channel: "Social Media",
    tone: "Friendly and inspiring"
);

Console.WriteLine($"Headline: {adCopy.Headline}");
Console.WriteLine($"Body: {adCopy.BodyCopy}");
Console.WriteLine($"CTA: {adCopy.CallToAction}");
```

### Develop Campaign Strategy

```csharp
var strategy = await advertisingService.DevelopCampaignStrategyAsync(
    brandName: "EcoClean",
    productDescription: "Eco-friendly laundry detergent",
    targetAudience: "Environmentally conscious consumers",
    campaignObjective: "Increase brand awareness",
    budget: 50000m
);

Console.WriteLine($"Theme: {strategy.CampaignTheme}");
Console.WriteLine("\nChannels:");
foreach (var channel in strategy.Channels)
{
    Console.WriteLine($"- {channel.Channel}: {channel.Rationale}");
}
```

### Analyze Target Audience

```csharp
var analysis = await advertisingService.AnalyzeTargetAudienceAsync(
    productDescription: "Premium fitness tracking smartwatch",
    demographic: "Ages 25-45, income $50k+",
    psychographic: "Health-conscious, tech-savvy"
);

Console.WriteLine($"Demographics: {analysis.PrimaryAudience.Demographics}");
Console.WriteLine("\nPain Points:");
foreach (var painPoint in analysis.PrimaryAudience.PainPoints)
{
    Console.WriteLine($"- {painPoint}");
}
```

## Best Practices

1. **Iterate**: Use AI output as a starting point, then refine based on brand guidelines
2. **Test**: Always A/B test AI-generated copy
3. **Brand Consistency**: Ensure all outputs align with brand voice guidelines
4. **Audience Focus**: Keep target audience top of mind
5. **Compliance**: Ensure all copy complies with advertising regulations

## Related Documentation

- [Getting Started](../getting-started/)
- [Best Practices](../best-practices/)
