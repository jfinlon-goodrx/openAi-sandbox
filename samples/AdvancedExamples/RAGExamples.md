# RAG (Retrieval-Augmented Generation) Examples

Examples demonstrating embeddings and RAG patterns for semantic search and Q&A.

## Requirements Document Q&A with RAG

```csharp
var ragService = new RAGService(openAIClient, logger);

// Create document embeddings
var documents = new List<Document>
{
    new() { Title = "Requirements Doc 1", Content = "..." },
    new() { Title = "Requirements Doc 2", Content = "..." },
    new() { Title = "Requirements Doc 3", Content = "..." }
};

var embeddings = await ragService.CreateDocumentEmbeddingsAsync(documents);

// Query with RAG
var answer = await ragService.QueryWithRAGAsync(
    question: "What are the security requirements?",
    documentEmbeddings: embeddings,
    topK: 3
);

Console.WriteLine(answer);
```

## Find Similar Requirements

```csharp
var similarDocs = await ragService.FindSimilarDocumentsAsync(
    query: "user authentication requirements",
    documentEmbeddings: embeddings,
    topK: 5
);

foreach (var doc in similarDocs)
{
    Console.WriteLine($"{doc.Document.Title} - Similarity: {doc.Similarity:F2}");
}
```

## Pharmacy: Drug Information Search

```csharp
// Create embeddings for drug information documents
var drugDocs = new List<Document>
{
    new() { Title = "Metformin Info", Content = "Metformin is used for..." },
    new() { Title = "Lisinopril Info", Content = "Lisinopril is used for..." }
};

var embeddings = await ragService.CreateDocumentEmbeddingsAsync(drugDocs);

// Search for drug interactions
var answer = await ragService.QueryWithRAGAsync(
    question: "Can Metformin be taken with Lisinopril?",
    documentEmbeddings: embeddings
);
```

## Publishing: Manuscript Search

```csharp
// Create embeddings for manuscript chapters
var chapters = LoadManuscriptChapters();
var embeddings = await ragService.CreateDocumentEmbeddingsAsync(chapters);

// Find relevant chapters for a topic
var similarChapters = await ragService.FindSimilarDocumentsAsync(
    query: "character development arc",
    documentEmbeddings: embeddings
);
```
