using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenAIShared;

namespace PublishingAssistant.Core;

/// <summary>
/// Senior Publishing Agent service for comprehensive manuscript review
/// Uses chunking and RAG to efficiently analyze large documents
/// </summary>
public class SeniorPublishingAgentService
{
    private readonly OpenAIClient _openAIClient;
    private readonly RAGService _ragService;
    private readonly DocumentChunkingService _chunkingService;
    private readonly ILogger<SeniorPublishingAgentService> _logger;
    private readonly string _model;

    public SeniorPublishingAgentService(
        OpenAIClient openAIClient,
        RAGService ragService,
        DocumentChunkingService chunkingService,
        ILogger<SeniorPublishingAgentService> logger,
        string model = "gpt-4-turbo-preview")
    {
        _openAIClient = openAIClient;
        _ragService = ragService;
        _chunkingService = chunkingService;
        _logger = logger;
        _model = model;
    }

    /// <summary>
    /// Performs comprehensive review of a manuscript using chunking and RAG
    /// </summary>
    public async Task<SeniorAgentReview> ReviewManuscriptAsync(
        string manuscriptContent,
        string documentId,
        string? genre = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting senior agent review for document {DocumentId}", documentId);

        // Step 1: Chunk the document
        var chunks = _chunkingService.ChunkDocument(manuscriptContent, documentId);
        _logger.LogInformation("Document chunked into {ChunkCount} chunks", chunks.Count);

        // Step 2: Create embeddings for chunks (for RAG-based analysis)
        var documents = chunks.Select((chunk, index) => new Document
        {
            Id = chunk.Id,
            Title = $"Chapter {chunk.ChapterNumber}, Chunk {chunk.ChunkIndex}",
            Content = chunk.Content,
            Metadata = new Dictionary<string, string>
            {
                { "chapter", chunk.ChapterNumber.ToString() },
                { "chunkIndex", chunk.ChunkIndex.ToString() },
                { "documentId", documentId }
            }
        }).ToList();

        var embeddings = await _ragService.CreateDocumentEmbeddingsAsync(documents, cancellationToken);
        _logger.LogInformation("Created embeddings for {Count} chunks", embeddings.Count);

        // Step 3: Generate high-level summary first (using first few chunks)
        var summaryChunks = chunks.Take(Math.Min(5, chunks.Count)).ToList();
        var summaryContent = string.Join("\n\n", summaryChunks.Select(c => c.Content));
        var overallSummary = await GenerateOverallSummaryAsync(summaryContent, genre, cancellationToken);

        // Step 4: Analyze key aspects using RAG
        var plotAnalysis = await AnalyzeAspectAsync(
            "Plot structure, pacing, narrative arc, and story coherence",
            embeddings,
            cancellationToken);

        var characterAnalysis = await AnalyzeAspectAsync(
            "Character development, depth, consistency, and dialogue quality",
            embeddings,
            cancellationToken);

        var writingStyleAnalysis = await AnalyzeAspectAsync(
            "Writing style, prose quality, show vs tell, and voice consistency",
            embeddings,
            cancellationToken);

        var structureAnalysis = await AnalyzeAspectAsync(
            "Story structure, chapter organization, transitions, and flow",
            embeddings,
            cancellationToken);

        // Step 5: Identify specific issues and suggestions
        var issuesAndSuggestions = await IdentifyIssuesAndSuggestionsAsync(
            embeddings,
            cancellationToken);

        // Step 6: Generate comprehensive recommendations
        var recommendations = await GenerateRecommendationsAsync(
            overallSummary,
            plotAnalysis,
            characterAnalysis,
            writingStyleAnalysis,
            structureAnalysis,
            issuesAndSuggestions,
            cancellationToken);

        _logger.LogInformation("Completed senior agent review for document {DocumentId}", documentId);

        return new SeniorAgentReview
        {
            DocumentId = documentId,
            OverallSummary = overallSummary,
            PlotAnalysis = plotAnalysis,
            CharacterAnalysis = characterAnalysis,
            WritingStyleAnalysis = writingStyleAnalysis,
            StructureAnalysis = structureAnalysis,
            IssuesAndSuggestions = issuesAndSuggestions,
            Recommendations = recommendations,
            ChunkCount = chunks.Count,
            EstimatedTokensUsed = EstimateTotalTokens(chunks),
            ReviewedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Generates overall summary of the manuscript
    /// </summary>
    private async Task<string> GenerateOverallSummaryAsync(
        string content,
        string? genre,
        CancellationToken cancellationToken)
    {
        var genreContext = !string.IsNullOrEmpty(genre) ? $"Genre: {genre}. " : "";
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are a senior publishing agent with 30+ years of experience. " +
                             "You have reviewed thousands of manuscripts and know what makes a book successful.")
            .WithInstruction($"{genreContext}Provide a comprehensive high-level summary of this manuscript covering: " +
                           "main themes, target audience, market potential, and overall quality assessment.")
            .WithInput(content)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are a senior publishing agent." },
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 1000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        return response.Choices.FirstOrDefault()?.Message?.Content ?? "Unable to generate summary.";
    }

    /// <summary>
    /// Analyzes a specific aspect using RAG to find relevant chunks
    /// </summary>
    private async Task<string> AnalyzeAspectAsync(
        string aspectDescription,
        List<DocumentEmbedding> embeddings,
        CancellationToken cancellationToken)
    {
        var query = $"Analyze {aspectDescription} in this manuscript";
        var answer = await _ragService.QueryWithRAGAsync(query, embeddings, topK: 5, cancellationToken);
        return answer;
    }

    /// <summary>
    /// Identifies specific issues and provides suggestions
    /// </summary>
    private async Task<List<IssueAndSuggestion>> IdentifyIssuesAndSuggestionsAsync(
        List<DocumentEmbedding> embeddings,
        CancellationToken cancellationToken)
    {
        var functionDefinition = new FunctionDefinition
        {
            Name = "identify_issues_and_suggestions",
            Description = "Identifies specific issues and provides actionable suggestions",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    issues = new
                    {
                        type = "array",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                category = new { type = "string", description = "Issue category (plot, character, style, structure, etc.)" },
                                severity = new { type = "string", description = "Severity: critical, major, minor" },
                                description = new { type = "string", description = "Detailed description of the issue" },
                                location = new { type = "string", description = "Where in the manuscript (chapter, section)" },
                                suggestion = new { type = "string", description = "Specific suggestion for improvement" }
                            },
                            required = new[] { "category", "severity", "description", "suggestion" }
                        }
                    }
                },
                required = new[] { "issues" }
            }
        };

        // Use RAG to find relevant sections with issues
        var query = "Identify specific issues, problems, or areas for improvement in this manuscript. " +
                   "Include plot inconsistencies, character development problems, writing style issues, " +
                   "structural problems, pacing issues, and any other concerns.";
        
        var relevantChunks = await _ragService.FindSimilarDocumentsAsync(query, embeddings, topK: 10, cancellationToken);
        var context = string.Join("\n\n", relevantChunks.Select(sd => 
            $"[{sd.Document.Title}]\n{sd.Document.Content}"));

        var prompt = new PromptBuilder()
            .WithSystemMessage("You are a senior publishing agent providing detailed editorial feedback.")
            .WithContext(context)
            .WithInstruction("Analyze the manuscript sections and identify specific issues with actionable suggestions. " +
                           "Be specific about locations and provide concrete improvement recommendations.")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are a senior publishing agent." },
                new() { Role = "user", Content = prompt }
            },
            Functions = new List<FunctionDefinition> { functionDefinition },
            FunctionCall = "auto",
            Temperature = 0.3,
            MaxTokens = 3000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var message = response.Choices.FirstOrDefault()?.Message;

        if (message?.FunctionCall != null)
        {
            return ParseIssuesFromFunctionCall(message.FunctionCall.Arguments);
        }

        return new List<IssueAndSuggestion>();
    }

    /// <summary>
    /// Generates comprehensive recommendations
    /// </summary>
    private async Task<string> GenerateRecommendationsAsync(
        string overallSummary,
        string plotAnalysis,
        string characterAnalysis,
        string writingStyleAnalysis,
        string structureAnalysis,
        List<IssueAndSuggestion> issues,
        CancellationToken cancellationToken)
    {
        var issuesSummary = string.Join("\n", issues.Select(i => 
            $"- [{i.Severity}] {i.Category}: {i.Description}"));

        var prompt = new PromptBuilder()
            .WithSystemMessage("You are a senior publishing agent providing final recommendations.")
            .WithContext($"Overall Summary:\n{overallSummary}\n\n" +
                        $"Plot Analysis:\n{plotAnalysis}\n\n" +
                        $"Character Analysis:\n{characterAnalysis}\n\n" +
                        $"Writing Style Analysis:\n{writingStyleAnalysis}\n\n" +
                        $"Structure Analysis:\n{structureAnalysis}\n\n" +
                        $"Identified Issues:\n{issuesSummary}")
            .WithInstruction("Based on all the analysis, provide comprehensive, prioritized recommendations " +
                           "for improving this manuscript. Organize by priority (critical, high, medium, low) " +
                           "and provide actionable next steps.")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are a senior publishing agent." },
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 2000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        return response.Choices.FirstOrDefault()?.Message?.Content ?? "Unable to generate recommendations.";
    }

    /// <summary>
    /// Parses issues from function call response
    /// </summary>
    private List<IssueAndSuggestion> ParseIssuesFromFunctionCall(string argumentsJson)
    {
        try
        {
            var arguments = JsonSerializer.Deserialize<JsonElement>(argumentsJson);
            var issues = new List<IssueAndSuggestion>();

            if (arguments.TryGetProperty("issues", out var issuesArray))
            {
                foreach (var issueElement in issuesArray.EnumerateArray())
                {
                    issues.Add(new IssueAndSuggestion
                    {
                        Category = issueElement.GetProperty("category").GetString() ?? "General",
                        Severity = issueElement.GetProperty("severity").GetString() ?? "minor",
                        Description = issueElement.GetProperty("description").GetString() ?? string.Empty,
                        Location = issueElement.TryGetProperty("location", out var loc) ? loc.GetString() : null,
                        Suggestion = issueElement.GetProperty("suggestion").GetString() ?? string.Empty
                    });
                }
            }

            return issues;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing issues from function call");
            return new List<IssueAndSuggestion>();
        }
    }

    /// <summary>
    /// Estimates total tokens used (rough approximation)
    /// </summary>
    private int EstimateTotalTokens(List<DocumentChunk> chunks)
    {
        var totalChars = chunks.Sum(c => c.Content.Length);
        return (int)(totalChars / 4.0); // Rough estimate: 1 token ≈ 4 characters
    }
}

public class SeniorAgentReview
{
    public string DocumentId { get; set; } = string.Empty;
    public string OverallSummary { get; set; } = string.Empty;
    public string PlotAnalysis { get; set; } = string.Empty;
    public string CharacterAnalysis { get; set; } = string.Empty;
    public string WritingStyleAnalysis { get; set; } = string.Empty;
    public string StructureAnalysis { get; set; } = string.Empty;
    public List<IssueAndSuggestion> IssuesAndSuggestions { get; set; } = new();
    public string Recommendations { get; set; } = string.Empty;
    public int ChunkCount { get; set; }
    public int EstimatedTokensUsed { get; set; }
    public DateTime ReviewedAt { get; set; }
}

public class IssueAndSuggestion
{
    public string Category { get; set; } = string.Empty; // plot, character, style, structure, etc.
    public string Severity { get; set; } = string.Empty; // critical, major, minor
    public string Description { get; set; } = string.Empty;
    public string? Location { get; set; } // Chapter X, Section Y, etc.
    public string Suggestion { get; set; } = string.Empty;
}
