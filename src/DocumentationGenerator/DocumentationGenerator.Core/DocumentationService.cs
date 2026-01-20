using Microsoft.Extensions.Logging;
using OpenAIShared;

namespace DocumentationGenerator.Core;

/// <summary>
/// Service for generating documentation from code
/// </summary>
public class DocumentationService
{
    private readonly OpenAIClient _openAIClient;
    private readonly ILogger<DocumentationService> _logger;
    private readonly string _model;

    public DocumentationService(
        OpenAIClient openAIClient,
        ILogger<DocumentationService> logger,
        string model = "gpt-4-turbo-preview")
    {
        _openAIClient = openAIClient;
        _logger = logger;
        _model = model;
    }

    /// <summary>
    /// Generates API documentation from code
    /// </summary>
    public async Task<string> GenerateApiDocumentationAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert technical writer. Generate clear, comprehensive API documentation.")
            .WithInstruction("Analyze the following code and generate API documentation including method signatures, parameters, return values, and usage examples.")
            .WithInput(code)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert technical writer." },
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 3000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        return response.Choices.FirstOrDefault()?.Message?.Content ?? "Unable to generate documentation.";
    }

    /// <summary>
    /// Generates README from code and comments
    /// </summary>
    public async Task<string> GenerateReadmeAsync(
        string code,
        string? projectName = null,
        CancellationToken cancellationToken = default)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert at writing README files for software projects.")
            .WithInstruction($"Generate a comprehensive README file{(projectName != null ? $" for {projectName}" : "")} based on the following code. Include installation, usage, examples, and API overview.")
            .WithInput(code)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert at writing README files." },
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 2000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        return response.Choices.FirstOrDefault()?.Message?.Content ?? "Unable to generate README.";
    }

    /// <summary>
    /// Generates changelog from commit messages
    /// </summary>
    public async Task<string> GenerateChangelogAsync(
        List<string> commitMessages,
        CancellationToken cancellationToken = default)
    {
        var commitsText = string.Join("\n", commitMessages.Select((c, i) => $"{i + 1}. {c}"));

        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert at writing changelogs.")
            .WithInstruction("Generate a changelog from the following commit messages. Organize by category (Added, Changed, Fixed, etc.) and format professionally.")
            .WithInput(commitsText)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert at writing changelogs." },
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 2000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        return response.Choices.FirstOrDefault()?.Message?.Content ?? "Unable to generate changelog.";
    }
}
