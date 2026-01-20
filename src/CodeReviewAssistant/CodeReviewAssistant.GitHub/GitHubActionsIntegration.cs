using CodeReviewAssistant.Core;
using Microsoft.Extensions.Logging;

namespace CodeReviewAssistant.GitHub;

/// <summary>
/// GitHub Actions integration for automated code reviews in CI/CD pipeline
/// </summary>
public class GitHubActionsIntegration
{
    private readonly CodeReviewService _codeReviewService;
    private readonly ILogger<GitHubActionsIntegration> _logger;

    public GitHubActionsIntegration(
        CodeReviewService codeReviewService,
        ILogger<GitHubActionsIntegration> logger)
    {
        _codeReviewService = codeReviewService;
        _logger = logger;
    }

    /// <summary>
    /// Reviews changed files in a pull request and posts review comments
    /// This is designed to be called from a GitHub Actions workflow
    /// </summary>
    public async Task ReviewPullRequestChangesAsync(
        string repository,
        int pullRequestNumber,
        string githubToken,
        List<string> changedFiles)
    {
        var githubClient = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("CodeReviewAssistant"));
        githubClient.Credentials = new Octokit.Credentials(githubToken);

        var parts = repository.Split('/');
        var owner = parts[0];
        var repo = parts[1];

        var pr = await githubClient.PullRequest.Get(owner, repo, pullRequestNumber);
        var files = await githubClient.PullRequest.Files(owner, repo, pullRequestNumber);

        var reviewComments = new List<Octokit.DraftPullRequestReviewComment>();
        var reviewResults = new List<CodeReviewResult>();

        foreach (var file in files.Where(f => changedFiles.Contains(f.FileName)))
        {
            try
            {
                var content = await githubClient.Repository.Content.GetAllContentsByRef(
                    owner, repo, file.FileName, pr.Head.Sha);

                var code = string.Join("\n", content.Select(c => c.Content));
                var reviewResult = await _codeReviewService.ReviewCodeAsync(
                    code, 
                    GetLanguageFromFileName(file.FileName));

                reviewResults.Add(reviewResult);

                // Only include errors and warnings in review comments
                var comments = reviewResult.Comments
                    .Where(c => c.Severity == "error" || c.Severity == "warning")
                    .Select(c => new Octokit.DraftPullRequestReviewComment(
                        $"[{c.Category.ToUpper()}] {c.Message}\n\n{c.Suggestion ?? ""}",
                        pr.Head.Sha,
                        file.FileName,
                        c.Line > 0 ? c.Line : 1))
                    .ToList();

                reviewComments.AddRange(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reviewing file {FileName}", file.FileName);
            }
        }

        if (reviewComments.Any())
        {
            var totalSecurity = reviewResults.Sum(r => r.SecurityIssues);
            var totalPerformance = reviewResults.Sum(r => r.PerformanceIssues);
            var totalStyle = reviewResults.Sum(r => r.StyleIssues);
            var summary = string.Join("\n\n", reviewResults.Select(r => r.Summary));

            var review = new Octokit.PullRequestReviewCreate
            {
                Event = Octokit.PullRequestReviewEvent.Comment,
                Body = $"## Code Review Summary\n\n{summary}\n\n" +
                       $"**Security Issues:** {totalSecurity}\n" +
                       $"**Performance Issues:** {totalPerformance}\n" +
                       $"**Style Issues:** {totalStyle}",
                Comments = reviewComments
            };

            await githubClient.PullRequest.Review.Create(owner, repo, pullRequestNumber, review);
        }
    }

    /// <summary>
    /// Generates a PR summary for GitHub Actions workflow
    /// </summary>
    public async Task<string> GeneratePRSummaryAsync(
        List<string> changedFiles,
        Dictionary<string, string> fileContents)
    {
        var changesText = string.Join("\n", changedFiles.Select(f => 
            $"## {f}\n```\n{fileContents.GetValueOrDefault(f, "")}\n```"));

        return await _codeReviewService.GeneratePRSummaryAsync(changesText);
    }

    private string? GetLanguageFromFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".cs" => "C#",
            ".js" => "JavaScript",
            ".ts" => "TypeScript",
            ".py" => "Python",
            ".java" => "Java",
            ".go" => "Go",
            ".rs" => "Rust",
            _ => null
        };
    }
}
