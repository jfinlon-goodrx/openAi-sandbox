# API Basics

This guide covers the fundamentals of using the OpenAI Platform APIs in .NET.

## Understanding Models

OpenAI provides several models, each optimized for different tasks:

- **GPT-4 Turbo** (`gpt-4-turbo-preview`): Best for complex reasoning, code generation, and analysis
- **GPT-4** (`gpt-4`): High-quality responses, good for complex tasks
- **GPT-3.5 Turbo** (`gpt-3.5-turbo`): Fast and cost-effective for simpler tasks
- **Whisper** (`whisper-1`): Speech-to-text transcription
- **Embeddings** (`text-embedding-ada-002`): Text embeddings for semantic search

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

Tokens are the units of text that OpenAI models process. Roughly:
- 1 token ≈ 4 characters
- 1 token ≈ 0.75 words

### Token Counting

Use the `TokenCounter` utility:

```csharp
using OpenAIShared;

var text = "Hello, this is a test.";
var tokenCount = TokenCounter.EstimateTokenCount(text);
Console.WriteLine($"Estimated tokens: {tokenCount}");
```

### Cost Estimation

Estimate costs using `CostCalculator`:

```csharp
using OpenAIShared;

var cost = CostCalculator.CalculateCost(
    model: "gpt-4-turbo-preview",
    inputTokens: 1000,
    outputTokens: 500
);

Console.WriteLine($"Estimated cost: {CostCalculator.FormatCost(cost)}");
```

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

## Error Handling

Always handle errors gracefully:

```csharp
try
{
    var response = await openAIClient.GetChatCompletionAsync(request);
    // Process response
}
catch (HttpRequestException ex)
{
    // Handle network/API errors
    Console.WriteLine($"API Error: {ex.Message}");
}
catch (Exception ex)
{
    // Handle other errors
    Console.WriteLine($"Error: {ex.Message}");
}
```

## Streaming Responses

For long responses, use streaming:

```csharp
await foreach (var chunk in openAIClient.GetChatCompletionStreamAsync(request))
{
    Console.Write(chunk);
    // Process chunk as it arrives
}
```

## Best Practices

1. **Set appropriate temperature** based on your use case
2. **Limit max tokens** to control costs
3. **Handle errors** gracefully with try-catch
4. **Monitor token usage** to track costs
5. **Use retry logic** (already built into OpenAIClient)
6. **Cache responses** when appropriate to reduce API calls

## Next Steps

- Learn about [Prompt Engineering](03-prompt-engineering.md)
- Build your [First Project](04-first-project.md)
- Check out [Role-Specific Guides](../role-guides/) for your workflow
