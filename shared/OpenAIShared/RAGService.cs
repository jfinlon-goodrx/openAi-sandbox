using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace OpenAIShared;

/// <summary>
/// Service for Retrieval-Augmented Generation (RAG) using embeddings
/// </summary>
public class RAGService
{
    private readonly OpenAIClient _openAIClient;
    private readonly ILogger<RAGService> _logger;

    public RAGService(
        OpenAIClient openAIClient,
        ILogger<RAGService> logger)
    {
        _openAIClient = openAIClient;
        _logger = logger;
    }

    /// <summary>
    /// Creates embeddings for a collection of documents
    /// </summary>
    public async Task<List<DocumentEmbedding>> CreateDocumentEmbeddingsAsync(
        List<Document> documents,
        CancellationToken cancellationToken = default)
    {
        var texts = documents.Select(d => d.Content).ToList();
        
        var request = new EmbeddingRequest
        {
            Model = "text-embedding-ada-002",
            Input = texts
        };

        var response = await _openAIClient.GetEmbeddingsAsync(request, cancellationToken);
        var embeddings = new List<DocumentEmbedding>();

        for (int i = 0; i < documents.Count && i < response.Data.Count; i++)
        {
            embeddings.Add(new DocumentEmbedding
            {
                Document = documents[i],
                Embedding = response.Data[i].Embedding
            });
        }

        return embeddings;
    }

    /// <summary>
    /// Finds similar documents using cosine similarity
    /// </summary>
    public async Task<List<SimilarDocument>> FindSimilarDocumentsAsync(
        string query,
        List<DocumentEmbedding> documentEmbeddings,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        // Create embedding for query
        var queryRequest = new EmbeddingRequest
        {
            Model = "text-embedding-ada-002",
            Input = new List<string> { query }
        };

        var queryResponse = await _openAIClient.GetEmbeddingsAsync(queryRequest, cancellationToken);
        var queryEmbedding = queryResponse.Data.First().Embedding;

        // Calculate cosine similarity
        var similarities = documentEmbeddings.Select(de =>
        {
            var similarity = CosineSimilarity(queryEmbedding, de.Embedding);
            return new SimilarDocument
            {
                Document = de.Document,
                Similarity = similarity
            };
        })
        .OrderByDescending(s => s.Similarity)
        .Take(topK)
        .ToList();

        return similarities;
    }

    /// <summary>
    /// Performs RAG: retrieves relevant documents and generates answer
    /// </summary>
    public async Task<string> QueryWithRAGAsync(
        string question,
        List<DocumentEmbedding> documentEmbeddings,
        int topK = 3,
        CancellationToken cancellationToken = default)
    {
        // Find similar documents
        var similarDocs = await FindSimilarDocumentsAsync(question, documentEmbeddings, topK, cancellationToken);

        // Build context from similar documents
        var context = string.Join("\n\n", similarDocs.Select(sd => 
            $"[{sd.Document.Title}]\n{sd.Document.Content}"));

        // Generate answer using context
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are a helpful assistant that answers questions based on provided context.")
            .WithContext(context)
            .WithInstruction($"Answer the following question based on the context provided. If the answer is not in the context, say so.\n\nQuestion: {question}")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = "gpt-4-turbo-preview",
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are a helpful assistant." },
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 500
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        return response.Choices.FirstOrDefault()?.Message?.Content ?? "Unable to generate answer.";
    }

    private static double CosineSimilarity(List<double> vectorA, List<double> vectorB)
    {
        if (vectorA.Count != vectorB.Count)
            throw new ArgumentException("Vectors must have the same length");

        double dotProduct = 0.0;
        double magnitudeA = 0.0;
        double magnitudeB = 0.0;

        for (int i = 0; i < vectorA.Count; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            magnitudeA += vectorA[i] * vectorA[i];
            magnitudeB += vectorB[i] * vectorB[i];
        }

        magnitudeA = Math.Sqrt(magnitudeA);
        magnitudeB = Math.Sqrt(magnitudeB);

        if (magnitudeA == 0.0 || magnitudeB == 0.0)
            return 0.0;

        return dotProduct / (magnitudeA * magnitudeB);
    }
}

public class Document
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class DocumentEmbedding
{
    public Document Document { get; set; } = new();
    public List<double> Embedding { get; set; } = new();
}

public class SimilarDocument
{
    public Document Document { get; set; } = new();
    public double Similarity { get; set; }
}
