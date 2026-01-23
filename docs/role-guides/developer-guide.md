# Developer Guide

**For:** Software developers who want to integrate OpenAI Platform capabilities into their development workflow.

**What you'll learn:** How to use AI for code review, test generation, documentation, and improving code quality.

This guide helps developers leverage OpenAI Platform capabilities in their daily workflow.

## Use Cases for Developers

### 1. Code Review Assistant

Automatically review code for:
- Security vulnerabilities
- Performance issues
- Code style violations
- Potential bugs

**Example:**
```csharp
var reviewService = new CodeReviewService(openAIClient, logger);
var result = await reviewService.ReviewCodeAsync(code, "C#");

foreach (var comment in result.Comments)
{
    Console.WriteLine($"[{comment.Severity}] Line {comment.Line}: {comment.Message}");
}
```

### 2. Test Case Generation

Generate unit tests from code:

```csharp
var testService = new TestCaseService(openAIClient, logger);
var testCases = await testService.GenerateUnitTestsAsync(code, "xUnit");

foreach (var testCase in testCases)
{
    Console.WriteLine($"Test: {testCase.Name}");
    Console.WriteLine($"Steps: {string.Join(", ", testCase.Steps)}");
}
```

### 3. Code Explanation

Understand complex code:

```csharp
var explanation = await reviewService.ExplainCodeAsync(complexCode);
Console.WriteLine(explanation);
```

### 4. Documentation Generation

Generate API documentation:

```csharp
var docService = new DocumentationService(openAIClient, logger);
var apiDocs = await docService.GenerateApiDocumentationAsync(code);
```

### 5. PR Summaries

Generate pull request summaries:

```csharp
var summary = await reviewService.GeneratePRSummaryAsync(codeChanges);
```

## Integration with GitHub Actions

Set up automated code reviews in your CI/CD pipeline:

1. Add the workflow file (already included):
   `.github/workflows/code-review.yml`

2. Add secrets to your GitHub repository:
   - `OPENAI_API_KEY`: Your OpenAI API key
   - `SLACK_WEBHOOK_URL`: (Optional) For Slack notifications

3. The workflow will automatically review pull requests

### GitHub Actions + Slack Notifications

Get notified in Slack when code reviews complete:

```yaml
- name: Notify Slack
  if: always()
  env:
    SLACK_WEBHOOK_URL: ${{ secrets.SLACK_WEBHOOK_URL }}
  run: |
    curl -X POST $SLACK_WEBHOOK_URL \
      -H 'Content-Type: application/json' \
      -d '{
        "text": "🔍 Code Review Completed",
        "blocks": [{
          "type": "section",
          "text": {
            "type": "mrkdwn",
            "text": "*PR Review Completed*\n<${{ github.event.pull_request.html_url }}|PR #${{ github.event.pull_request.number }}>"
          }
        }]
      }'
```

See [Slack Integration Guide](../integrations/slack-integration.md) and [GitHub Actions Workflows](../../samples/GitHubExamples/GitHubActionsWorkflows.md) for more examples.

## Daily Workflow Examples

### Morning: Review Overnight PRs

```bash
# Use Code Review Assistant to review PRs
dotnet run --project CodeReviewAssistant.GitHub -- \
  --repo owner/repo \
  --pr 123
```

### During Development: Generate Tests

```bash
# Generate tests for new code
dotnet run --project TestCaseGenerator.Cli -- \
  from-code MyClass.cs
```

### Before Committing: Generate Documentation

```csharp
var readme = await docService.GenerateReadmeAsync(code, "MyProject");
await File.WriteAllTextAsync("README.md", readme);
```

## Best Practices

1. **Review AI Suggestions**: Always review AI-generated code and suggestions
2. **Use Appropriate Models**: Use GPT-4 for code, GPT-3.5 for simpler tasks
3. **Set Low Temperature**: Use 0.2-0.3 for code generation
4. **Test Generated Code**: Always test AI-generated code thoroughly
5. **Monitor Costs**: Track token usage, especially in CI/CD

## Common Patterns

### Code Review Pattern

```csharp
public async Task<CodeReviewResult> ReviewPullRequest(string prNumber)
{
    var code = await GetPullRequestCode(prNumber);
    var review = await _codeReviewService.ReviewCodeAsync(code);
    
    // Post comments to PR
    await PostReviewComments(prNumber, review.Comments);
    
    return review;
}
```

### Test Generation Pattern

```csharp
public async Task GenerateTestsForFile(string filePath)
{
    var code = await File.ReadAllTextAsync(filePath);
    var tests = await _testService.GenerateUnitTestsAsync(code);
    
    var testFile = GenerateTestFile(tests);
    await File.WriteAllTextAsync(GetTestFilePath(filePath), testFile);
}
```

## Troubleshooting

**Issue:** Code review misses obvious bugs
- **Solution:** Provide more context in prompts, use GPT-4 instead of GPT-3.5

**Issue:** Generated tests don't compile
- **Solution:** Review and fix generated code. AI is a helper, not a replacement for human review

**Issue:** High API costs
- **Solution:** Use GPT-3.5 for simpler tasks, cache results when possible

## Slack Integration

Send code review summaries and test generation results to Slack:

```csharp
var slackIntegration = new SlackIntegration(httpClient, logger, slackWebhookUrl);

// After code review
await slackIntegration.SendPrNotificationAsync(
    repository: "my-org/my-repo",
    prNumber: 123,
    title: "Add new feature",
    author: "john.doe",
    status: "Open",
    prUrl: "https://github.com/my-org/my-repo/pull/123",
    channel: "#code-review"
);
```

See [Slack Integration Guide](../integrations/slack-integration.md) for more examples.

## Resources

- [Code Review Assistant Documentation](../project-docs/code-review-assistant.md)
- [Test Case Generator Documentation](../project-docs/test-case-generator.md)
- [Documentation Generator Documentation](../project-docs/documentation-generator.md)
- [Slack Integration Guide](../integrations/slack-integration.md) ⭐ NEW
- [GitHub Actions Workflows](../../samples/GitHubExamples/GitHubActionsWorkflows.md) ⭐ NEW
- [Best Practices](../best-practices/)
