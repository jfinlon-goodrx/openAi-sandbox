# Your First Project

**For:** Developers - Build your first AI-powered application step-by-step

**What you'll build:** A simple console application that uses OpenAI to summarize text. This demonstrates core concepts and gives you a working example to build upon.

**Time required:** 15-30 minutes

**Prerequisites:**
- .NET 8.0 SDK installed (see [Setup Guide](01-setup.md))
- OpenAI API key configured (see [Setup Guide](01-setup.md))
- Basic familiarity with command line/terminal

**What you'll learn:**
- How to set up a .NET project
- How to use the OpenAI client library
- How to make API calls and handle responses
- How to structure a simple AI application

## Project Setup

1. Create a new console application:

```bash
dotnet new console -n HelloAI -o samples/HelloAI
cd samples/HelloAI
```

2. Add required packages:

```bash
dotnet add package Microsoft.Extensions.Configuration
dotnet add package Microsoft.Extensions.Configuration.Json
dotnet add package Microsoft.Extensions.DependencyInjection
dotnet add package Microsoft.Extensions.Logging
dotnet add package Microsoft.Extensions.Logging.Console
```

3. Add reference to OpenAIShared:

```bash
dotnet add reference ../../shared/OpenAIShared/OpenAIShared.csproj
```

## Implementation

Create `Program.cs`:

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAIShared;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());
services.AddOpenAIServices(configuration);

var serviceProvider = services.BuildServiceProvider();
var openAIClient = serviceProvider.GetRequiredService<OpenAIClient>();

Console.WriteLine("Hello AI - Text Summarizer");
Console.WriteLine("==========================\n");

Console.Write("Enter text to summarize: ");
var input = Console.ReadLine();

if (string.IsNullOrWhiteSpace(input))
{
    Console.WriteLine("No input provided.");
    return;
}

try
{
    var request = new ChatCompletionRequest
    {
        Model = "gpt-4-turbo-preview",
        Messages = new List<ChatMessage>
        {
            new() 
            { 
                Role = "system", 
                Content = "You are an expert at summarizing text. Provide concise, clear summaries." 
            },
            new() 
            { 
                Role = "user", 
                Content = $"Summarize the following text in 2-3 sentences:\n\n{input}" 
            }
        },
        Temperature = 0.3,
        MaxTokens = 150
    };

    Console.WriteLine("\nGenerating summary...\n");
    
    var response = await openAIClient.GetChatCompletionAsync(request);
    var summary = response.Choices.First().Message.Content;
    
    Console.WriteLine("Summary:");
    Console.WriteLine("--------");
    Console.WriteLine(summary);
    
    // Show token usage
    if (response.Usage != null)
    {
        var cost = CostCalculator.CalculateCost(
            request.Model,
            response.Usage.PromptTokens,
            response.Usage.CompletionTokens
        );
        
        Console.WriteLine($"\nTokens used: {response.Usage.TotalTokens}");
        Console.WriteLine($"Estimated cost: {CostCalculator.FormatCost(cost)}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

Create `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "OpenAI": {
    "ApiKey": "",
    "BaseUrl": "https://api.openai.com/v1",
    "DefaultModel": "gpt-4-turbo-preview",
    "MaxRetries": 3,
    "TimeoutSeconds": 60,
    "EnableLogging": true
  }
}
```

Create `.gitignore`:

```
bin/
obj/
appsettings.json
```

## Running the Project

1. Set your API key:

```bash
export OpenAI__ApiKey="your-api-key-here"
```

2. Run the application:

```bash
dotnet run
```

3. Enter some text when prompted and see the summary!

## Enhancing the Project

### Add File Input

```csharp
Console.Write("Enter file path (or press Enter to type): ");
var filePath = Console.ReadLine();

string text;
if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
{
    text = await File.ReadAllTextAsync(filePath);
}
else
{
    Console.Write("Enter text: ");
    text = Console.ReadLine() ?? string.Empty;
}
```

### Add Streaming

```csharp
Console.WriteLine("\nSummary (streaming):\n");

await foreach (var chunk in openAIClient.GetChatCompletionStreamAsync(request))
{
    Console.Write(chunk);
}
```

### Add Multiple Summaries

```csharp
var lengths = new[] { "one sentence", "2-3 sentences", "a paragraph" };

foreach (var length in lengths)
{
    request.Messages[1].Content = $"Summarize in {length}:\n\n{input}";
    var response = await openAIClient.GetChatCompletionAsync(request);
    Console.WriteLine($"\n{length.ToUpperInvariant()}:");
    Console.WriteLine(response.Choices.First().Message.Content);
}
```

## What You've Learned

After completing this project, you now understand:
- ✅ How to set up a .NET project with OpenAI integration
- ✅ How to configure and use the OpenAI client library
- ✅ How to structure prompts for AI interactions
- ✅ How to handle API responses and errors
- ✅ How to track token usage and estimate costs
- ✅ Basic patterns for building AI-powered applications

## Next Steps

Now that you've built your first project:

1. **Explore more:** Check out the [sample projects](../samples/) for more examples
2. **Find your role:** Review [Role-Specific Guides](../role-guides/) for workflows specific to your role
3. **Learn best practices:** Read [Best Practices](../best-practices/) before building production applications
4. **Dive deeper:** Review the [Project Documentation](../project-docs/) for detailed examples and advanced features
5. **Build something:** Apply what you've learned to solve a real problem in your work

## Common Issues

**Issue:** "API key not found"
- Ensure you've set the `OpenAI__ApiKey` environment variable

**Issue:** "Rate limit exceeded"
- You may be hitting OpenAI rate limits. Wait a moment and try again

**Issue:** "Timeout"
- Check your internet connection. The default timeout is 60 seconds

## Congratulations!

You've built your first AI-powered application! You now understand:
- How to set up OpenAI API calls
- How to structure prompts
- How to handle responses
- How to track token usage and costs

Continue exploring the other projects in this repository to learn more advanced techniques.
