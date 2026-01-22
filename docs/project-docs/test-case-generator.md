# Automated Test Case Generator

## Overview

The Test Case Generator helps Testers and Developers automatically generate test cases from code or user stories, identify edge cases, and create comprehensive test coverage.

## Features

- **Code Analysis**: Generate unit tests from code files
- **User Story Testing**: Generate test cases from user stories
- **Edge Case Identification**: Suggest boundary conditions and edge cases
- **Multiple Framework Support**: xUnit, MSTest, NUnit
- **CLI Tool**: Command-line interface for quick test generation

## Architecture

```
┌─────────────────┐
│   CLI Tool      │
└────────┬────────┘
         │
┌────────▼────────┐
│ Test Case       │
│ Service         │
└────────┬────────┘
         │
┌────────▼────────┐
│  OpenAI Client  │
└─────────────────┘
```

## Setup

1. Configure OpenAI API key (see [Setup Guide](../getting-started/01-setup.md))

2. Build the CLI tool:
```bash
cd src/TestCaseGenerator/TestCaseGenerator.Cli
dotnet build
```

## CLI Usage

### Generate Tests from Code

```bash
dotnet run --project TestCaseGenerator.Cli -- from-code MyClass.cs
```

### Generate Tests from User Story

```bash
dotnet run --project TestCaseGenerator.Cli -- from-story "As a user, I want to login so that I can access my account"
```

### Suggest Edge Cases

```bash
dotnet run --project TestCaseGenerator.Cli -- edge-cases MyClass.cs
```

## Programmatic Usage

### Generate Unit Tests

```csharp
var testService = new TestCaseService(openAIClient, logger);

var code = @"
public class Calculator
{
    public int Add(int a, int b) => a + b;
    public int Divide(int a, int b) => a / b;
}
";

var testCases = await testService.GenerateUnitTestsAsync(code, "xUnit");

foreach (var testCase in testCases)
{
    Console.WriteLine($"Test: {testCase.Name}");
    Console.WriteLine($"Description: {testCase.Description}");
    Console.WriteLine("Steps:");
    foreach (var step in testCase.Steps)
    {
        Console.WriteLine($"  {step}");
    }
    Console.WriteLine($"Expected: {testCase.ExpectedResult}");
}
```

### Generate from User Story

```csharp
var userStory = @"
As a user, I want to login to the system
so that I can access my account.

Acceptance Criteria:
- User can login with email and password
- Invalid credentials show error message
- Account is locked after 3 failed attempts
";

var testCases = await testService.GenerateTestCasesFromUserStoryAsync(userStory);
```

### Identify Edge Cases

```csharp
var suggestions = await testService.SuggestEdgeCasesAsync(code);
Console.WriteLine(suggestions);
```

## Output Format

Generated test cases include:

- **Name**: Test case name
- **Description**: What the test verifies
- **Test Type**: Unit, Integration, E2E
- **Steps**: Test execution steps
- **Expected Result**: Expected outcome
- **Tags**: Categorization tags

## Integration with Test Frameworks

### xUnit Example

```csharp
[Fact]
public void Add_TwoNumbers_ReturnsSum()
{
    // Arrange
    var calculator = new Calculator();
    
    // Act
    var result = calculator.Add(2, 3);
    
    // Assert
    Assert.Equal(5, result);
}
```

### MSTest Example

```csharp
[TestMethod]
public void Add_TwoNumbers_ReturnsSum()
{
    // Arrange
    var calculator = new Calculator();
    
    // Act
    var result = calculator.Add(2, 3);
    
    // Assert
    Assert.AreEqual(5, result);
}
```

## Best Practices

1. **Review Generated Tests**: Always review and validate AI-generated tests
2. **Test the Tests**: Run generated tests to ensure they compile and work
3. **Refine Prompts**: Adjust prompts based on your testing standards
4. **Combine Approaches**: Use AI for initial generation, then refine manually
5. **Maintain Coverage**: Ensure all acceptance criteria are covered

## Troubleshooting

**Issue:** Generated tests don't compile
- **Solution:** Review and fix syntax. AI is a helper, not perfect

**Issue:** Tests miss important scenarios
- **Solution:** Provide more context in prompts, include examples

**Issue:** Too many tests generated
- **Solution:** Adjust prompts to focus on high-priority scenarios

## Slack Integration

Share test case generation results with your team:

```csharp
var slackIntegration = new SlackIntegration(httpClient, logger, slackWebhookUrl);

// After generating test cases
await slackIntegration.SendFormattedMessageAsync(
    title: "🧪 Test Cases Generated",
    text: $"Generated {testCases.Count} test cases for feature: {featureName}",
    fields: testCases.Take(5).Select(tc => new SlackField
    {
        Title = tc.Name,
        Value = tc.Description
    }).ToList(),
    channel: "#qa"
);
```

See [Slack Integration Guide](../integrations/slack-integration.md) for more examples.

## Related Documentation

- [Tester Guide](../role-guides/tester-guide.md)
- [Developer Guide](../role-guides/developer-guide.md)
- [Slack Integration Guide](../integrations/slack-integration.md) ⭐ NEW
- [Getting Started](../getting-started/)
