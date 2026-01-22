using Models;

namespace RetroAnalyzer.Core;

/// <summary>
/// Comprehensive retrospective analysis result
/// </summary>
public class RetroAnalysisResult
{
    public string Summary { get; set; } = string.Empty;
    public List<ActionItem> ActionItems { get; set; } = new();
    public List<string> Themes { get; set; } = new();
    public SentimentAnalysis Sentiment { get; set; } = new();
    public string ImprovementSuggestions { get; set; } = string.Empty;
}
