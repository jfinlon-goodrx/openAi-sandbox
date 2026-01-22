# JSON Mode

## Overview

JSON Mode ensures the model responds with valid JSON, useful for structured data extraction.

## Usage

```csharp
var request = new ChatCompletionRequest
{
    Model = "gpt-4-turbo-preview",
    Messages = new List<ChatMessage>
    {
        new() 
        { 
            Role = "user", 
            Content = "List 5 cities with their populations. Return as JSON array." 
        }
    },
    ResponseFormat = new { type = "json_object" }, // Enable JSON mode
    Temperature = 0.3
};

var response = await openAIClient.GetChatCompletionAsync(request);
var json = response.Choices.First().Message.Content;

// Parse structured data
var cities = JsonSerializer.Deserialize<List<City>>(json);
```

## Use Cases

- Structured data extraction
- API response generation
- Data transformation
- Configuration generation

## Best Practices

1. Always specify the JSON structure in your prompt
2. Use lower temperature (0.2-0.3) for consistency
3. Validate JSON before parsing
4. Provide examples in prompts when possible
