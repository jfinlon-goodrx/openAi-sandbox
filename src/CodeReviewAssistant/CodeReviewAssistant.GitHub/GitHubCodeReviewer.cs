using CodeReviewAssistant.Core;
using Octokit;

namespace CodeReviewAssistant.GitHub;

/// <summary>
/// GitHub Actions integration for automated code reviews
/// </summary>
public class GitHubCodeReviewer
{
    private readonly CodeReviewService _codeReviewService;
    private readonly GitHubClient _githubClient;

    public GitHubCodeReviewer(
        CodeReviewService codeReviewService,
        GitHubClient githubClient)
    {
        _codeReviewService = codeReviewService;
        _githubClient = githubClient;
    }

    /// <summary>
    /// Reviews a pull request and posts comments
    /// </summary>
    public async Task ReviewPullRequestAsync(
        string owner,
        string repo,
        int pullRequestNumber)
    {
        var pr = await _githubClient.PullRequest.Get(owner, repo, pullRequestNumber);
        var files = await _githubClient.PullRequest.Files(owner, repo, pullRequestNumber);

        foreach (var file in files)
        {
            if (file.Status == "removed")
                continue;

            var content = await _githubClient.Repository.Content.GetAllContentsByRef(
                owner, repo, file.FileName, pr.Head.Sha);

            var code = string.Join("\n", content.Select(c => c.Content));
            var reviewResult = await _codeReviewService.ReviewCodeAsync(code, GetLanguageFromFileName(file.FileName));

            // Post review comments
            var comments = reviewResult.Comments
                .Where(c => c.Severity == "error" || c.Severity == "warning")
                .Select(c => new DraftPullRequestReviewComment(
                    c.Message,
                    pr.Head.Sha,
                    file.FileName,
                    c.Line))
                .ToList();

            if (comments.Any())
            {
                var review = new PullRequestReviewCreate
                {
                    Event = PullRequestReviewEvent.Comment,
                    Body = reviewResult.Summary,
                    Comments = comments
                };

                await _githubClient.PullRequest.Review.Create(owner, repo, pullRequestNumber, review);
            }
        }
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
