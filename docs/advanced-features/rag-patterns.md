# RAG (Retrieval-Augmented Generation) Patterns

## Overview

RAG combines embeddings for semantic search with GPT for generation, enabling Q&A over large document collections.

## Architecture

```
Documents → Embeddings → Vector Store
                            ↓
Query → Embedding → Similarity Search → Relevant Docs → GPT → Answer
```

## Usage

### Create Document Embeddings

```csharp
var ragService = new RAGService(openAIClient, logger);

var documents = new List<Document>
{
    new() { Title = "Doc 1", Content = "..." },
    new() { Title = "Doc 2", Content = "..." }
};

var embeddings = await ragService.CreateDocumentEmbeddingsAsync(documents);
```

### Query with RAG

```csharp
var answer = await ragService.QueryWithRAGAsync(
    question: "What are the security requirements?",
    documentEmbeddings: embeddings,
    topK: 3 // Number of similar documents to use
);
```

### Find Similar Documents

```csharp
var similar = await ragService.FindSimilarDocumentsAsync(
    query: "user authentication",
    documentEmbeddings: embeddings,
    topK: 5
);
```

## Use Cases

- Requirements document Q&A
- Drug information search (Pharmacy)
- Manuscript search (Publishing)
- Knowledge base queries

## Best Practices

1. Chunk large documents appropriately
2. Store embeddings for reuse
3. Use appropriate topK values (3-5 typically)
4. Combine with metadata filtering when possible
