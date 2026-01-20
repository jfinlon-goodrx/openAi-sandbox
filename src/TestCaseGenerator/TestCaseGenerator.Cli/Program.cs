using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAIShared;
using TestCaseGenerator.Core;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());
services.AddOpenAIServices(configuration);
services.AddScoped<TestCaseService>(sp =>
{
    var openAIClient = sp.GetRequiredService<OpenAIClient>();
    var logger = sp.GetRequiredService<ILogger<TestCaseService>>();
    var model = configuration["OpenAI:DefaultModel"] ?? "gpt-4-turbo-preview";
    return new TestCaseService(openAIClient, logger, model);
});

var serviceProvider = services.BuildServiceProvider();
var testCaseService = serviceProvider.GetRequiredService<TestCaseService>();

Console.WriteLine("Test Case Generator CLI");
Console.WriteLine("======================");
Console.WriteLine();

if (args.Length == 0)
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run -- <command> [options]");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  from-code <file>     Generate tests from code file");
    Console.WriteLine("  from-story <text>    Generate tests from user story");
    Console.WriteLine("  edge-cases <file>    Suggest edge cases for code");
    return;
}

try
{
    var command = args[0];

    switch (command)
    {
        case "from-code":
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Please provide a code file path");
                return;
            }
            await GenerateFromCode(testCaseService, args[1]);
            break;

        case "from-story":
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Please provide user story text");
                return;
            }
            await GenerateFromStory(testCaseService, args[1]);
            break;

        case "edge-cases":
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Please provide a code file path");
                return;
            }
            await SuggestEdgeCases(testCaseService, args[1]);
            break;

        default:
            Console.WriteLine($"Unknown command: {command}");
            break;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

async Task GenerateFromCode(TestCaseService service, string filePath)
{
    if (!File.Exists(filePath))
    {
        Console.WriteLine($"Error: File not found: {filePath}");
        return;
    }

    var code = await File.ReadAllTextAsync(filePath);
    Console.WriteLine("Generating test cases...");
    
    var testCases = await service.GenerateUnitTestsAsync(code);
    
    Console.WriteLine($"\nGenerated {testCases.Count} test cases:\n");
    foreach (var testCase in testCases)
    {
        Console.WriteLine($"- {testCase.Name}");
        Console.WriteLine($"  {testCase.Description}");
        Console.WriteLine($"  Steps: {string.Join(", ", testCase.Steps)}");
        Console.WriteLine();
    }
}

async Task GenerateFromStory(TestCaseService service, string story)
{
    Console.WriteLine("Generating test cases from user story...");
    
    var testCases = await service.GenerateTestCasesFromUserStoryAsync(story);
    
    Console.WriteLine($"\nGenerated {testCases.Count} test cases:\n");
    foreach (var testCase in testCases)
    {
        Console.WriteLine($"- {testCase.Name}");
        Console.WriteLine($"  {testCase.Description}");
        Console.WriteLine();
    }
}

async Task SuggestEdgeCases(TestCaseService service, string filePath)
{
    if (!File.Exists(filePath))
    {
        Console.WriteLine($"Error: File not found: {filePath}");
        return;
    }

    var code = await File.ReadAllTextAsync(filePath);
    Console.WriteLine("Analyzing code for edge cases...");
    
    var suggestions = await service.SuggestEdgeCasesAsync(code);
    
    Console.WriteLine("\nEdge Case Suggestions:");
    Console.WriteLine(suggestions);
}
