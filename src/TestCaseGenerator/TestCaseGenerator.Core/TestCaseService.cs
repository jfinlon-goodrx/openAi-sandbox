using System.Text.Json;
using Microsoft.Extensions.Logging;
using Models;
using OpenAIShared;

namespace TestCaseGenerator.Core;

/// <summary>
/// Service for generating test cases from code and user stories
/// </summary>
public class TestCaseService
{
    private readonly OpenAIClient _openAIClient;
    private readonly ILogger<TestCaseService> _logger;
    private readonly string _model;

    public TestCaseService(
        OpenAIClient openAIClient,
        ILogger<TestCaseService> logger,
        string model = "gpt-4-turbo-preview")
    {
        _openAIClient = openAIClient;
        _logger = logger;
        _model = model;
    }

    /// <summary>
    /// Generates unit tests from code
    /// </summary>
    public async Task<List<TestCase>> GenerateUnitTestsAsync(
        string code,
        string testFramework = "xUnit",
        CancellationToken cancellationToken = default)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage($"You are an expert software tester. Generate comprehensive unit tests using {testFramework}.")
            .WithInstruction("Analyze the following code and generate unit tests. Include tests for normal cases, edge cases, and error conditions.")
            .WithInput(code)
            .Build();

        var functionDefinition = new FunctionDefinition
        {
            Name = "generate_test_cases",
            Description = "Generates test cases from code",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    testCases = new
                    {
                        type = "array",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                name = new { type = "string" },
                                description = new { type = "string" },
                                testType = new { type = "string" },
                                steps = new
                                {
                                    type = "array",
                                    items = new { type = "string" }
                                },
                                expectedResult = new { type = "string" },
                                tags = new
                                {
                                    type = "array",
                                    items = new { type = "string" }
                                }
                            },
                            required = new[] { "name", "description", "steps", "expectedResult" }
                        }
                    }
                },
                required = new[] { "testCases" }
            }
        };

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = $"You are an expert software tester specializing in {testFramework}." },
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
            return ParseTestCasesFromFunctionCall(message.FunctionCall.Arguments);
        }

        return new List<TestCase>();
    }

    /// <summary>
    /// Generates test cases from user stories
    /// </summary>
    public async Task<List<TestCase>> GenerateTestCasesFromUserStoryAsync(
        string userStory,
        CancellationToken cancellationToken = default)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert software tester. Generate test cases from user stories.")
            .WithInstruction("Analyze the following user story and generate comprehensive test cases covering all acceptance criteria and edge cases.")
            .WithInput(userStory)
            .Build();

        var functionDefinition = new FunctionDefinition
        {
            Name = "generate_test_cases",
            Description = "Generates test cases from user stories",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    testCases = new
                    {
                        type = "array",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                name = new { type = "string" },
                                description = new { type = "string" },
                                testType = new { type = "string" },
                                steps = new
                                {
                                    type = "array",
                                    items = new { type = "string" }
                                },
                                expectedResult = new { type = "string" },
                                tags = new
                                {
                                    type = "array",
                                    items = new { type = "string" }
                                }
                            },
                            required = new[] { "name", "description", "steps", "expectedResult" }
                        }
                    }
                },
                required = new[] { "testCases" }
            }
        };

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert software tester." },
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
            return ParseTestCasesFromFunctionCall(message.FunctionCall.Arguments);
        }

        return new List<TestCase>();
    }

    /// <summary>
    /// Suggests edge cases and boundary conditions
    /// </summary>
    public async Task<string> SuggestEdgeCasesAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert software tester. Identify edge cases and boundary conditions.")
            .WithInstruction("Analyze the following code and suggest edge cases, boundary conditions, and potential failure scenarios that should be tested.")
            .WithInput(code)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert software tester." },
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 1500
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        return response.Choices.FirstOrDefault()?.Message?.Content ?? "Unable to generate suggestions.";
    }

    private List<TestCase> ParseTestCasesFromFunctionCall(string argumentsJson)
    {
        var testCases = new List<TestCase>();

        try
        {
            var arguments = JsonSerializer.Deserialize<JsonElement>(argumentsJson);
            if (arguments.TryGetProperty("testCases", out var testCasesArray))
            {
                foreach (var testCaseElement in testCasesArray.EnumerateArray())
                {
                    var testCase = new TestCase
                    {
                        Name = testCaseElement.GetProperty("name").GetString() ?? string.Empty,
                        Description = testCaseElement.GetProperty("description").GetString() ?? string.Empty,
                        TestType = testCaseElement.TryGetProperty("testType", out var type) ? type.GetString() : "Unit",
                        ExpectedResult = testCaseElement.GetProperty("expectedResult").GetString()
                    };

                    if (testCaseElement.TryGetProperty("steps", out var steps))
                    {
                        testCase.Steps = steps.EnumerateArray()
                            .Select(s => s.GetString() ?? string.Empty)
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToList();
                    }

                    if (testCaseElement.TryGetProperty("tags", out var tags))
                    {
                        testCase.Tags = tags.EnumerateArray()
                            .Select(t => t.GetString() ?? string.Empty)
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToList();
                    }

                    testCases.Add(testCase);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing test cases from function call");
        }

        return testCases;
    }
}
