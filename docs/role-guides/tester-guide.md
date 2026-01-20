# Tester Guide

This guide helps Testers leverage OpenAI Platform capabilities for test case generation and quality assurance.

## Use Cases for Testers

### 1. Test Case Generation

Generate test cases from code or user stories:

```csharp
var testService = new TestCaseService(openAIClient, logger);

// Generate from code
var testCases = await testService.GenerateUnitTestsAsync(code, "xUnit");

// Generate from user story
var storyTests = await testService.GenerateTestCasesFromUserStoryAsync(userStory);

foreach (var testCase in testCases)
{
    Console.WriteLine($"Test: {testCase.Name}");
    Console.WriteLine($"Description: {testCase.Description}");
    Console.WriteLine($"Steps:");
    foreach (var step in testCase.Steps)
    {
        Console.WriteLine($"  {step}");
    }
    Console.WriteLine($"Expected: {testCase.ExpectedResult}");
}
```

### 2. Edge Case Identification

Identify edge cases and boundary conditions:

```csharp
var suggestions = await testService.SuggestEdgeCasesAsync(code);
Console.WriteLine(suggestions);
```

### 3. Test Data Generation

Generate test data (can be extended):

```csharp
// Example: Generate test data prompts
var prompt = $"Generate test data for a User class with properties: " +
             $"FirstName (string), LastName (string), Email (string), Age (int). " +
             $"Include edge cases: null values, empty strings, negative ages, invalid emails.";

var request = new ChatCompletionRequest
{
    Model = "gpt-4-turbo-preview",
    Messages = new List<ChatMessage>
    {
        new() { Role = "user", Content = prompt }
    }
};

var response = await openAIClient.GetChatCompletionAsync(request);
var testData = response.Choices.First().Message.Content;
```

### 4. Test Plan Generation

Generate test plans from requirements:

```csharp
var prompt = $"Create a comprehensive test plan for the following requirements:\n{requirements}";

var request = new ChatCompletionRequest
{
    Model = "gpt-4-turbo-preview",
    Messages = new List<ChatMessage>
    {
        new() 
        { 
            Role = "system", 
            Content = "You are an expert QA engineer. Create detailed test plans." 
        },
        new() { Role = "user", Content = prompt }
    },
    Temperature = 0.3,
    MaxTokens = 2000
};

var response = await openAIClient.GetChatCompletionAsync(request);
var testPlan = response.Choices.First().Message.Content;
```

## Daily Workflow Examples

### New Feature Testing

1. Get user story from Jira/Confluence
2. Generate test cases
3. Review and refine
4. Add to test management tool

### Code Review Testing

1. Get code changes from PR
2. Generate unit tests
3. Identify edge cases
4. Review test coverage

### Regression Testing

1. Analyze code changes
2. Generate regression test scenarios
3. Prioritize based on risk

## CLI Usage

Use the Test Case Generator CLI:

```bash
# Generate tests from code
dotnet run --project TestCaseGenerator.Cli -- from-code MyClass.cs

# Generate tests from user story
dotnet run --project TestCaseGenerator.Cli -- from-story "As a user, I want to login..."

# Suggest edge cases
dotnet run --project TestCaseGenerator.Cli -- edge-cases MyClass.cs
```

## Best Practices

1. **Review Generated Tests**: Always review and validate AI-generated tests
2. **Test the Tests**: Run generated tests to ensure they work
3. **Refine Prompts**: Adjust prompts based on your testing standards
4. **Combine Approaches**: Use AI for initial generation, then refine manually
5. **Maintain Coverage**: Ensure all acceptance criteria are covered

## Common Patterns

### Test Generation Pattern

```csharp
public async Task<List<TestCase>> GenerateTestsForFeature(string userStory)
{
    var testService = new TestCaseService(openAIClient, logger);
    
    // Generate test cases
    var testCases = await testService.GenerateTestCasesFromUserStoryAsync(userStory);
    
    // Get edge case suggestions
    var edgeCases = await testService.SuggestEdgeCasesAsync(code);
    
    // Combine and refine
    var allTests = testCases.Concat(ParseEdgeCasesToTests(edgeCases)).ToList();
    
    return allTests;
}
```

### Test Plan Pattern

```csharp
public async Task<string> GenerateTestPlan(string requirements)
{
    var prompt = new PromptBuilder()
        .WithSystemMessage("You are an expert QA engineer.")
        .WithInstruction("Create a comprehensive test plan including:\n" +
                        "1. Test scope\n" +
                        "2. Test approach\n" +
                        "3. Test cases by category\n" +
                        "4. Test data requirements\n" +
                        "5. Risk assessment")
        .WithInput(requirements)
        .Build();
    
    var request = new ChatCompletionRequest
    {
        Model = "gpt-4-turbo-preview",
        Messages = new List<ChatMessage>
        {
            new() { Role = "system", Content = "You are an expert QA engineer." },
            new() { Role = "user", Content = prompt }
        },
        Temperature = 0.3,
        MaxTokens = 3000
    };
    
    var response = await openAIClient.GetChatCompletionAsync(request);
    return response.Choices.First().Message.Content;
}
```

## Integration with Test Management Tools

### Export to TestRail/Zephyr

```csharp
// Example: Export test cases to TestRail format
foreach (var testCase in testCases)
{
    var testRailCase = new
    {
        title = testCase.Name,
        description = testCase.Description,
        steps = testCase.Steps,
        expected_result = testCase.ExpectedResult
    };
    
    // Post to TestRail API
    await PostToTestRail(testRailCase);
}
```

## Troubleshooting

**Issue:** Generated tests don't compile
- **Solution:** Review and fix syntax. AI is a helper, not perfect

**Issue:** Tests miss important scenarios
- **Solution:** Provide more context in prompts, include examples

**Issue:** Too many tests generated
- **Solution:** Adjust prompts to focus on high-priority scenarios

**Issue:** Edge cases not identified
- **Solution:** Use specific prompts asking for boundary conditions

## Resources

- [Test Case Generator Documentation](../project-docs/test-case-generator.md)
- [Getting Started Guide](../getting-started/)
- [Best Practices](../best-practices/)
