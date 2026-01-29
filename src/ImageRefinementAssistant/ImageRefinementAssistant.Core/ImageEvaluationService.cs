using Microsoft.Extensions.Logging;
using OpenAIShared;
using System.Text.Json;

namespace ImageRefinementAssistant.Core;

/// <summary>
/// Service for evaluating generated images using GPT-4 Vision API
/// </summary>
public class ImageEvaluationService
{
    private readonly VisionService _visionService;
    private readonly ILogger<ImageEvaluationService> _logger;

    public ImageEvaluationService(
        VisionService visionService,
        ILogger<ImageEvaluationService> logger)
    {
        _visionService = visionService;
        _logger = logger;
    }

    /// <summary>
    /// Evaluates a single image and returns a scored result
    /// </summary>
    public async Task<ImageEvaluationResult> EvaluateImageAsync(
        string imageUrl,
        string originalPrompt,
        string? revisedPrompt,
        string? customCriteria = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Evaluating image: {ImageUrl}", imageUrl);

        var evaluationPrompt = BuildEvaluationPrompt(originalPrompt, customCriteria);
        
        try
        {
            var analysis = await _visionService.AnalyzeImageAsync(
                imageUrl,
                evaluationPrompt,
                "high",
                cancellationToken);

            var result = ParseEvaluationResponse(analysis, imageUrl, originalPrompt, revisedPrompt);
            
            _logger.LogInformation(
                "Image evaluated. Overall Score: {Score}, Quality: {Quality}, Relevance: {Relevance}",
                result.OverallScore,
                result.QualityScore,
                result.RelevanceScore);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating image: {ImageUrl}", imageUrl);
            // Return a default result with low scores if evaluation fails
            return new ImageEvaluationResult
            {
                ImageUrl = imageUrl,
                Prompt = originalPrompt,
                RevisedPrompt = revisedPrompt,
                QualityScore = 0.0,
                RelevanceScore = 0.0,
                OverallScore = 0.0,
                EvaluationDetails = $"Error during evaluation: {ex.Message}",
                EvaluatedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Evaluates multiple images and ranks them
    /// </summary>
    public async Task<List<ImageEvaluationResult>> EvaluateAndRankImagesAsync(
        List<(string ImageUrl, string Prompt, string? RevisedPrompt)> images,
        string? customCriteria = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Evaluating and ranking {Count} images", images.Count);

        var evaluationTasks = images.Select(img =>
            EvaluateImageAsync(img.ImageUrl, img.Prompt, img.RevisedPrompt, customCriteria, cancellationToken)
        ).ToList();

        var results = await Task.WhenAll(evaluationTasks);

        // Sort by overall score descending
        var rankedResults = results
            .OrderByDescending(r => r.OverallScore)
            .ToList();

        _logger.LogInformation(
            "Evaluation complete. Top score: {TopScore}, Bottom score: {BottomScore}",
            rankedResults.FirstOrDefault()?.OverallScore ?? 0,
            rankedResults.LastOrDefault()?.OverallScore ?? 0);

        return rankedResults;
    }

    private string BuildEvaluationPrompt(string originalPrompt, string? customCriteria)
    {
        var jsonExample = @"{
    ""qualityScore"": <number 0-10>,
    ""relevanceScore"": <number 0-10>,
    ""evaluationDetails"": ""<detailed explanation>"",
    ""strengths"": [""<strength1>"", ""<strength2>"", ...],
    ""weaknesses"": [""<weakness1>"", ""<weakness2>"", ...]
}";
        
        var basePrompt = $@"Evaluate this image that was generated from the prompt: ""{originalPrompt}""

Please provide a detailed evaluation in the following JSON format:
{jsonExample}

Evaluation criteria:
1. Quality Score (0-10): Technical quality, visual appeal, composition, color harmony, detail level
2. Relevance Score (0-10): How well the image matches the original prompt, accuracy, appropriateness
3. Evaluation Details: A comprehensive analysis explaining your scores
4. Strengths: List specific positive aspects of the image
5. Weaknesses: List specific areas for improvement

{(customCriteria != null ? $"\nAdditional criteria: {customCriteria}" : "")}

            Be thorough and specific in your evaluation. Consider:
            - Visual composition and balance
            - Color palette and harmony
            - Technical execution (sharpness, clarity)
            - Adherence to the prompt requirements
            - Overall aesthetic appeal
            - Professional quality
            """;

        return basePrompt;
    }

    private ImageEvaluationResult ParseEvaluationResponse(
        string analysisText,
        string imageUrl,
        string originalPrompt,
        string? revisedPrompt)
    {
        var result = new ImageEvaluationResult
        {
            ImageUrl = imageUrl,
            Prompt = originalPrompt,
            RevisedPrompt = revisedPrompt,
            EvaluatedAt = DateTime.UtcNow
        };

        try
        {
            // Try to extract JSON from the response
            var jsonStart = analysisText.IndexOf('{');
            var jsonEnd = analysisText.LastIndexOf('}') + 1;
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonText = analysisText.Substring(jsonStart, jsonEnd - jsonStart);
                var evaluation = JsonSerializer.Deserialize<JsonEvaluationResponse>(
                    jsonText,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (evaluation != null)
                {
                    result.QualityScore = evaluation.QualityScore;
                    result.RelevanceScore = evaluation.RelevanceScore;
                    result.EvaluationDetails = evaluation.EvaluationDetails ?? analysisText;
                    result.Strengths = evaluation.Strengths ?? new List<string>();
                    result.Weaknesses = evaluation.Weaknesses ?? new List<string>();
                    result.OverallScore = (evaluation.QualityScore + evaluation.RelevanceScore) / 2.0;
                    return result;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse JSON from evaluation response, using fallback parsing");
        }

        // Fallback: Try to extract scores from text if JSON parsing fails
        result.EvaluationDetails = analysisText;
        result.QualityScore = ExtractScoreFromText(analysisText, "quality");
        result.RelevanceScore = ExtractScoreFromText(analysisText, "relevance");
        result.OverallScore = (result.QualityScore + result.RelevanceScore) / 2.0;

        // Extract strengths and weaknesses from text
        result.Strengths = ExtractListFromText(analysisText, "strength");
        result.Weaknesses = ExtractListFromText(analysisText, "weakness");

        return result;
    }

    private double ExtractScoreFromText(string text, string keyword)
    {
        // Look for patterns like "quality score: 8" or "quality: 8/10"
        var patterns = new[]
        {
            $@"{keyword}.*?(\d+(?:\.\d+)?)",
            $@"{keyword}.*?(\d+)/10",
            $@"{keyword}.*?(\d+)"
        };

        foreach (var pattern in patterns)
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                text,
                pattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (match.Success && double.TryParse(match.Groups[1].Value, out var score))
            {
                return Math.Min(10.0, Math.Max(0.0, score));
            }
        }

        return 5.0; // Default score if not found
    }

    private List<string> ExtractListFromText(string text, string keyword)
    {
        var items = new List<string>();
        // Simple extraction - look for bullet points or numbered lists after the keyword
        var keywordIndex = text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
        if (keywordIndex >= 0)
        {
            var section = text.Substring(keywordIndex);
            var lines = section.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if ((trimmed.StartsWith("-") || trimmed.StartsWith("*") || 
                     System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^\d+[\.\)]")) &&
                    trimmed.Length > 2)
                {
                    items.Add(trimmed.Substring(trimmed.IndexOfAny(new[] { '-', '*', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }) + 1).Trim());
                }
            }
        }
        return items.Take(5).ToList(); // Limit to 5 items
    }

    private class JsonEvaluationResponse
    {
        public double QualityScore { get; set; }
        public double RelevanceScore { get; set; }
        public string? EvaluationDetails { get; set; }
        public List<string>? Strengths { get; set; }
        public List<string>? Weaknesses { get; set; }
    }
}
