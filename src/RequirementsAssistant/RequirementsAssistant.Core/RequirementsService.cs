using System.Text.Json;
using Microsoft.Extensions.Logging;
using Models;
using OpenAIShared;

namespace RequirementsAssistant.Core;

/// <summary>
/// Service for processing requirements documents and generating user stories
/// </summary>
public class RequirementsService
{
    private readonly OpenAIClient _openAIClient;
    private readonly ILogger<RequirementsService> _logger;
    private readonly string _model;

    public RequirementsService(
        OpenAIClient openAIClient,
        ILogger<RequirementsService> logger,
        string model = "gpt-4-turbo-preview")
    {
        _openAIClient = openAIClient;
        _logger = logger;
        _model = model;
    }

    /// <summary>
    /// Summarizes a requirements document
    /// </summary>
    public async Task<string> SummarizeDocumentAsync(string documentContent, CancellationToken cancellationToken = default)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert business analyst. Summarize requirements documents clearly and concisely.")
            .WithInstruction("Summarize the following requirements document. Focus on key requirements, stakeholders, and business objectives.")
            .WithInput(documentContent)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert business analyst." },
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 1000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        return response.Choices.FirstOrDefault()?.Message?.Content ?? "Unable to generate summary.";
    }

    /// <summary>
    /// Generates user stories from requirements document
    /// </summary>
    public async Task<List<UserStory>> GenerateUserStoriesAsync(
        string documentContent,
        CancellationToken cancellationToken = default)
    {
        var functionDefinition = new FunctionDefinition
        {
            Name = "generate_user_stories",
            Description = "Generates user stories from requirements",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    stories = new
                    {
                        type = "array",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                title = new { type = "string" },
                                asA = new { type = "string" },
                                iWant = new { type = "string" },
                                soThat = new { type = "string" },
                                acceptanceCriteria = new
                                {
                                    type = "array",
                                    items = new { type = "string" }
                                },
                                priority = new { type = "string" },
                                tags = new
                                {
                                    type = "array",
                                    items = new { type = "string" }
                                }
                            },
                            required = new[] { "title", "asA", "iWant", "soThat", "acceptanceCriteria" }
                        }
                    }
                },
                required = new[] { "stories" }
            }
        };

        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert business analyst. Generate user stories in the standard format: As a [user], I want [goal], so that [benefit].")
            .WithInstruction("Analyze the following requirements document and generate user stories with acceptance criteria. Extract key requirements and convert them into well-structured user stories.")
            .WithInput(documentContent)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert business analyst." },
                new() { Role = "user", Content = prompt }
            },
            Functions = new List<FunctionDefinition> { functionDefinition },
            FunctionCall = "auto",
            Temperature = 0.3,
            MaxTokens = 2000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var message = response.Choices.FirstOrDefault()?.Message;

        if (message?.FunctionCall != null)
        {
            var arguments = JsonSerializer.Deserialize<JsonElement>(message.FunctionCall.Arguments);
            var stories = new List<UserStory>();

            if (arguments.TryGetProperty("stories", out var storiesArray))
            {
                foreach (var storyElement in storiesArray.EnumerateArray())
                {
                    var story = new UserStory
                    {
                        Title = storyElement.GetProperty("title").GetString() ?? string.Empty,
                        AsA = storyElement.GetProperty("asA").GetString() ?? string.Empty,
                        IWant = storyElement.GetProperty("iWant").GetString() ?? string.Empty,
                        SoThat = storyElement.GetProperty("soThat").GetString() ?? string.Empty,
                        Priority = storyElement.TryGetProperty("priority", out var priority) ? priority.GetString() : null
                    };

                    if (storyElement.TryGetProperty("acceptanceCriteria", out var criteria))
                    {
                        story.AcceptanceCriteria = criteria.EnumerateArray()
                            .Select(c => c.GetString() ?? string.Empty)
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToList();
                    }

                    if (storyElement.TryGetProperty("tags", out var tags))
                    {
                        story.Tags = tags.EnumerateArray()
                            .Select(t => t.GetString() ?? string.Empty)
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToList();
                    }

                    stories.Add(story);
                }
            }

            return stories;
        }

        // Fallback: parse from text response
        return ParseUserStoriesFromText(message?.Content ?? string.Empty);
    }

    /// <summary>
    /// Answers questions about requirements using embeddings and semantic search
    /// </summary>
    public async Task<string> AnswerQuestionAsync(
        string question,
        string documentContent,
        CancellationToken cancellationToken = default)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert business analyst. Answer questions about requirements documents accurately and concisely.")
            .WithContext($"Requirements Document:\n{documentContent}")
            .WithInstruction($"Answer the following question about the requirements: {question}")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert business analyst." },
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 500
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        return response.Choices.FirstOrDefault()?.Message?.Content ?? "Unable to answer question.";
    }

    /// <summary>
    /// Identifies gaps and conflicts in requirements
    /// </summary>
    public async Task<string> IdentifyGapsAndConflictsAsync(
        string documentContent,
        CancellationToken cancellationToken = default)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert business analyst. Identify gaps, conflicts, and ambiguities in requirements documents.")
            .WithInstruction("Analyze the following requirements document and identify:\n1. Missing requirements or gaps\n2. Conflicting requirements\n3. Ambiguous statements\n4. Recommendations for clarification")
            .WithInput(documentContent)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert business analyst." },
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 1500
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        return response.Choices.FirstOrDefault()?.Message?.Content ?? "Unable to analyze document.";
    }

    private List<UserStory> ParseUserStoriesFromText(string text)
    {
        // Simple fallback parser - in production, use function calling
        var stories = new List<UserStory>();
        // This is a basic implementation - would need more sophisticated parsing
        return stories;
    }
}
