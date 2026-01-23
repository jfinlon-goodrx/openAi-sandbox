# Publishing Assistant Examples

Examples demonstrating OpenAI Platform capabilities for publishing workflows.

## Features

- **Book Review Generation** - Comprehensive reviews with ratings and recommendations
- **Summary/Blurb Generation** - Marketing-ready summaries
- **Marketing Blurb Creation** - Social media and promotional copy
- **Cover Image Generation** - DALL-E integration for cover art
- **Cover Image Analysis** - Vision API for evaluating existing covers
- **File Format Conversion** - Markdown to HTML, Plain Text, EPUB, PDF
- **Editorial Notes** - Detailed editing suggestions
- **PDF Manuscript Review** ⭐ NEW - Senior publishing agent review with intelligent chunking

## PDF Manuscript Review (NEW)

### Overview

The PDF review feature allows uploading a complete manuscript PDF for comprehensive review by a "senior publishing agent". The system uses intelligent chunking and RAG to efficiently analyze large documents while minimizing token usage.

### Key Features

1. **Intelligent Chunking**
   - Preserves chapter and paragraph boundaries
   - Overlaps chunks to maintain context
   - ~2000 characters per chunk (~1500 tokens)

2. **Local Storage**
   - PDFs stored locally for reference
   - Avoids re-extraction on subsequent queries
   - Metadata tracking for stored documents

3. **RAG-Based Analysis**
   - Creates embeddings for each chunk
   - Uses semantic search to find relevant sections
   - Only sends relevant chunks to GPT (not entire document)

4. **Comprehensive Review**
   - Overall summary
   - Plot analysis
   - Character development analysis
   - Writing style assessment
   - Structure evaluation
   - Specific issues and suggestions (with locations)
   - Prioritized recommendations

### Token Optimization

**Without Optimization:**
- 100K word novel: ~125,000 tokens per analysis
- Multiple analyses: 500,000+ tokens

**With Chunking + RAG:**
- Initial chunking + embeddings: ~15,000 tokens (one-time)
- Each analysis aspect: ~2,000-5,000 tokens (only relevant chunks)
- Total for full review: ~15,000-25,000 tokens
- **Savings: 80-88% reduction in token usage**

### Usage Examples

#### Upload PDF for Review

```bash
curl -X POST "http://localhost:5001/api/publishing/review-pdf?genre=Science Fiction" \
  -H "X-API-Key: your-api-key" \
  -F "pdfFile=@manuscript.pdf"
```

#### Python Example

```python
import requests

with open("manuscript.pdf", "rb") as pdf_file:
    files = {"pdfFile": ("manuscript.pdf", pdf_file, "application/pdf")}
    params = {"genre": "Science Fiction"}
    
    response = requests.post(
        "http://localhost:5001/api/publishing/review-pdf",
        files=files,
        params=params,
        headers={"X-API-Key": "your-api-key"}
    )
    
    review = response.json()
    print(f"Document ID: {review['documentId']}")
    print(f"Chunks: {review['chunkCount']}")
    print(f"Estimated Tokens: {review['estimatedTokensUsed']}")
    
    # Access review sections
    print(f"\nOverall Summary:\n{review['overallSummary']}")
    print(f"\nPlot Analysis:\n{review['plotAnalysis']}")
    print(f"\nCharacter Analysis:\n{review['characterAnalysis']}")
    
    # Issues and suggestions
    for issue in review['issuesAndSuggestions']:
        print(f"\n[{issue['severity'].upper()}] {issue['category']}")
        print(f"  Issue: {issue['description']}")
        print(f"  Suggestion: {issue['suggestion']}")
        if issue.get('location'):
            print(f"  Location: {issue['location']}")
    
    print(f"\nRecommendations:\n{review['recommendations']}")
```

#### Response Structure

```json
{
  "documentId": "abc-123-def",
  "overallSummary": "Comprehensive summary...",
  "plotAnalysis": "Detailed plot analysis...",
  "characterAnalysis": "Character development assessment...",
  "writingStyleAnalysis": "Writing style evaluation...",
  "structureAnalysis": "Structure and organization review...",
  "issuesAndSuggestions": [
    {
      "category": "plot",
      "severity": "major",
      "description": "Pacing slows in middle chapters",
      "location": "Chapters 8-12",
      "suggestion": "Consider tightening narrative or adding subplot"
    }
  ],
  "recommendations": "Prioritized recommendations...",
  "chunkCount": 45,
  "estimatedTokensUsed": 18500,
  "reviewedAt": "2024-01-15T10:30:00Z"
}
```

### Managing Stored PDFs

#### Get PDF Metadata

```bash
curl -X GET "http://localhost:5001/api/publishing/pdf/{documentId}/metadata" \
  -H "X-API-Key: your-api-key"
```

#### Delete Stored PDF

```bash
curl -X DELETE "http://localhost:5001/api/publishing/pdf/{documentId}" \
  -H "X-API-Key: your-api-key"
```

## Other Publishing Examples

### Generate Book Review

```csharp
var review = await publishingService.GenerateReviewAsync(
    manuscriptContent: "Chapter 1: The story begins...",
    genre: "Science Fiction");
```

### Generate Summary

```csharp
var summary = await publishingService.GenerateSummaryAsync(
    manuscriptContent: "Full manuscript...",
    maxLength: 250);
```

### Generate Marketing Blurb

```csharp
var blurb = await publishingService.GenerateMarketingBlurbAsync(
    manuscriptContent: "Full manuscript...",
    targetAudience: "Young adults aged 18-25");
```

### Generate Cover Image Description

```csharp
var description = await publishingService.GenerateCoverImageDescriptionAsync(
    manuscriptContent: "Full manuscript...",
    genre: "Mystery");
```

### Analyze Cover Image

```csharp
var analysis = await coverAnalyzer.AnalyzeCoverImageAsync(
    imageUrl: "https://example.com/cover.jpg",
    genre: "Mystery");
```

## Best Practices

1. **PDF Processing**
   - Use PdfPig library for text extraction
   - Store PDFs locally to avoid re-processing
   - Clean extracted text before chunking

2. **Chunking Strategy**
   - Preserve chapter boundaries when possible
   - Use paragraph boundaries as fallback
   - Include overlap for context preservation
   - Aim for ~1500 tokens per chunk

3. **RAG Optimization**
   - Create embeddings once, reuse for multiple queries
   - Use appropriate top-K values (3-5 for analysis, 10+ for issue detection)
   - Store embeddings in vector database for production

4. **Token Management**
   - Monitor token usage per review
   - Use chunking for documents >10K words
   - Cache embeddings to avoid re-computation
   - Consider batch processing for multiple documents

5. **Storage**
   - Configure storage path in appsettings.json
   - Implement cleanup policies for old PDFs
   - Consider cloud storage (Azure Blob, S3) for production

## Configuration

```json
{
  "Publishing": {
    "StoragePath": "/path/to/storage"  // Optional, defaults to temp directory
  }
}
```

## Dependencies

- **PdfPig**: PDF text extraction (`UglyToad.PdfPig`)
- **Markdig**: Markdown processing
- **OpenAI SDK**: GPT-4, Embeddings API, Vision API, DALL-E

## Production Considerations

1. **PDF Library**: Currently uses PdfPig. Consider:
   - iTextSharp for advanced features
   - PdfSharp for .NET Core compatibility
   - Cloud-based PDF processing services

2. **Vector Database**: For production, use:
   - Pinecone (managed)
   - Weaviate (self-hosted)
   - Azure Cognitive Search (with vector support)
   - PostgreSQL with pgvector

3. **Storage**: Consider:
   - Azure Blob Storage
   - AWS S3
   - Local storage with backup

4. **Scaling**: 
   - Process PDFs asynchronously
   - Use background jobs for large documents
   - Implement progress tracking
   - Add caching for repeated reviews
