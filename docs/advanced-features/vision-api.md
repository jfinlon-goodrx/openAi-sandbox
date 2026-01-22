# Vision API (GPT-4 Vision)

## Overview

GPT-4 Vision allows you to analyze images and answer questions about them. This is useful for:
- Cover image analysis (Publishing)
- Prescription label verification (Pharmacy)
- Ad creative analysis (Advertising)
- Document image analysis

## Setup

The VisionService is available in the OpenAIShared library:

```csharp
var visionService = new VisionService(httpClient, config, logger);
```

## Usage Examples

### Analyze Cover Image

```csharp
var analysis = await visionService.AnalyzeImageAsync(
    imageUrl: "https://example.com/cover.jpg",
    prompt: "Analyze this book cover. Evaluate visual appeal, typography, and marketability.",
    detail: "high" // "low", "high", or "auto"
);
```

### Analyze from Base64

```csharp
var base64Image = Convert.ToBase64String(imageBytes);
var analysis = await visionService.AnalyzeImageFromBase64Async(
    base64Image,
    prompt: "Describe this image"
);
```

## Use Cases

### Publishing: Cover Image Feedback

```csharp
var coverAnalyzer = new CoverImageAnalyzer(visionService, logger);
var analysis = await coverAnalyzer.AnalyzeCoverImageAsync(
    coverImageUrl,
    genre: "Science Fiction"
);
```

### Pharmacy: Label Verification

```csharp
var analysis = await visionService.AnalyzeImageAsync(
    labelImageUrl,
    prompt: "Extract and verify prescription label information: medication name, dosage, frequency, instructions"
);
```

## Best Practices

1. Use "high" detail for important analysis
2. Use "low" detail for simple descriptions (faster, cheaper)
3. Provide specific prompts for better results
4. Combine with text context when available
