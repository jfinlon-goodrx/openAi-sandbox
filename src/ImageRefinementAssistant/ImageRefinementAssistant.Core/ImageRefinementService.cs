using Microsoft.Extensions.Logging;
using OpenAIShared;

namespace ImageRefinementAssistant.Core;

/// <summary>
/// Main service for generating and refining images through systematic evaluation
/// </summary>
public class ImageRefinementService
{
    private readonly OpenAIClient _openAIClient;
    private readonly ImageEvaluationService _evaluationService;
    private readonly ILogger<ImageRefinementService> _logger;

    public ImageRefinementService(
        OpenAIClient openAIClient,
        ImageEvaluationService evaluationService,
        ILogger<ImageRefinementService> logger)
    {
        _openAIClient = openAIClient;
        _evaluationService = evaluationService;
        _logger = logger;
    }

    /// <summary>
    /// Generates multiple images from prompts, evaluates them, and returns the top candidates
    /// </summary>
    public async Task<ImageRefinementResult> RefineImagesAsync(
        ImageRefinementRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting image refinement. Prompts: {PromptCount}, Generate: {GenerateCount}, Return Top: {TopCount}",
            request.Prompts.Count,
            request.NumberOfImagesToGenerate,
            request.TopImagesToReturn);

        var result = new ImageRefinementResult
        {
            OriginalDescription = string.Join("; ", request.Prompts),
            ProcessedAt = DateTime.UtcNow
        };

        try
        {
            // Step 1: Generate images from all prompts
            var generatedImages = await GenerateImagesAsync(
                request.Prompts,
                request.NumberOfImagesToGenerate,
                request.Model,
                request.Size,
                request.Quality,
                cancellationToken);

            result.TotalImagesGenerated = generatedImages.Count;
            _logger.LogInformation("Generated {Count} images", generatedImages.Count);

            // Step 2: Evaluate all generated images
            var imagesToEvaluate = generatedImages.Select(img =>
                (img.ImageUrl, img.Prompt, img.RevisedPrompt)
            ).ToList();

            var evaluatedImages = await _evaluationService.EvaluateAndRankImagesAsync(
                imagesToEvaluate,
                request.EvaluationCriteria,
                cancellationToken);

            result.ImagesEvaluated = evaluatedImages.Count;
            result.AllGeneratedImages = evaluatedImages;

            // Step 3: Select top images
            result.TopImages = evaluatedImages
                .Take(request.TopImagesToReturn)
                .ToList();

            // Step 4: Generate summary
            result.Summary = GenerateSummary(result);

            _logger.LogInformation(
                "Refinement complete. Top image score: {TopScore}",
                result.TopImages.FirstOrDefault()?.OverallScore ?? 0);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during image refinement");
            throw;
        }
    }

    /// <summary>
    /// Generates images from multiple prompts, distributing the target count across prompts
    /// </summary>
    private async Task<List<GeneratedImage>> GenerateImagesAsync(
        List<string> prompts,
        int totalImagesToGenerate,
        string model,
        string size,
        string quality,
        CancellationToken cancellationToken)
    {
        var generatedImages = new List<GeneratedImage>();
        var imagesPerPrompt = Math.Max(1, totalImagesToGenerate / prompts.Count);
        var remainingImages = totalImagesToGenerate;

        foreach (var prompt in prompts)
        {
            if (remainingImages <= 0) break;

            var imagesForThisPrompt = Math.Min(imagesPerPrompt, remainingImages);
            
            _logger.LogInformation(
                "Generating {Count} images for prompt: {Prompt}",
                imagesForThisPrompt,
                prompt);

            // Generate images one at a time (DALL-E 3 only supports n=1)
            for (int i = 0; i < imagesForThisPrompt; i++)
            {
                try
                {
                    var response = await _openAIClient.GenerateImageAsync(
                        prompt: prompt,
                        model: model,
                        size: size,
                        quality: quality,
                        n: 1,
                        cancellationToken: cancellationToken);

                    if (response.Data != null && response.Data.Any())
                    {
                        var imageData = response.Data.First();
                        if (!string.IsNullOrEmpty(imageData.Url))
                        {
                            generatedImages.Add(new GeneratedImage
                            {
                                ImageUrl = imageData.Url,
                                Prompt = prompt,
                                RevisedPrompt = imageData.RevisedPrompt,
                                GeneratedAt = DateTime.UtcNow
                            });
                        }
                    }

                    // Small delay to avoid rate limiting
                    await Task.Delay(500, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to generate image for prompt: {Prompt}", prompt);
                }
            }

            remainingImages -= imagesForThisPrompt;
        }

        return generatedImages;
    }

    private string GenerateSummary(ImageRefinementResult result)
    {
        if (!result.TopImages.Any())
        {
            return "No images were successfully generated or evaluated.";
        }

        var topImage = result.TopImages.First();
        var avgScore = result.TopImages.Average(img => img.OverallScore);
        var scoreRange = result.TopImages.Max(img => img.OverallScore) - 
                        result.TopImages.Min(img => img.OverallScore);

        return $"""
            Generated {result.TotalImagesGenerated} images from {result.OriginalDescription.Split(';').Length} prompt(s).
            Evaluated {result.ImagesEvaluated} images and selected the top {result.TopImages.Count} candidates.
            
            Top Image Score: {topImage.OverallScore:F2}/10.0
            Average Score of Top Images: {avgScore:F2}/10.0
            Score Range: {scoreRange:F2}
            
            Top image strengths: {string.Join(", ", topImage.Strengths.Take(3))}
            """;
    }

    private class GeneratedImage
    {
        public string ImageUrl { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public string? RevisedPrompt { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
