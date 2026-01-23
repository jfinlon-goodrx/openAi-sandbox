# RAG (Retrieval-Augmented Generation) Patterns

**For:** Developers and technical professionals who want to build Q&A systems over large document collections.

**What you'll learn:** How to use RAG to enable Q&A over large documents, create embeddings, perform semantic search, and reduce token usage by 80-88% compared to sending entire documents.

## Overview

RAG (Retrieval-Augmented Generation) combines embeddings for semantic search with GPT for generation, enabling Q&A over large document collections.

**Why use RAG?**
- **Cost efficiency:** Only send relevant document sections to AI, reducing token usage by 80-88%
- **Accuracy:** Find the most relevant information before generating answers
- **Scalability:** Handle document collections too large to send in a single request
- **Context:** Provide AI with the most relevant context for better answers
- **Real-world applications:** Perfect for document Q&A, knowledge bases, and search systems

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
