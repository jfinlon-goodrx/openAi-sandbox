# Advanced OpenAI Features

This directory contains documentation for advanced OpenAI Platform features demonstrated in this portfolio.

## Available Features

### [Vision API](./vision-api.md)
GPT-4 Vision for image analysis and understanding. Used for:
- Cover image analysis (Publishing)
- Prescription label verification (Pharmacy)
- Ad creative analysis (Advertising)

### [RAG Patterns](./rag-patterns.md)
Retrieval-Augmented Generation using embeddings for semantic search and Q&A over document collections.

### [Moderation API](./moderation-api.md)
Content safety and compliance checking for user-generated content.

### [Batch Processing](./batch-processing.md)
Asynchronous batch processing with 50% cost reduction for high-volume tasks.

### [JSON Mode](./json-mode.md)
Structured output generation ensuring valid JSON responses.

## Implementation Status

All features are implemented in the shared `OpenAIShared` library and integrated into various projects:

- **VisionService**: Available in `shared/OpenAIShared/VisionService.cs`
- **ModerationService**: Available in `shared/OpenAIShared/ModerationService.cs`
- **RAGService**: Available in `shared/OpenAIShared/RAGService.cs`
- **BatchService**: Available in `shared/OpenAIShared/BatchService.cs`
- **JSON Mode**: Supported via `ChatCompletionRequest.ResponseFormat`

## Examples

See `samples/AdvancedExamples/` for code examples:
- `VisionAPI.md` - Vision API usage examples
- `RAGExamples.md` - RAG pattern examples
- `ModerationExamples.md` - Moderation API examples
- `BatchProcessingExamples.md` - Batch processing examples
- `JSONModeExamples.md` - JSON mode examples

## Complete Workflows

See `samples/CompleteWorkflows/` for end-to-end workflow examples:
- `PharmacyWorkflow.cs` - Complete prescription processing workflow
- `AdvertisingWorkflow.cs` - Complete campaign development workflow

## Getting Started

All services are registered via `ServiceCollectionExtensions.AddOpenAIServices()`:

```csharp
builder.Services.AddOpenAIServices(configuration);
```

This registers:
- `OpenAIClient` (Chat Completions, Embeddings, Whisper, DALL-E)
- `VisionService` (GPT-4 Vision)
- `ModerationService` (Content Moderation)
- `BatchService` (Batch Processing)
- `RAGService` (RAG patterns)

## Next Steps

1. Review the feature-specific documentation
2. Check out the examples in `samples/AdvancedExamples/`
3. Explore complete workflows in `samples/CompleteWorkflows/`
4. Integrate into your projects as needed
