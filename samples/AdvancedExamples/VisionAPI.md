# Vision API Examples

Examples demonstrating GPT-4 Vision capabilities for image analysis.

## Cover Image Analysis

Analyze book cover images for publishing:

```csharp
var visionService = new VisionService(httpClient, config, logger);

// Analyze a cover image
var analysis = await visionService.AnalyzeImageAsync(
    imageUrl: "https://example.com/book-cover.jpg",
    prompt: "Analyze this book cover. Evaluate visual appeal, typography, color scheme, and marketability."
);

Console.WriteLine(analysis);
```

## Pharmacy: Medication Label Analysis

Analyze prescription labels for accuracy:

```csharp
var analysis = await visionService.AnalyzeImageAsync(
    imageUrl: labelImageUrl,
    prompt: "Analyze this prescription label. Extract: medication name, dosage, frequency, instructions, and warnings. Verify all text is readable."
);
```

## Advertising: Ad Creative Analysis

Analyze ad creatives for effectiveness:

```csharp
var analysis = await visionService.AnalyzeImageAsync(
    imageUrl: adImageUrl,
    prompt: "Analyze this advertisement. Evaluate: visual hierarchy, call-to-action visibility, brand presence, and overall effectiveness."
);
```

## Base64 Image Analysis

Analyze images from base64 data:

```csharp
var base64Image = Convert.ToBase64String(imageBytes);
var analysis = await visionService.AnalyzeImageFromBase64Async(
    base64Image,
    prompt: "Describe this image in detail."
);
```
