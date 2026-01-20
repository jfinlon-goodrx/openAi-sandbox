using System.Text.Json;
using Microsoft.Extensions.Logging;
using Models;
using OpenAIShared;

namespace MeetingAnalyzer.Core;

/// <summary>
/// Service for analyzing meeting transcripts
/// </summary>
public class MeetingService
{
    private readonly OpenAIClient _openAIClient;
    private readonly ILogger<MeetingService> _logger;
    private readonly string _model;

    public MeetingService(
        OpenAIClient openAIClient,
        ILogger<MeetingService> logger,
        string model = "gpt-4-turbo-preview")
    {
        _openAIClient = openAIClient;
        _logger = logger;
        _model = model;
    }

    /// <summary>
    /// Transcribes audio file using Whisper API
    /// </summary>
    public async Task<string> TranscribeAudioAsync(
        Stream audioStream,
        string fileName,
        string? language = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _openAIClient.TranscribeAudioAsync(audioStream, fileName, language, cancellationToken);
        return response.Text;
    }

    /// <summary>
    /// Generates meeting summary from transcript
    /// </summary>
    public async Task<string> GenerateSummaryAsync(
        string transcript,
        CancellationToken cancellationToken = default)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert at summarizing meetings. Create clear, concise summaries.")
            .WithInstruction("Summarize the following meeting transcript. Include key topics discussed, decisions made, and important points.")
            .WithInput(transcript)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert at summarizing meetings." },
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 1500
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        return response.Choices.FirstOrDefault()?.Message?.Content ?? "Unable to generate summary.";
    }

    /// <summary>
    /// Extracts action items from meeting transcript
    /// </summary>
    public async Task<List<ActionItem>> ExtractActionItemsAsync(
        string transcript,
        CancellationToken cancellationToken = default)
    {
        var functionDefinition = new FunctionDefinition
        {
            Name = "extract_action_items",
            Description = "Extracts action items from meeting transcript",
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
                                dueDate = new { type = "string", description = "ISO date string or null" },
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
            .WithSystemMessage("You are an expert at extracting action items from meetings.")
            .WithInstruction("Analyze the following meeting transcript and extract all action items. Identify owners when mentioned.")
            .WithInput(transcript)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert at extracting action items from meetings." },
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
    /// Generates follow-up email from meeting transcript
    /// </summary>
    public async Task<string> GenerateFollowUpEmailAsync(
        string transcript,
        List<string> attendees,
        CancellationToken cancellationToken = default)
    {
        var attendeesText = string.Join(", ", attendees);
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert at writing professional follow-up emails.")
            .WithInstruction($"Generate a professional follow-up email for a meeting with attendees: {attendeesText}. Include summary, action items, and next steps.")
            .WithInput(transcript)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert at writing professional follow-up emails." },
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.5,
            MaxTokens = 1500
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        return response.Choices.FirstOrDefault()?.Message?.Content ?? "Unable to generate email.";
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

                    if (itemElement.TryGetProperty("dueDate", out var dueDate) && dueDate.ValueKind != JsonValueKind.Null)
                    {
                        var dueDateStr = dueDate.GetString();
                        if (!string.IsNullOrEmpty(dueDateStr) && DateTime.TryParse(dueDateStr, out var parsedDate))
                        {
                            actionItem.DueDate = parsedDate;
                        }
                    }

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
}
