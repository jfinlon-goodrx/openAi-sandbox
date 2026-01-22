# Documentation Generator

## Overview

The Documentation Generator helps Developers automatically generate API documentation, README files, and changelogs from code and comments.

## Features

- **API Documentation**: Generate comprehensive API docs from code
- **README Generation**: Create README files with installation and usage instructions
- **Changelog Generation**: Generate changelogs from commit messages
- **Architecture Diagrams**: Generate text descriptions of architecture

## Architecture

```
┌─────────────────┐
│ Documentation   │
│ Service         │
└────────┬────────┘
         │
┌────────▼────────┐
│  OpenAI Client  │
└─────────────────┘
```

## Setup

1. Configure OpenAI API key (see [Setup Guide](../getting-started/01-setup.md))

2. Use the service:
```csharp
var docService = new DocumentationService(openAIClient, logger);
```

## Usage

### Generate API Documentation

```csharp
var code = @"
public class UserService
{
    /// <summary>
    /// Gets a user by ID
    /// </summary>
    public User GetUser(int id) { }
}
";

var apiDocs = await docService.GenerateApiDocumentationAsync(code);
Console.WriteLine(apiDocs);
```

### Generate README

```csharp
var code = @"
// Project: MyAwesomeLibrary
// Description: A library for doing awesome things
public class AwesomeClass { }
";

var readme = await docService.GenerateReadmeAsync(code, "MyAwesomeLibrary");
await File.WriteAllTextAsync("README.md", readme);
```

### Generate Changelog

```csharp
var commitMessages = new List<string>
{
    "Add user authentication",
    "Fix bug in payment processing",
    "Update dependencies"
};

var changelog = await docService.GenerateChangelogAsync(commitMessages);
Console.WriteLine(changelog);
```

## Output Examples

### API Documentation

```
# UserService API

## GetUser

Gets a user by ID.

**Parameters:**
- `id` (int): The user ID

**Returns:**
- `User`: The user object

**Example:**
```csharp
var user = userService.GetUser(123);
```
```

### README

```
# MyAwesomeLibrary

A library for doing awesome things.

## Installation

```bash
dotnet add package MyAwesomeLibrary
```

## Usage

```csharp
using MyAwesomeLibrary;

var awesome = new AwesomeClass();
```

## API Reference

See [API Documentation](docs/api.md)
```

### Changelog

```
# Changelog

## [Unreleased]

### Added
- User authentication

### Fixed
- Bug in payment processing

### Changed
- Updated dependencies
```

## Best Practices

1. **Include Comments**: Add XML comments to code for better documentation
2. **Review Output**: Always review and refine generated documentation
3. **Keep Updated**: Regenerate documentation when code changes
4. **Use Templates**: Create templates for consistent formatting

## Integration with Build Process

### MSBuild Target

Add to your `.csproj`:

```xml
<Target Name="GenerateDocs" AfterTargets="Build">
  <Exec Command="dotnet run --project DocumentationGenerator.Core -- generate-api --input $(OutputPath)" />
</Target>
```

### GitHub Actions

```yaml
- name: Generate Documentation
  run: |
    dotnet run --project DocumentationGenerator.Core -- generate-readme
    dotnet run --project DocumentationGenerator.Core -- generate-changelog
```

## Troubleshooting

**Issue:** Documentation lacks detail
- **Solution:** Add more comments to code, provide context

**Issue:** Formatting issues
- **Solution:** Review and adjust prompts, use markdown templates

**Issue:** Missing information
- **Solution:** Include more context in code comments

## Slack Integration

Notify your team when documentation is generated:

```csharp
var slackIntegration = new SlackIntegration(httpClient, logger, slackWebhookUrl);

// After generating documentation
await slackIntegration.SendMessageAsync(
    text: $"📚 Documentation generated for {projectName}\n\n{apiDocs.Substring(0, Math.Min(200, apiDocs.Length))}...",
    channel: "#documentation"
);
```

See [Slack Integration Guide](../integrations/slack-integration.md) for more examples.

## Related Documentation

- [Developer Guide](../role-guides/developer-guide.md)
- [Slack Integration Guide](../integrations/slack-integration.md) ⭐ NEW
- [Getting Started](../getting-started/)
