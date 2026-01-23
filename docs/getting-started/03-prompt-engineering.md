# Prompt Engineering Guide

**For:** Everyone - Learn how to write effective prompts for better AI results

**What is prompt engineering?** The art and science of writing instructions (prompts) that get the best results from AI models. Like giving clear directions to a colleague - better instructions lead to better outcomes.

This guide covers best practices for writing prompts that produce accurate, useful, and cost-effective results.

## What You'll Learn

- How to structure prompts for clarity
- Techniques for getting better results
- Common patterns and examples
- How to iterate and improve your prompts
- Troubleshooting when results aren't what you expect

## Basic Principles

### 1. Be Clear and Specific

**Bad:**
```
Summarize this document.
```

**Good:**
```
Summarize the following requirements document. Focus on:
- Key stakeholders mentioned
- Main functional requirements
- Non-functional requirements (performance, security)
- Any constraints or dependencies

Keep the summary under 200 words.
```

### 2. Provide Context

**Bad:**
```
Generate user stories.
```

**Good:**
```
You are an expert business analyst. Generate user stories from the following requirements document.
Use the format: "As a [user type], I want [goal], so that [benefit]."
Include 3-5 acceptance criteria for each story.
```

### 3. Use Examples (Few-Shot Learning)

```csharp
var prompt = new PromptBuilder()
    .WithSystemMessage("You are an expert code reviewer.")
    .WithExample(
        "public int Add(int a, int b) { return a + b; }",
        "Good: Simple, clear method. Consider adding null checks if parameters could be nullable."
    )
    .WithExample(
        "public void ProcessData(string data) { var result = data.Split(','); }",
        "Warning: No null check. Add: if (string.IsNullOrEmpty(data)) throw new ArgumentNullException(nameof(data));"
    )
    .WithInput(code)
    .Build();
```

## Using PromptBuilder

The `PromptBuilder` class helps structure prompts:

```csharp
using OpenAIShared;

var prompt = new PromptBuilder()
    .WithSystemMessage("You are an expert business analyst.")
    .WithContext("The following document contains requirements for a new e-commerce platform.")
    .WithInstruction("Extract user stories with acceptance criteria.")
    .WithInput(documentContent)
    .Build();
```

## Common Patterns

### 1. Role-Based Prompts

Always set a role for the AI:

```csharp
.WithSystemMessage("You are an expert software tester specializing in .NET applications.")
```

### 2. Chain-of-Thought Reasoning

Encourage step-by-step thinking:

```csharp
.WithInstruction("Analyze this code step by step:\n1. Identify potential security issues\n2. Check for performance problems\n3. Review code style\n4. Suggest improvements")
```

### 3. Structured Output

Use function calling for structured data:

```csharp
var functionDefinition = new FunctionDefinition
{
    Name = "extract_action_items",
    Description = "Extracts action items from text",
    Parameters = new
    {
        type = "object",
        properties = new
        {
            actionItems = new
            {
                type = "array",
                items = new { /* ... */ }
            }
        }
    }
};
```

## Temperature Guidelines

Choose temperature based on task:

| Task Type | Temperature | Example |
|-----------|-------------|---------|
| Code generation | 0.2-0.3 | Generate unit tests |
| Code review | 0.2-0.3 | Review code for bugs |
| Summarization | 0.3-0.5 | Summarize documents |
| Creative writing | 0.7-1.0 | Generate user stories |
| Brainstorming | 1.0-1.5 | Generate ideas |

## Advanced Techniques

### 1. Iterative Refinement

Start with a basic prompt, then refine based on results:

```csharp
// First attempt
var prompt1 = "Summarize this document.";

// Refined based on results
var prompt2 = "Summarize this requirements document. Focus on functional requirements, " +
              "identify stakeholders, and highlight any technical constraints. " +
              "Format as bullet points.";
```

### 2. Prompt Templates

Create reusable templates:

```csharp
public static class PromptTemplates
{
    public static string CodeReview(string code, string language) =>
        $"You are an expert {language} code reviewer. Review the following code:\n\n{code}";
    
    public static string UserStory(string requirement) =>
        $"As a business analyst, convert this requirement into a user story:\n{requirement}";
}
```

### 3. Multi-Step Prompts

Break complex tasks into steps:

```csharp
// Step 1: Extract requirements
var step1 = "Extract all functional requirements from this document.";

// Step 2: Generate user stories
var step2 = $"Based on these requirements: {step1Result}, generate user stories.";

// Step 3: Create acceptance criteria
var step3 = $"For each user story: {step2Result}, create acceptance criteria.";
```

## Common Mistakes to Avoid

1. **Too vague**: "Fix this code" → Be specific about what to fix
2. **No context**: Provide background information
3. **Wrong temperature**: Use low temperature for deterministic tasks
4. **Too long prompts**: Keep prompts concise but complete
5. **No examples**: Provide examples for complex tasks
6. **Ignoring system messages**: Always set a role/context

## Testing Prompts

Test prompts with different inputs:

```csharp
var testCases = new[]
{
    "Simple requirement",
    "Complex requirement with multiple stakeholders",
    "Requirement with technical constraints"
};

foreach (var testCase in testCases)
{
    var result = await TestPrompt(testCase);
    Console.WriteLine($"Input: {testCase}\nOutput: {result}\n");
}
```

## Resources

- [OpenAI Prompt Engineering Guide](https://platform.openai.com/docs/guides/prompt-engineering)
- [Best Practices for Prompt Engineering](https://help.openai.com/en/articles/6654000-best-practices-for-prompt-engineering-with-openai-api)

## Next Steps

- Build your [First Project](04-first-project.md)
- Check out [Role-Specific Guides](../role-guides/) for examples
- Review [Best Practices](../best-practices/) for production use
