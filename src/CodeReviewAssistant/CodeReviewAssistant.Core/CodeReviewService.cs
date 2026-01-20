using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenAIShared;

namespace CodeReviewAssistant.Core;

/// <summary>
/// Service for performing intelligent code reviews
/// </summary>
public class CodeReviewService
{
    private readonly OpenAIClient _openAIClient;
    private readonly ILogger<CodeReviewService> _logger;
    private readonly string _model;

    public CodeReviewService(
        OpenAIClient openAIClient,
        ILogger<CodeReviewService> logger,
        string model = "gpt-4-turbo-preview")
    {
        _openAIClient = openAIClient;
        _logger = logger;
        _model = model;
    }

    /// <summary>
    /// Reviews code and provides suggestions
    /// </summary>
    public async Task<CodeReviewResult> ReviewCodeAsync(
        string code,
        string? language = null,
        CancellationToken cancellationToken = default)
    {
        var functionDefinition = new FunctionDefinition
        {
            Name = "code_review",
            Description = "Performs comprehensive code review",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    comments = new
                    {
                        type = "array",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                line = new { type = "number" },
                                severity = new { type = "string", description = "info, warning, or error" },
                                category = new { type = "string", description = "security, performance, style, bug, or suggestion" },
                                message = new { type = "string" },
                                suggestion = new { type = "string" }
                            },
                            required = new[] { "line", "severity", "category", "message" }
                        }
                    },
                    summary = new { type = "string" },
                    securityIssues = new { type = "number" },
                    performanceIssues = new { type = "number" },
                    styleIssues = new { type = "number" }
                },
                required = new[] { "comments", "summary" }
            }
        };

        var languageContext = !string.IsNullOrEmpty(language) ? $" ({language})" : "";
        var prompt = new PromptBuilder()
            .WithSystemMessage($"You are an expert code reviewer{languageContext}. Review code for security, performance, style, and bugs.")
            .WithInstruction("Review the following code and provide detailed feedback. Focus on security vulnerabilities, performance issues, code style, and potential bugs.")
            .WithInput(code)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = $"You are an expert code reviewer{languageContext}." },
                new() { Role = "user", Content = prompt }
            },
            Functions = new List<FunctionDefinition> { functionDefinition },
            FunctionCall = "auto",
            Temperature = 0.2,
            MaxTokens = 3000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var message = response.Choices.FirstOrDefault()?.Message;

        if (message?.FunctionCall != null)
        {
            return ParseReviewResultFromFunctionCall(message.FunctionCall.Arguments);
        }

        // Fallback to text response
        return new CodeReviewResult
        {
            Summary = message?.Content ?? "Unable to review code.",
            Comments = new List<CodeReviewComment>()
        };
    }

    /// <summary>
    /// Generates a PR summary from code changes
    /// </summary>
    public async Task<string> GeneratePRSummaryAsync(
        string codeChanges,
        CancellationToken cancellationToken = default)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert at summarizing code changes for pull requests.")
            .WithInstruction("Summarize the following code changes in a clear, concise manner suitable for a pull request description.")
            .WithInput(codeChanges)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert at summarizing code changes." },
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 1000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        return response.Choices.FirstOrDefault()?.Message?.Content ?? "Unable to generate summary.";
    }

    /// <summary>
    /// Explains complex code sections
    /// </summary>
    public async Task<string> ExplainCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert at explaining code in simple terms.")
            .WithInstruction("Explain what the following code does, how it works, and why it might be written this way.")
            .WithInput(code)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert at explaining code." },
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 1500
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        return response.Choices.FirstOrDefault()?.Message?.Content ?? "Unable to explain code.";
    }

    private CodeReviewResult ParseReviewResultFromFunctionCall(string argumentsJson)
    {
        try
        {
            var arguments = JsonSerializer.Deserialize<JsonElement>(argumentsJson);
            var result = new CodeReviewResult
            {
                Summary = arguments.GetProperty("summary").GetString() ?? string.Empty,
                Comments = new List<CodeReviewComment>()
            };

            if (arguments.TryGetProperty("comments", out var commentsArray))
            {
                foreach (var commentElement in commentsArray.EnumerateArray())
                {
                    var comment = new CodeReviewComment
                    {
                        Line = commentElement.TryGetProperty("line", out var line) ? line.GetInt32() : 0,
                        Severity = commentElement.GetProperty("severity").GetString() ?? "info",
                        Category = commentElement.GetProperty("category").GetString() ?? "suggestion",
                        Message = commentElement.GetProperty("message").GetString() ?? string.Empty,
                        Suggestion = commentElement.TryGetProperty("suggestion", out var suggestion) ? suggestion.GetString() : null
                    };

                    result.Comments.Add(comment);
                }
            }

            if (arguments.TryGetProperty("securityIssues", out var security))
                result.SecurityIssues = security.GetInt32();
            if (arguments.TryGetProperty("performanceIssues", out var performance))
                result.PerformanceIssues = performance.GetInt32();
            if (arguments.TryGetProperty("styleIssues", out var style))
                result.StyleIssues = style.GetInt32();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing review result from function call");
            return new CodeReviewResult
            {
                Summary = "Error parsing review results.",
                Comments = new List<CodeReviewComment>()
            };
        }
    }
}

public class CodeReviewResult
{
    public string Summary { get; set; } = string.Empty;
    public List<CodeReviewComment> Comments { get; set; } = new();
    public int SecurityIssues { get; set; }
    public int PerformanceIssues { get; set; }
    public int StyleIssues { get; set; }
}

public class CodeReviewComment
{
    public int Line { get; set; }
    public string Severity { get; set; } = "info";
    public string Category { get; set; } = "suggestion";
    public string Message { get; set; } = string.Empty;
    public string? Suggestion { get; set; }
}
