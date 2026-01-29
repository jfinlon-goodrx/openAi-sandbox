namespace ImageRefinementAssistant.Core;

/// <summary>
/// Result of evaluating a single generated image
/// </summary>
public class ImageEvaluationResult
{
    public string ImageUrl { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string? RevisedPrompt { get; set; }
    public double QualityScore { get; set; }
    public double RelevanceScore { get; set; }
    public double OverallScore { get; set; }
    public string EvaluationDetails { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public DateTime EvaluatedAt { get; set; }
}

/// <summary>
/// Result of the complete refinement process
/// </summary>
public class ImageRefinementResult
{
    public string OriginalDescription { get; set; } = string.Empty;
    public List<ImageEvaluationResult> AllGeneratedImages { get; set; } = new();
    public List<ImageEvaluationResult> TopImages { get; set; } = new();
    public int TotalImagesGenerated { get; set; }
    public int ImagesEvaluated { get; set; }
    public string Summary { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}

/// <summary>
/// Request for image refinement
/// </summary>
public class ImageRefinementRequest
{
    public List<string> Prompts { get; set; } = new();
    public int NumberOfImagesToGenerate { get; set; } = 8;
    public int TopImagesToReturn { get; set; } = 3;
    public string? EvaluationCriteria { get; set; }
    public string Size { get; set; } = "1024x1024";
    public string Quality { get; set; } = "standard";
    public string Model { get; set; } = "dall-e-3";
}
