# Advertising Agency Examples

Examples for using the Advertising Agency Assistant API in advertising workflows.

## Examples

### 1. Generate Ad Copy

```csharp
var advertisingService = new AdvertisingService(openAIClient, logger);

var adCopy = await advertisingService.GenerateAdCopyAsync(
    productName: "EcoClean Laundry Detergent",
    productDescription: "Eco-friendly laundry detergent made from plant-based ingredients",
    targetAudience: "Environmentally conscious consumers aged 25-45",
    channel: "Social Media",
    tone: "Friendly and inspiring"
);

Console.WriteLine($"Headline: {adCopy.Headline}");
Console.WriteLine($"Subheadline: {adCopy.Subheadline}");
Console.WriteLine($"\nBody Copy:\n{adCopy.BodyCopy}");
Console.WriteLine($"\nCall to Action: {adCopy.CallToAction}");
Console.WriteLine($"Tagline: {adCopy.Tagline}");

if (adCopy.SocialMediaPosts != null)
{
    Console.WriteLine($"\nTwitter: {adCopy.SocialMediaPosts.Twitter}");
    Console.WriteLine($"Instagram: {adCopy.SocialMediaPosts.Instagram}");
}
```

### 2. Develop Campaign Strategy

```csharp
var strategy = await advertisingService.DevelopCampaignStrategyAsync(
    brandName: "EcoClean",
    productDescription: "Eco-friendly laundry detergent",
    targetAudience: "Environmentally conscious consumers",
    campaignObjective: "Increase brand awareness and trial",
    budget: 50000m
);

Console.WriteLine($"Campaign Theme: {strategy.CampaignTheme}");
Console.WriteLine("\nKey Messages:");
foreach (var message in strategy.KeyMessages)
{
    Console.WriteLine($"- {message}");
}

Console.WriteLine("\nChannels:");
foreach (var channel in strategy.Channels)
{
    Console.WriteLine($"- {channel.Channel}: {channel.Rationale}");
    if (!string.IsNullOrEmpty(channel.BudgetAllocation))
    {
        Console.WriteLine($"  Budget: {channel.BudgetAllocation}");
    }
}
```

### 3. Analyze Target Audience

```csharp
var analysis = await advertisingService.AnalyzeTargetAudienceAsync(
    productDescription: "Premium fitness tracking smartwatch",
    demographic: "Ages 25-45, income $50k+",
    psychographic: "Health-conscious, tech-savvy, active lifestyle"
);

Console.WriteLine($"Demographics: {analysis.PrimaryAudience.Demographics}");
Console.WriteLine($"Psychographics: {analysis.PrimaryAudience.Psychographics}");

Console.WriteLine("\nPain Points:");
foreach (var painPoint in analysis.PrimaryAudience.PainPoints)
{
    Console.WriteLine($"- {painPoint}");
}

Console.WriteLine("\nMotivations:");
foreach (var motivation in analysis.PrimaryAudience.Motivations)
{
    Console.WriteLine($"- {motivation}");
}

Console.WriteLine("\nMessaging Recommendations:");
foreach (var recommendation in analysis.MessagingRecommendations)
{
    Console.WriteLine($"- {recommendation}");
}
```

### 4. Develop Brand Voice

```csharp
var brandVoice = await advertisingService.DevelopBrandVoiceAsync(
    brandName: "TechFlow",
    brandDescription: "Streamlined project management software for teams",
    existingContent: "Some existing marketing materials..."
);

Console.WriteLine($"Brand Voice: {brandVoice.VoiceDescription}");

Console.WriteLine("\nTone Guidelines:");
foreach (var guideline in brandVoice.ToneGuidelines)
{
    Console.WriteLine($"\n{guideline.Tone}:");
    Console.WriteLine($"  When to use: {guideline.WhenToUse}");
    Console.WriteLine("  Examples:");
    foreach (var example in guideline.Examples)
    {
        Console.WriteLine($"    - {example}");
    }
}

if (brandVoice.DoAndDonts != null)
{
    Console.WriteLine("\nDo's:");
    foreach (var doItem in brandVoice.DoAndDonts.Do)
    {
        Console.WriteLine($"- {doItem}");
    }

    Console.WriteLine("\nDon'ts:");
    foreach (var dontItem in brandVoice.DoAndDonts.Dont)
    {
        Console.WriteLine($"- {dontItem}");
    }
}
```

### 5. Generate Creative Brief

```csharp
var brief = await advertisingService.GenerateCreativeBriefAsync(
    clientName: "TechFlow",
    productDescription: "Project management software",
    campaignObjective: "Increase trial sign-ups by 30%",
    targetAudience: "Small to medium business teams",
    budget: "$25,000"
);

Console.WriteLine($"Client: {brief.ClientName}");
Console.WriteLine($"\nBackground:\n{brief.Background}");
Console.WriteLine($"\nObjective: {brief.Objective}");
Console.WriteLine($"Target Audience: {brief.TargetAudience}");
Console.WriteLine($"Key Message: {brief.KeyMessage}");

Console.WriteLine("\nDeliverables:");
foreach (var deliverable in brief.Deliverables)
{
    Console.WriteLine($"- {deliverable}");
}

Console.WriteLine("\nSuccess Criteria:");
foreach (var criteria in brief.SuccessCriteria)
{
    Console.WriteLine($"- {criteria}");
}
```

### 6. Generate A/B Test Hypotheses

```csharp
var hypotheses = await advertisingService.GenerateABTestHypothesesAsync(
    campaignDescription: "Email campaign for product launch",
    metric: "Click-through rate"
);

Console.WriteLine("A/B Test Hypotheses:");
foreach (var hypothesis in hypotheses)
{
    Console.WriteLine($"\nHypothesis: {hypothesis.Hypothesis}");
    Console.WriteLine($"Variant A: {hypothesis.VariantA}");
    Console.WriteLine($"Variant B: {hypothesis.VariantB}");
    Console.WriteLine($"Expected Outcome: {hypothesis.ExpectedOutcome}");
    if (!string.IsNullOrEmpty(hypothesis.Rationale))
    {
        Console.WriteLine($"Rationale: {hypothesis.Rationale}");
    }
}
```

## Complete Campaign Workflow

```csharp
// 1. Analyze target audience
var audience = await advertisingService.AnalyzeTargetAudienceAsync(
    productDescription: "Eco-friendly laundry detergent"
);

// 2. Develop brand voice
var brandVoice = await advertisingService.DevelopBrandVoiceAsync(
    brandName: "EcoClean",
    brandDescription: "Eco-friendly laundry detergent"
);

// 3. Develop campaign strategy
var strategy = await advertisingService.DevelopCampaignStrategyAsync(
    brandName: "EcoClean",
    productDescription: "Eco-friendly laundry detergent",
    targetAudience: audience.PrimaryAudience.Demographics,
    campaignObjective: "Increase brand awareness",
    budget: 50000m
);

// 4. Generate ad copy for each channel
foreach (var channel in strategy.Channels)
{
    var adCopy = await advertisingService.GenerateAdCopyAsync(
        productName: "EcoClean",
        productDescription: "Eco-friendly laundry detergent",
        targetAudience: audience.PrimaryAudience.Demographics,
        channel: channel.Channel,
        tone: brandVoice.ToneGuidelines.First().Tone
    );
    
    // Use ad copy for campaign
}

// 5. Generate A/B test hypotheses
var hypotheses = await advertisingService.GenerateABTestHypothesesAsync(
    campaignDescription: "Social media campaign",
    metric: "Conversion rate"
);
```

## API Usage

Start the API:
```bash
cd src/AdvertisingAgency/AdvertisingAgency.Api
dotnet run
```

**Generate Ad Copy:**
```bash
curl -X POST https://localhost:7005/api/advertising/ad-copy \
  -H "Content-Type: application/json" \
  -d '{
    "productName": "EcoClean",
    "productDescription": "Eco-friendly laundry detergent",
    "targetAudience": "Environmentally conscious consumers",
    "channel": "Social Media",
    "tone": "Friendly"
  }'
```

**Develop Campaign Strategy:**
```bash
curl -X POST https://localhost:7005/api/advertising/campaign-strategy \
  -H "Content-Type: application/json" \
  -d '{
    "brandName": "EcoClean",
    "productDescription": "Eco-friendly laundry detergent",
    "targetAudience": "Environmentally conscious consumers",
    "campaignObjective": "Increase brand awareness",
    "budget": 50000
  }'
```

## Best Practices

1. **Iterate**: Use AI output as starting point, refine based on brand guidelines
2. **Test**: Always A/B test AI-generated copy
3. **Brand Consistency**: Ensure all outputs align with brand voice
4. **Audience Focus**: Keep target audience in mind
5. **Compliance**: Ensure compliance with advertising regulations
