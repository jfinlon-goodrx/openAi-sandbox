# API Basics

**For:** Everyone - Understanding how to interact with OpenAI Platform APIs

This guide explains the fundamental concepts you need to understand when working with OpenAI Platform APIs. Even if you're not writing code, understanding these concepts will help you use the REST APIs effectively and understand how the projects work.

## What You'll Learn

- What AI models are and how they work
- Understanding tokens and how they affect costs
- How to make API calls (with code examples and REST API examples)
- How to choose the right model for your task
- Best practices for using APIs effectively

## Understanding Models

**What is a model?** An AI model is a trained system that can perform specific tasks. Think of it as a specialized tool - you wouldn't use a hammer to screw in a bolt, and you wouldn't use a speech-to-text model to generate images.

**How do models work?** Models are trained on vast amounts of data to learn patterns. When you send them input (like text or an image), they use what they've learned to generate appropriate responses.

### Available Models

OpenAI provides several models, each optimized for different tasks:

#### Text Generation Models

- **GPT-4 Turbo** (`gpt-4-turbo-preview`): 
  - **Best for:** Complex reasoning, code generation, analysis, and high-quality content
  - **When to use:** When you need the best quality and can afford slightly higher costs
  - **Example use cases:** Code review, requirements analysis, complex Q&A

- **GPT-4** (`gpt-4`): 
  - **Best for:** High-quality responses for complex tasks
  - **When to use:** Similar to GPT-4 Turbo but slightly older version
  - **Example use cases:** Document analysis, strategic planning

- **GPT-3.5 Turbo** (`gpt-3.5-turbo`): 
  - **Best for:** Fast and cost-effective responses for simpler tasks
  - **When to use:** When speed and cost matter more than maximum quality
  - **Example use cases:** Simple summaries, basic Q&A, content generation

#### Specialized Models

- **Whisper** (`whisper-1`): 
  - **Purpose:** Speech-to-text transcription
  - **When to use:** Converting audio recordings (meetings, interviews) to text
  - **Example use cases:** Meeting transcriptions, voice note processing

- **Embeddings** (`text-embedding-ada-002`): 
  - **Purpose:** Creates mathematical representations of text for semantic search
  - **When to use:** Finding similar documents, building search systems, RAG workflows
  - **Example use cases:** Document similarity search, recommendation systems

**How to choose?** See the [Model Selection Guide](#model-selection-guide) below.

## Making Your First API Call

### Using the Shared Client

The `OpenAIShared` library provides a wrapper around OpenAI APIs:

```csharp
using OpenAIShared;
using Microsoft.Extensions.DependencyInjection;

// Register services
var services = new ServiceCollection();
services.AddOpenAIServices(configuration);
var serviceProvider = services.BuildServiceProvider();

// Get the client
var openAIClient = serviceProvider.GetRequiredService<OpenAIClient>();

// Make a simple chat completion request
var request = new ChatCompletionRequest
{
    Model = "gpt-4-turbo-preview",
    Messages = new List<ChatMessage>
    {
        new() { Role = "user", Content = "Hello, how are you?" }
    },
    Temperature = 0.7,
    MaxTokens = 100
};

var response = await openAIClient.GetChatCompletionAsync(request);
var answer = response.Choices.First().Message.Content;
Console.WriteLine(answer);
```

## Understanding Tokens

**What are tokens?** Tokens are the units of text that AI models process. Think of them as "chunks" of text that the model understands.

**Why do tokens matter?** 
- **Cost:** You're charged based on token usage (both input and output)
- **Limits:** Models have maximum token limits per request
- **Performance:** More tokens = longer processing time

### Token Conversion Guide

**Rough estimates:**
- 1 token ≈ 4 characters
- 1 token ≈ 0.75 words
- 100 tokens ≈ 75 words ≈ 400 characters

**Examples:**
- "Hello, world!" = ~3 tokens
- This paragraph = ~50 tokens
- A typical email = ~100-200 tokens
- A page of text = ~500-750 tokens

### Token Counting

**In C# code:**
```csharp
using OpenAIShared;

var text = "Hello, this is a test.";
var tokenCount = TokenCounter.EstimateTokenCount(text);
Console.WriteLine($"Estimated tokens: {tokenCount}");
```

**Understanding the count:**
- Input tokens: Text you send to the model
- Output tokens: Text the model generates in response
- Total tokens: Input + Output (this is what you're charged for)

### Cost Estimation

**Why estimate costs?** To budget and avoid unexpected charges, especially when processing large volumes.

**In C# code:**
```csharp
using OpenAIShared;

var cost = CostCalculator.CalculateCost(
    model: "gpt-4-turbo-preview",
    inputTokens: 1000,   // Tokens in your request
    outputTokens: 500    // Tokens in the response
);

Console.WriteLine($"Estimated cost: ${CostCalculator.FormatCost(cost)}");
```

**Cost per model (approximate, check OpenAI pricing for current rates):**
- GPT-4 Turbo: ~$0.01 per 1K input tokens, ~$0.03 per 1K output tokens
- GPT-3.5 Turbo: ~$0.0005 per 1K input tokens, ~$0.0015 per 1K output tokens
- Embeddings: ~$0.0001 per 1K tokens

**Real-world example:**
- Processing a 10-page document (5,000 tokens) with GPT-4 Turbo
- Input: 5,000 tokens = $0.05
- Output: 1,000 tokens = $0.03
- **Total: $0.08 per document**

**Cost optimization tips:**
- Use GPT-3.5 Turbo for simple tasks (10x cheaper)
- Limit output tokens with `MaxTokens` parameter
- Cache responses when possible (see [Caching](../improvements/README.md#caching))
- Use batch processing for high volume (50% cost reduction)

## Temperature and Other Parameters

### Temperature

Controls randomness (0.0 to 2.0):
- **0.0-0.3**: Deterministic, focused responses (good for code, facts)
- **0.7-1.0**: Balanced creativity and coherence (good for general tasks)
- **1.5-2.0**: Very creative, less predictable (good for creative writing)

```csharp
var request = new ChatCompletionRequest
{
    Model = "gpt-4-turbo-preview",
    Messages = messages,
    Temperature = 0.3, // Lower for more deterministic output
    MaxTokens = 1000
};
```

### Max Tokens

Limits the length of the response. Set based on your needs:
- Short responses: 100-500 tokens
- Medium responses: 500-2000 tokens
- Long responses: 2000+ tokens

## Model Selection Guide

**How do I choose the right model?** Consider these factors:

### Decision Tree

1. **What type of task?**
   - **Text generation/analysis** → GPT-4 Turbo or GPT-3.5 Turbo
   - **Speech-to-text** → Whisper
   - **Semantic search/similarity** → Embeddings

2. **How complex is the task?**
   - **Simple** (summaries, basic Q&A) → GPT-3.5 Turbo (cheaper, faster)
   - **Complex** (code review, analysis, reasoning) → GPT-4 Turbo (better quality)

3. **What's your priority?**
   - **Cost** → GPT-3.5 Turbo
   - **Quality** → GPT-4 Turbo
   - **Speed** → GPT-3.5 Turbo

### Common Use Cases

| Use Case | Recommended Model | Why |
|----------|------------------|-----|
| Code review | GPT-4 Turbo | Needs complex reasoning |
| Requirements analysis | GPT-4 Turbo | Complex understanding required |
| Simple summaries | GPT-3.5 Turbo | Cost-effective, fast |
| User stories | GPT-4 Turbo | Structured output, quality matters |
| Meeting transcription | Whisper | Specialized for audio |
| Document search | Embeddings | Designed for similarity search |
| Test case generation | GPT-4 Turbo | Needs to understand code context |

## Error Handling

**Why handle errors?** APIs can fail for many reasons (network issues, rate limits, invalid requests). Proper error handling ensures your application doesn't crash and provides helpful feedback.

### Common Error Types

1. **Rate Limit Errors (429)**
   - **Cause:** Too many requests too quickly
   - **Solution:** Wait and retry, or upgrade your plan
   - **Prevention:** Implement rate limiting in your code

2. **Authentication Errors (401)**
   - **Cause:** Invalid or missing API key
   - **Solution:** Check your API key configuration
   - **Prevention:** Verify keys before making requests

3. **Invalid Request (400)**
   - **Cause:** Malformed request (e.g., too many tokens, invalid parameters)
   - **Solution:** Check request format and limits
   - **Prevention:** Validate input before sending

4. **Server Errors (500)**
   - **Cause:** OpenAI's servers are having issues
   - **Solution:** Retry with exponential backoff
   - **Prevention:** Implement retry logic (already in OpenAIClient)

### Error Handling in Code

```csharp
try
{
    var response = await openAIClient.GetChatCompletionAsync(request);
    // Process response
}
catch (HttpRequestException ex)
{
    // Handle network/API errors
    if (ex.Message.Contains("429"))
    {
        Console.WriteLine("Rate limit exceeded. Please wait and try again.");
    }
    else
    {
        Console.WriteLine($"API Error: {ex.Message}");
    }
}
catch (Exception ex)
{
    // Handle other errors
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

**Note:** The `OpenAIClient` in this project already includes retry logic, so temporary failures are automatically retried.

## Streaming Responses

**What is streaming?** Receiving responses in real-time as they're generated, rather than waiting for the complete response.

**Why use streaming?**
- **Better user experience:** Users see progress immediately
- **Perceived performance:** Feels faster even if total time is similar
- **Large responses:** Don't have to wait for everything before showing anything

**When to use:**
- Long responses (summaries, reports, documentation)
- Interactive applications (chat interfaces)
- Real-time updates (status reports, progress)

**In code:**
```csharp
await foreach (var chunk in openAIClient.GetChatCompletionStreamAsync(request))
{
    Console.Write(chunk);  // Display as it arrives
    // Process chunk as it arrives
}
```

**In REST API:** Use Server-Sent Events (SSE). See [Streaming Examples](../improvements/streaming-examples.md) for details.

## Best Practices

### 1. Set Appropriate Temperature
- **Lower (0.0-0.3)** for factual, consistent outputs
- **Higher (0.7-1.0)** for creative, varied outputs
- **Match to your use case** - don't use high temperature for code generation

### 2. Limit Max Tokens
- **Set based on actual needs** - don't over-allocate
- **Monitor costs** - longer responses cost more
- **Use streaming** for long responses to improve UX

### 3. Handle Errors Gracefully
- **Always use try-catch** blocks
- **Provide helpful error messages** to users
- **Log errors** for debugging (but don't log API keys!)

### 4. Monitor Token Usage
- **Track usage** to understand costs
- **Set budgets** to prevent unexpected charges
- **Use metrics** (see [Metrics Guide](../improvements/metrics-guide.md))

### 5. Use Retry Logic
- **Already built into OpenAIClient** - handles temporary failures automatically
- **For custom implementations:** Use exponential backoff (wait longer between retries)

### 6. Cache Responses
- **Cache identical requests** to reduce API calls
- **Use for:** Static content, repeated queries, expensive operations
- **See:** [Caching Service](../improvements/README.md#caching)

### 7. Optimize Prompts
- **Be specific** - clearer prompts = better results
- **Provide context** - helps the model understand your needs
- **Use examples** - few-shot learning improves results
- **See:** [Prompt Engineering Guide](03-prompt-engineering.md)

## Cost Management Tips

1. **Choose the right model** - GPT-3.5 Turbo is 10x cheaper for simple tasks
2. **Set MaxTokens appropriately** - Don't allocate more than you need
3. **Cache responses** - Don't regenerate the same content
4. **Use batch processing** - 50% cost reduction for high-volume operations
5. **Monitor usage** - Track costs to identify optimization opportunities
6. **Review regularly** - Check your OpenAI dashboard for usage patterns

## Next Steps

Now that you understand the basics:

1. **Learn Prompt Engineering:**
   - [Prompt Engineering Guide](03-prompt-engineering.md) - Writing effective prompts for better results

2. **Try It Yourself:**
   - [First Project](04-first-project.md) - Build your first AI-powered app
   - [REST API Examples](../../samples/REST-API-Examples/README.md) - Try APIs without coding

3. **Explore Role-Specific Workflows:**
   - [Role Guides](../role-guides/) - Find workflows for your role
   - [Project Documentation](../project-docs/) - See what each project does

4. **Learn Advanced Features:**
   - [Advanced Features](../advanced-features/) - Vision API, RAG, Batch Processing
   - [Best Practices](../best-practices/) - Security, cost optimization

---

**Need help?** Check the [Glossary](../GLOSSARY.md) for definitions of technical terms.
