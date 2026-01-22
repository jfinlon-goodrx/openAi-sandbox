using Microsoft.Extensions.Logging;
using OpenAIShared;

namespace PublishingAssistant.Core;

/// <summary>
/// Uses Vision API to analyze existing cover images and provide feedback
/// </summary>
public class CoverImageAnalyzer
{
    private readonly VisionService _visionService;
    private readonly ILogger<CoverImageAnalyzer> _logger;

    public CoverImageAnalyzer(
        VisionService visionService,
        ILogger<CoverImageAnalyzer> logger)
    {
        _visionService = visionService;
        _logger = logger;
    }

    /// <summary>
    /// Analyzes an existing cover image and provides feedback
    /// </summary>
    public async Task<CoverImageAnalysis> AnalyzeCoverImageAsync(
        string imageUrl,
        string? genre = null,
        CancellationToken cancellationToken = default)
    {
        var genreContext = !string.IsNullOrEmpty(genre) ? $"Genre: {genre}. " : "";
        var prompt = $"Analyze this book cover image. {genreContext}" +
                    "Evaluate:\n" +
                    "1. Visual appeal and design quality\n" +
                    "2. Typography and readability\n" +
                    "3. Color scheme and mood\n" +
                    "4. Genre appropriateness\n" +
                    "5. Marketability\n" +
                    "6. Strengths and weaknesses\n" +
                    "7. Recommendations for improvement";

        var analysis = await _visionService.AnalyzeImageAsync(imageUrl, prompt, "high", cancellationToken);

        return new CoverImageAnalysis
        {
            ImageUrl = imageUrl,
            Analysis = analysis,
            AnalyzedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Compares multiple cover designs
    /// </summary>
    public async Task<CoverComparison> CompareCoversAsync(
        List<string> imageUrls,
        string? genre = null,
        CancellationToken cancellationToken = default)
    {
        var analyses = new List<CoverImageAnalysis>();

        foreach (var imageUrl in imageUrls)
        {
            var analysis = await AnalyzeCoverImageAsync(imageUrl, genre, cancellationToken);
            analyses.Add(analysis);
        }

        // Generate comparison summary
        var comparisonPrompt = $"Compare these {analyses.Count} book cover designs. " +
                              "Rank them by:\n" +
                              "1. Visual appeal\n" +
                              "2. Marketability\n" +
                              "3. Genre appropriateness\n" +
                              "Provide a recommendation for which cover to use.";

        var comparisonText = string.Join("\n\n", analyses.Select((a, i) => 
            $"Cover {i + 1}:\n{a.Analysis}"));

        var comparison = await _visionService.AnalyzeImageAsync(
            imageUrls.First(),
            comparisonPrompt + "\n\n" + comparisonText,
            cancellationToken: cancellationToken);

        return new CoverComparison
        {
            Analyses = analyses,
            ComparisonSummary = comparison
        };
    }
}

public class CoverImageAnalysis
{
    public string ImageUrl { get; set; } = string.Empty;
    public string Analysis { get; set; } = string.Empty;
    public DateTime AnalyzedAt { get; set; }
}

public class CoverComparison
{
    public List<CoverImageAnalysis> Analyses { get; set; } = new();
    public string ComparisonSummary { get; set; } = string.Empty;
}
