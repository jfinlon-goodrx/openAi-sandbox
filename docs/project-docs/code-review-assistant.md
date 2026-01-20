# Intelligent Code Review Assistant

## Overview

The Code Review Assistant helps Developers automatically review code for security vulnerabilities, performance issues, code style violations, and potential bugs. It integrates with GitHub Actions for automated reviews in CI/CD pipelines.

## Features

- **Automated Code Review**: Review code for security, performance, and style
- **PR Summaries**: Generate pull request summaries
- **Code Explanation**: Explain complex code sections
- **GitHub Actions Integration**: Automated reviews in CI/CD
- **Multi-Language Support**: C#, JavaScript, TypeScript, Python, Java, Go, Rust

## Architecture

```
┌─────────────────┐
│ GitHub Actions   │
└────────┬────────┘
         │
┌────────▼────────┐
│ GitHub          │
│ Integration     │
└────────┬────────┘
         │
┌────────▼────────┐
│ Code Review     │
│ Service         │
└────────┬────────┘
         │
┌────────▼────────┐
│  OpenAI Client  │
└─────────────────┘
```

## Setup

### Local Usage

1. Configure OpenAI API key (see [Setup Guide](../getting-started/01-setup.md))

2. Use the service:
```csharp
var reviewService = new CodeReviewService(openAIClient, logger);

var code = @"
public void ProcessPayment(decimal amount)
{
    var result = amount * 1.1m;
    return result;
}
";

var review = await reviewService.ReviewCodeAsync(code, "C#");

Console.WriteLine($"Summary: {review.Summary}");
Console.WriteLine($"Security Issues: {review.SecurityIssues}");
Console.WriteLine($"Performance Issues: {review.PerformanceIssues}");

foreach (var comment in review.Comments)
{
    Console.WriteLine($"[{comment.Severity}] Line {comment.Line}: {comment.Message}");
    if (!string.IsNullOrEmpty(comment.Suggestion))
    {
        Console.WriteLine($"  Suggestion: {comment.Suggestion}");
    }
}
```

### GitHub Actions Integration

1. Add `OPENAI_API_KEY` to your GitHub repository secrets

2. The workflow file is already configured at:
   `.github/workflows/code-review.yml`

3. The workflow will automatically run on pull requests

## API Usage

### Review Code

```csharp
var review = await reviewService.ReviewCodeAsync(code, "C#");

// Review includes:
// - Summary
// - Comments (line-by-line feedback)
// - Security issues count
// - Performance issues count
// - Style issues count
```

### Generate PR Summary

```csharp
var codeChanges = @"
+ public void NewMethod() { }
- public void OldMethod() { }
";

var summary = await reviewService.GeneratePRSummaryAsync(codeChanges);
```

### Explain Code

```csharp
var complexCode = @"
public async Task ProcessAsync()
{
    await Task.Run(() => { /* complex logic */ });
}
";

var explanation = await reviewService.ExplainCodeAsync(complexCode);
```

## Review Categories

The assistant reviews code for:

1. **Security**: SQL injection, XSS, authentication issues, etc.
2. **Performance**: N+1 queries, inefficient algorithms, memory leaks
3. **Style**: Code style violations, naming conventions
4. **Bugs**: Potential runtime errors, null reference exceptions
5. **Suggestions**: Code improvements, best practices

## Severity Levels

- **Error**: Critical issues that must be fixed
- **Warning**: Issues that should be addressed
- **Info**: Suggestions and improvements

## GitHub Actions Workflow

The workflow automatically:

1. Checks out the code
2. Reviews changed files
3. Posts review comments to the PR
4. Provides a summary

### Customizing the Workflow

Edit `.github/workflows/code-review.yml`:

```yaml
- name: Run Code Review
  env:
    OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
  run: |
    dotnet run --project CodeReviewAssistant.GitHub -- \
      --repo ${{ github.repository }} \
      --pr ${{ github.event.pull_request.number }}
```

## Best Practices

1. **Review AI Suggestions**: Always review AI-generated comments
2. **Use Appropriate Models**: Use GPT-4 for code review (more accurate)
3. **Set Low Temperature**: Use 0.2-0.3 for deterministic reviews
4. **Focus on Critical Issues**: Prioritize security and performance
5. **Learn from Reviews**: Use feedback to improve coding practices

## Troubleshooting

**Issue:** Reviews miss obvious bugs
- **Solution:** Use GPT-4 instead of GPT-3.5, provide more context

**Issue:** Too many false positives
- **Solution:** Adjust prompts, focus on critical issues only

**Issue:** GitHub Actions fails
- **Solution:** Verify `OPENAI_API_KEY` secret is set correctly

**Issue:** Review takes too long
- **Solution:** Review only changed files, use GPT-3.5 for simpler reviews

## Related Documentation

- [Developer Guide](../role-guides/developer-guide.md)
- [Getting Started](../getting-started/)
- [Best Practices](../best-practices/)
