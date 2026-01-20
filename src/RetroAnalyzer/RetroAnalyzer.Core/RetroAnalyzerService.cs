using System.Text.Json;
using Microsoft.Extensions.Logging;
using Models;
using OpenAIShared;

namespace RetroAnalyzer.Core;

/// <summary>
/// Service for analyzing sprint retrospective comments
/// </summary>
public class RetroAnalyzerService
{
    private readonly OpenAIClient _openAIClient;
    private readonly ILogger<RetroAnalyzerService> _logger;
    private readonly string _model;

    public RetroAnalyzerService(
        OpenAIClient openAIClient,
        ILogger<RetroAnalyzerService> logger,
        string model = "gpt-4-turbo-preview")
    {
        _openAIClient = openAIClient;
        _logger = logger;
        _model = model;
    }

    /// <summary>
    /// Analyzes retrospective comments and extracts action items
    /// </summary>
    public async Task<List<ActionItem>> ExtractActionItemsAsync(
        List<string> comments,
        CancellationToken cancellationToken = default)
    {
        var commentsText = string.Join("\n", comments.Select((c, i) => $"{i + 1}. {c}"));

        var functionDefinition = new FunctionDefinition
        {
            Name = "extract_action_items",
            Description = "Extracts action items from retrospective comments",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    actionItems = new
                    {
                        type = "array",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                description = new { type = "string" },
                                owner = new { type = "string" },
                                priority = new { type = "string" }
                            },
                            required = new[] { "description" }
                        }
                    }
                },
                required = new[] { "actionItems" }
            }
        };

        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert Scrum Master. Extract actionable items from retrospective comments.")
            .WithInstruction("Analyze the following retrospective comments and extract concrete action items. Identify owners when mentioned.")
            .WithInput(commentsText)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert Scrum Master." },
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
            return ParseActionItemsFromFunctionCall(message.FunctionCall.Arguments);
        }

        return new List<ActionItem>();
    }

    /// <summary>
    /// Identifies recurring themes in retrospective comments
    /// </summary>
    public async Task<List<string>> IdentifyThemesAsync(
        List<string> comments,
        CancellationToken cancellationToken = default)
    {
        var commentsText = string.Join("\n", comments.Select((c, i) => $"{i + 1}. {c}"));

        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert Scrum Master. Identify recurring themes and patterns in retrospective feedback.")
            .WithInstruction("Analyze the following retrospective comments and identify 3-5 recurring themes or patterns. List them concisely.")
            .WithInput(commentsText)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert Scrum Master." },
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 500
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var content = response.Choices.FirstOrDefault()?.Message?.Content ?? string.Empty;

        // Parse themes from response (simple line-by-line parsing)
        return content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Trim().TrimStart('-', '*', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '.'))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();
    }

    /// <summary>
    /// Analyzes sentiment of retrospective comments
    /// </summary>
    public async Task<SentimentAnalysis> AnalyzeSentimentAsync(
        List<string> comments,
        CancellationToken cancellationToken = default)
    {
        var commentsText = string.Join("\n", comments.Select((c, i) => $"{i + 1}. {c}"));

        var functionDefinition = new FunctionDefinition
        {
            Name = "analyze_sentiment",
            Description = "Analyzes sentiment of retrospective comments",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    overallSentiment = new { type = "string", description = "positive, neutral, or negative" },
                    sentimentScore = new { type = "number", description = "Score from -1 (very negative) to 1 (very positive)" },
                    keyInsights = new
                    {
                        type = "array",
                        items = new { type = "string" }
                    }
                },
                required = new[] { "overallSentiment", "sentimentScore" }
            }
        };

        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert at analyzing team sentiment from retrospective comments.")
            .WithInstruction("Analyze the sentiment of the following retrospective comments. Provide an overall sentiment and key insights.")
            .WithInput(commentsText)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert at analyzing team sentiment." },
                new() { Role = "user", Content = prompt }
            },
            Functions = new List<FunctionDefinition> { functionDefinition },
            FunctionCall = "auto",
            Temperature = 0.3,
            MaxTokens = 1000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var message = response.Choices.FirstOrDefault()?.Message;

        if (message?.FunctionCall != null)
        {
            return ParseSentimentFromFunctionCall(message.FunctionCall.Arguments);
        }

        return new SentimentAnalysis
        {
            OverallSentiment = "neutral",
            SentimentScore = 0.0
        };
    }

    /// <summary>
    /// Generates improvement suggestions based on retrospective comments
    /// </summary>
    public async Task<string> GenerateImprovementSuggestionsAsync(
        List<string> comments,
        CancellationToken cancellationToken = default)
    {
        var commentsText = string.Join("\n", comments.Select((c, i) => $"{i + 1}. {c}"));

        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert Scrum Master. Provide actionable improvement suggestions.")
            .WithInstruction("Based on the following retrospective comments, provide 5-7 concrete improvement suggestions for the team.")
            .WithInput(commentsText)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert Scrum Master." },
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.5,
            MaxTokens = 1500
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        return response.Choices.FirstOrDefault()?.Message?.Content ?? "Unable to generate suggestions.";
    }

    private List<ActionItem> ParseActionItemsFromFunctionCall(string argumentsJson)
    {
        var actionItems = new List<ActionItem>();

        try
        {
            var arguments = JsonSerializer.Deserialize<JsonElement>(argumentsJson);
            if (arguments.TryGetProperty("actionItems", out var itemsArray))
            {
                foreach (var itemElement in itemsArray.EnumerateArray())
                {
                    var actionItem = new ActionItem
                    {
                        Description = itemElement.GetProperty("description").GetString() ?? string.Empty,
                        Owner = itemElement.TryGetProperty("owner", out var owner) ? owner.GetString() : null,
                        Priority = itemElement.TryGetProperty("priority", out var priority) ? priority.GetString() : null
                    };

                    actionItems.Add(actionItem);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing action items from function call");
        }

        return actionItems;
    }

    private SentimentAnalysis ParseSentimentFromFunctionCall(string argumentsJson)
    {
        try
        {
            var arguments = JsonSerializer.Deserialize<JsonElement>(argumentsJson);
            var analysis = new SentimentAnalysis
            {
                OverallSentiment = arguments.GetProperty("overallSentiment").GetString() ?? "neutral",
                SentimentScore = arguments.GetProperty("sentimentScore").GetDouble()
            };

            if (arguments.TryGetProperty("keyInsights", out var insights))
            {
                analysis.KeyInsights = insights.EnumerateArray()
                    .Select(i => i.GetString() ?? string.Empty)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing sentiment from function call");
            return new SentimentAnalysis
            {
                OverallSentiment = "neutral",
                SentimentScore = 0.0
            };
        }
    }
}

public class SentimentAnalysis
{
    public string OverallSentiment { get; set; } = string.Empty;
    public double SentimentScore { get; set; }
    public List<string> KeyInsights { get; set; } = new();
}
