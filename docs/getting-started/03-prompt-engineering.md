# Prompt Engineering Guide

**For:** Everyone - Learn how to write effective prompts for better AI results

**What is prompt engineering?** The art and science of writing instructions (prompts) that get the best results from AI models. Like giving clear directions to a colleague - better instructions lead to better outcomes.

This guide covers best practices for writing prompts that produce accurate, useful, and cost-effective results.

## What You'll Learn

- How to structure prompts for clarity and maximum effectiveness
- Techniques for getting better results (few-shot learning, chain-of-thought, etc.)
- Common patterns and real-world examples from production code
- How to optimize prompts for cost and token usage
- How to use JSON Mode and Function Calling for structured outputs
- RAG prompt patterns for document Q&A
- How to iterate and improve your prompts
- Troubleshooting when results aren't what you expect
- REST API examples alongside C# code examples

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

Providing examples helps the AI understand the desired format and quality level. This is especially important for complex tasks.

**C# Example:**
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

**REST API Example (curl):**
```bash
curl https://api.openai.com/v1/chat/completions \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $OPENAI_API_KEY" \
  -d '{
    "model": "gpt-4-turbo-preview",
    "messages": [
      {
        "role": "system",
        "content": "You are an expert code reviewer."
      },
      {
        "role": "user",
        "content": "Example 1:\nInput: public int Add(int a, int b) { return a + b; }\nOutput: Good: Simple, clear method. Consider adding null checks if parameters could be nullable.\n\nExample 2:\nInput: public void ProcessData(string data) { var result = data.Split(\",\"); }\nOutput: Warning: No null check. Add: if (string.IsNullOrEmpty(data)) throw new ArgumentNullException(nameof(data));\n\nNow review this code:\n[YOUR_CODE_HERE]"
      }
    ],
    "temperature": 0.2
  }'
```

**Why this works:** Examples show the AI exactly what format and level of detail you want, reducing ambiguity and improving consistency.

## Using PromptBuilder

The `PromptBuilder` class helps structure prompts consistently. This is a real example from the Requirements Assistant:

**C# Example:**
```csharp
using OpenAIShared;

var prompt = new PromptBuilder()
    .WithSystemMessage("You are an expert business analyst. Generate user stories in the standard format: As a [user], I want [goal], so that [benefit].")
    .WithInstruction("Analyze the following requirements document and generate user stories with acceptance criteria. Extract key requirements and convert them into well-structured user stories.")
    .WithInput(documentContent)
    .Build();
```

**REST API Equivalent:**
```bash
curl https://api.openai.com/v1/chat/completions \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $OPENAI_API_KEY" \
  -d '{
    "model": "gpt-4-turbo-preview",
    "messages": [
      {
        "role": "system",
        "content": "You are an expert business analyst. Generate user stories in the standard format: As a [user], I want [goal], so that [benefit]."
      },
      {
        "role": "user",
        "content": "Analyze the following requirements document and generate user stories with acceptance criteria. Extract key requirements and convert them into well-structured user stories.\n\n[DOCUMENT_CONTENT]"
      }
    ],
    "temperature": 0.3,
    "max_tokens": 2000
  }'
```

**Python Equivalent:**
```python
import requests
import json

headers = {
    'Authorization': f'Bearer {api_key}',
    'Content-Type': 'application/json'
}

data = {
    'model': 'gpt-4-turbo-preview',
    'messages': [
        {
            'role': 'system',
            'content': 'You are an expert business analyst. Generate user stories in the standard format: As a [user], I want [goal], so that [benefit].'
        },
        {
            'role': 'user',
            'content': f'Analyze the following requirements document and generate user stories with acceptance criteria.\n\n{document_content}'
        }
    ],
    'temperature': 0.3,
    'max_tokens': 2000
}

response = requests.post('https://api.openai.com/v1/chat/completions', headers=headers, json=data)
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

### 3. Structured Output with Function Calling

Function calling ensures you get structured, parseable data. This example is from the Retro Analyzer:

**C# Example:**
```csharp
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
    Model = "gpt-4-turbo-preview",
    Messages = new List<ChatMessage>
    {
        new() { Role = "system", Content = "You are an expert Scrum Master." },
        new() { Role = "user", Content = prompt }
    },
    Functions = new List<FunctionDefinition> { functionDefinition },
    FunctionCall = "auto",
    Temperature = 0.3
};
```

**REST API Example:**
```bash
curl https://api.openai.com/v1/chat/completions \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $OPENAI_API_KEY" \
  -d '{
    "model": "gpt-4-turbo-preview",
    "messages": [
      {
        "role": "system",
        "content": "You are an expert Scrum Master. Extract actionable items from retrospective comments."
      },
      {
        "role": "user",
        "content": "Analyze the following retrospective comments and extract concrete action items:\n\n1. We need better documentation\n2. John will improve CI/CD pipeline\n3. Code reviews take too long"
      }
    ],
    "functions": [
      {
        "name": "extract_action_items",
        "description": "Extracts action items from retrospective comments",
        "parameters": {
          "type": "object",
          "properties": {
            "actionItems": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "description": { "type": "string" },
                  "owner": { "type": "string" },
                  "priority": { "type": "string" }
                },
                "required": ["description"]
              }
            }
          },
          "required": ["actionItems"]
        }
      }
    ],
    "function_call": "auto",
    "temperature": 0.3
  }'
```

**Why use Function Calling?**
- Guaranteed structured output (JSON)
- Type-safe parsing
- Better than parsing free-form text
- Reduces errors from inconsistent formats

## Temperature Guidelines

Temperature controls randomness. Lower = more deterministic, Higher = more creative. Choose based on task:

| Task Type | Temperature | Example | Why |
|-----------|-------------|---------|-----|
| Code generation | 0.2-0.3 | Generate unit tests | Need consistent, correct code |
| Code review | 0.2-0.3 | Review code for bugs | Need reliable, accurate analysis |
| Data extraction | 0.2-0.3 | Extract structured data | Need consistent format |
| Summarization | 0.3-0.5 | Summarize documents | Balance between accuracy and variety |
| User stories | 0.3-0.5 | Generate user stories | Some creativity but structured format |
| Creative writing | 0.7-1.0 | Marketing copy, ad copy | Need variety and creativity |
| Brainstorming | 0.8-1.2 | Generate ideas | Maximum creativity |

**Real Examples from Codebase:**
- Code Review: `Temperature = 0.2` (CodeReviewService)
- User Stories: `Temperature = 0.3` (RequirementsService)
- Retro Analysis: `Temperature = 0.3` (RetroAnalyzerService)
- Creative Briefs: `Temperature = 0.5` (AdvertisingService)
- Brand Voice: `Temperature = 0.5` (AdvertisingService)

**Pro Tip:** Start with lower temperatures (0.2-0.3) for most tasks. Only increase if you need more variety or creativity.

## Advanced Techniques

### 1. Iterative Refinement

Start with a basic prompt, then refine based on results. This is the most effective way to improve prompts:

**Iteration 1 - Too vague:**
```csharp
var prompt1 = "Summarize this document.";
// Result: Generic summary, missing important details
```

**Iteration 2 - More specific:**
```csharp
var prompt2 = "Summarize this requirements document. Focus on functional requirements, " +
              "identify stakeholders, and highlight any technical constraints. " +
              "Format as bullet points.";
// Result: Better, but still missing structure
```

**Iteration 3 - With role and structure:**
```csharp
var prompt3 = new PromptBuilder()
    .WithSystemMessage("You are an expert business analyst.")
    .WithInstruction("Summarize this requirements document. Include:\n" +
                    "1. Key stakeholders\n" +
                    "2. Main functional requirements\n" +
                    "3. Non-functional requirements (performance, security)\n" +
                    "4. Technical constraints\n" +
                    "Format as structured bullet points.")
    .WithInput(documentContent)
    .Build();
// Result: Much better - structured, comprehensive, consistent
```

**How to iterate:**
1. Test with real data
2. Identify what's missing or wrong
3. Add specificity to address gaps
4. Test again
5. Repeat until satisfied

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

Break complex tasks into steps. This is used in the Publishing Assistant for comprehensive manuscript reviews:

**Example: Multi-Step Manuscript Review**
```csharp
// Step 1: High-level summary
var summaryPrompt = new PromptBuilder()
    .WithSystemMessage("You are a senior publishing agent.")
    .WithInstruction("Provide a comprehensive high-level summary covering: main themes, target audience, market potential, and overall quality.")
    .WithInput(manuscriptContent)
    .Build();

var summary = await GetCompletionAsync(summaryPrompt);

// Step 2: Detailed analysis (using RAG for large documents)
var analysisPrompt = new PromptBuilder()
    .WithSystemMessage("You are a senior publishing agent providing detailed editorial feedback.")
    .WithContext(relevantChunks) // From RAG search
    .WithInstruction("Analyze the manuscript sections and identify specific issues with actionable suggestions.")
    .Build();

var analysis = await GetCompletionAsync(analysisPrompt);

// Step 3: Generate recommendations
var recommendationsPrompt = new PromptBuilder()
    .WithSystemMessage("You are a senior publishing agent.")
    .WithInstruction($"Based on this summary: {summary}\n\nAnd this analysis: {analysis}\n\nGenerate comprehensive recommendations.")
    .Build();

var recommendations = await GetCompletionAsync(recommendationsPrompt);
```

**When to use multi-step:**
- Tasks too complex for a single prompt
- Need intermediate results for decision-making
- Large documents that need chunking (RAG)
- Different expertise needed at each step

## Token Optimization

Optimizing prompts reduces costs and improves performance. Here are key strategies:

### 1. Be Concise but Complete

**Bad (too verbose):**
```
Please analyze the following code snippet that I'm about to provide. I want you to look at it carefully and provide a comprehensive review. The code is written in C# and I'm particularly interested in security issues, performance problems, and code style. Please be thorough.
```

**Good (concise):**
```
Review this C# code for security, performance, and style issues.
```

### 2. Use RAG for Large Documents

Instead of sending entire documents, use RAG to send only relevant chunks:

**Bad (sends entire document):**
```csharp
var prompt = $"Analyze this 100-page document: {entireDocument}";
// Uses ~25,000 tokens!
```

**Good (uses RAG):**
```csharp
// Find relevant chunks (5 chunks of 500 tokens each)
var relevantChunks = await ragService.FindSimilarDocumentsAsync(query, embeddings, topK: 5);
var prompt = $"Analyze these relevant sections: {string.Join("\n\n", relevantChunks)}";
// Uses only ~2,500 tokens (90% reduction!)
```

### 3. Remove Redundancy

**Bad:**
```
You are an expert code reviewer. You are an expert at reviewing code. Review code for bugs. Look for bugs in the code.
```

**Good:**
```
You are an expert code reviewer. Review the code for bugs.
```

### 4. Use Function Calling Instead of Long Instructions

**Bad (instructs AI to format JSON):**
```
Return the results in JSON format with this structure: {"items": [{"name": "...", "value": "..."}]}. Make sure it's valid JSON. Don't forget the quotes around keys.
```

**Good (uses function calling):**
```csharp
Functions = new List<FunctionDefinition> { functionDefinition },
FunctionCall = "auto"
// AI automatically returns structured data
```

## Common Mistakes to Avoid

1. **Too vague**: "Fix this code" → Be specific: "Review this code for security vulnerabilities and performance issues"
2. **No context**: Provide background information → "This is a payment processing function in a .NET API"
3. **Wrong temperature**: Use low temperature (0.2-0.3) for deterministic tasks like code review
4. **Too long prompts**: Keep prompts concise but complete → Remove unnecessary words
5. **No examples**: Provide examples for complex tasks → Show desired format
6. **Ignoring system messages**: Always set a role/context → "You are an expert [role]"
7. **Sending entire large documents**: Use RAG for documents > context window
8. **Not using function calling**: Parse free-form text instead of structured output
9. **Inconsistent formatting**: Use PromptBuilder for consistency
10. **No error handling**: Plan for when AI doesn't return expected format

## Testing Prompts

Test prompts with diverse inputs to ensure they work across different scenarios:

**C# Example:**
```csharp
var testCases = new[]
{
    "Simple requirement: User can login",
    "Complex requirement with multiple stakeholders: Payment processing with fraud detection",
    "Requirement with technical constraints: Must support 1M concurrent users"
};

foreach (var testCase in testCases)
{
    var result = await TestPrompt(testCase);
    Console.WriteLine($"Input: {testCase}\nOutput: {result}\n");
    
    // Validate output format
    Assert.IsNotNull(result);
    Assert.IsTrue(result.Contains("As a"));
}
```

**REST API Testing (curl):**
```bash
# Test case 1: Simple requirement
curl https://api.openai.com/v1/chat/completions \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $OPENAI_API_KEY" \
  -d '{
    "model": "gpt-4-turbo-preview",
    "messages": [
      {"role": "system", "content": "You are an expert business analyst."},
      {"role": "user", "content": "Generate user stories from: User can login"}
    ],
    "temperature": 0.3
  }'

# Test case 2: Complex requirement
curl https://api.openai.com/v1/chat/completions \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $OPENAI_API_KEY" \
  -d '{
    "model": "gpt-4-turbo-preview",
    "messages": [
      {"role": "system", "content": "You are an expert business analyst."},
      {"role": "user", "content": "Generate user stories from: Payment processing with fraud detection for e-commerce platform"}
    ],
    "temperature": 0.3
  }'
```

**What to test:**
- ✅ Simple inputs (happy path)
- ✅ Complex inputs (edge cases)
- ✅ Empty/null inputs (error handling)
- ✅ Very long inputs (token limits)
- ✅ Different formats (structured vs. unstructured)
- ✅ Output format consistency
- ✅ Token usage (cost tracking)

## Real-World Examples from the Codebase

Here are production-ready prompt examples from actual projects:

### Code Review (CodeReviewAssistant)

```csharp
var prompt = new PromptBuilder()
    .WithSystemMessage("You are an expert code reviewer. Review code for security, performance, style, and bugs.")
    .WithInstruction("Review the following code and provide detailed feedback. Focus on security vulnerabilities, performance issues, code style, and potential bugs.")
    .WithInput(code)
    .Build();
// Temperature: 0.2, MaxTokens: 3000
```

### User Story Generation (RequirementsAssistant)

```csharp
var prompt = new PromptBuilder()
    .WithSystemMessage("You are an expert business analyst. Generate user stories in the standard format: As a [user], I want [goal], so that [benefit].")
    .WithInstruction("Analyze the following requirements document and generate user stories with acceptance criteria. Extract key requirements and convert them into well-structured user stories.")
    .WithInput(documentContent)
    .Build();
// Temperature: 0.3, MaxTokens: 2000, Uses Function Calling
```

### Action Item Extraction (RetroAnalyzer)

```csharp
var prompt = new PromptBuilder()
    .WithSystemMessage("You are an expert Scrum Master. Extract actionable items from retrospective comments.")
    .WithInstruction("Analyze the following retrospective comments and extract concrete action items. Identify owners when mentioned.")
    .WithInput(commentsText)
    .Build();
// Temperature: 0.3, MaxTokens: 2000, Uses Function Calling
```

### RAG-Based Analysis (PublishingAssistant)

```csharp
var prompt = new PromptBuilder()
    .WithSystemMessage("You are a senior publishing agent providing detailed editorial feedback.")
    .WithContext(relevantChunks) // From RAG search
    .WithInstruction("Analyze the manuscript sections and identify specific issues with actionable suggestions. Be specific about locations and provide concrete improvement recommendations.")
    .Build();
// Temperature: 0.3, MaxTokens: 3000, Uses RAG for token optimization
```

## Troubleshooting Common Issues

### Issue: AI doesn't follow the format

**Problem:** AI returns free-form text instead of structured format.

**Solution:** Use Function Calling or JSON Mode:
```csharp
// Instead of: "Return JSON format: {...}"
// Use:
Functions = new List<FunctionDefinition> { functionDefinition },
FunctionCall = "auto"
// Or:
ResponseFormat = new { type = "json_object" }
```

### Issue: Inconsistent results

**Problem:** Same prompt gives different results each time.

**Solution:** Lower temperature and add more specificity:
```csharp
Temperature = 0.2, // Lower = more consistent
// Add more examples
.WithExample(input1, output1)
.WithExample(input2, output2)
```

### Issue: Missing important information

**Problem:** AI skips key details in summaries or analysis.

**Solution:** Be explicit about what to include:
```csharp
.WithInstruction("Include ALL of the following:\n" +
                "1. Key stakeholders\n" +
                "2. Functional requirements\n" +
                "3. Non-functional requirements\n" +
                "4. Constraints\n" +
                "Do not omit any section.")
```

### Issue: Too many tokens / High cost

**Problem:** Prompts use too many tokens, increasing costs.

**Solution:** Use RAG, remove redundancy, be concise:
```csharp
// Instead of sending entire document
var relevantChunks = await ragService.FindSimilarDocumentsAsync(query, embeddings, topK: 5);
// Send only relevant chunks (80-90% token reduction)
```

### Issue: AI hallucinates information

**Problem:** AI makes up facts or details not in the input.

**Solution:** Add explicit instructions and use lower temperature:
```csharp
.WithInstruction("Only use information provided in the document. Do not add information that is not explicitly stated. If information is missing, state 'Not provided in document.'")
Temperature = 0.2 // Lower temperature reduces hallucinations
```

## What You've Learned

After reading this guide, you should understand:
- ✅ How to structure prompts for clarity and specificity
- ✅ When and how to use examples (few-shot learning)
- ✅ How to use the PromptBuilder class effectively
- ✅ Common prompt patterns for different use cases
- ✅ How to optimize prompts for token usage and cost
- ✅ How to use Function Calling and JSON Mode for structured outputs
- ✅ RAG prompt patterns for large documents
- ✅ How to iterate and improve your prompts
- ✅ How to troubleshoot when results aren't what you expect
- ✅ Real-world examples from production code

## Resources

- [OpenAI Prompt Engineering Guide](https://platform.openai.com/docs/guides/prompt-engineering)
- [Best Practices for Prompt Engineering](https://help.openai.com/en/articles/6654000-best-practices-for-prompt-engineering-with-openai-api)
- [Function Calling Guide](../advanced-features/json-mode.md) - Structured outputs
- [RAG Patterns](../advanced-features/rag-patterns.md) - Document Q&A patterns
- [Cost Optimization](../best-practices/cost-optimization.md) - Reduce token usage

## Next Steps

- **Practice:** Try writing prompts for your specific use case using both C# and REST APIs
- **Learn more:** Read [API Basics](02-api-basics.md) to understand models and parameters
- **Build:** Follow [First Project](04-first-project.md) to apply what you've learned
- **Explore:** Check out [Role-Specific Guides](../role-guides/) for role-specific prompt examples
- **Study:** Review actual prompt implementations in `src/` projects
- **Optimize:** Learn [Cost Optimization](../best-practices/cost-optimization.md) techniques
- **Production:** Review [Best Practices](../best-practices/) before building production applications
