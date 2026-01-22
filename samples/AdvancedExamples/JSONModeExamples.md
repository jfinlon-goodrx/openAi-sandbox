# JSON Mode Examples

Examples demonstrating structured output using JSON mode.

## Structured Output with JSON Mode

```csharp
var request = new ChatCompletionRequest
{
    Model = "gpt-4-turbo-preview",
    Messages = new List<ChatMessage>
    {
        new() 
        { 
            Role = "user", 
            Content = "List 5 programming languages with their primary use cases. Return as JSON array." 
        }
    },
    ResponseFormat = new { type = "json_object" }, // JSON mode
    Temperature = 0.3
};

var response = await openAIClient.GetChatCompletionAsync(request);
var jsonContent = response.Choices.First().Message.Content;

// Parse structured JSON
var languages = JsonSerializer.Deserialize<List<Language>>(jsonContent);
```

## Pharmacy: Structured Drug Information

```csharp
var request = new ChatCompletionRequest
{
    Model = "gpt-4-turbo-preview",
    Messages = new List<ChatMessage>
    {
        new() 
        { 
            Role = "user", 
            Content = "Provide information about Metformin in JSON format with fields: name, uses, dosage, sideEffects (array), interactions (array)" 
        }
    },
    ResponseFormat = new { type = "json_object" },
    Temperature = 0.2
};
```

## Publishing: Structured Book Metadata

```csharp
var request = new ChatCompletionRequest
{
    Model = "gpt-4-turbo-preview",
    Messages = new List<ChatMessage>
    {
        new() 
        { 
            Role = "user", 
            Content = "Extract book metadata from this manuscript. Return JSON with: title, author, genre, estimatedWordCount, themes (array), targetAudience" 
        }
    },
    ResponseFormat = new { type = "json_object" }
};
```
