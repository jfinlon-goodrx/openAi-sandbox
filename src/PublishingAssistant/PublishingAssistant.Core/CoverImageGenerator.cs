using Microsoft.Extensions.Logging;
using OpenAIShared;

namespace PublishingAssistant.Core;

/// <summary>
/// Service for generating cover images using DALL-E API
/// Note: This requires OpenAI DALL-E API access
/// </summary>
public class CoverImageGenerator
{
    private readonly OpenAIClient _openAIClient;
    private readonly ILogger<CoverImageGenerator> _logger;

    public CoverImageGenerator(
        OpenAIClient openAIClient,
        ILogger<CoverImageGenerator> logger)
    {
        _openAIClient = openAIClient;
        _logger = logger;
    }

    /// <summary>
    /// Generates a cover image using DALL-E based on description
    /// </summary>
    public async Task<CoverImageResult> GenerateCoverImageAsync(
        string description,
        string size = "1024x1024",
        string quality = "standard",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating cover image with description: {Description}", description);
        
        try
        {
            // Use DALL-E 3 for high-quality book covers
            var response = await _openAIClient.GenerateImageAsync(
                prompt: description,
                model: "dall-e-3",
                size: size,
                quality: quality,
                n: 1,
                cancellationToken: cancellationToken);

            if (response.Data == null || !response.Data.Any())
            {
                throw new InvalidOperationException("No image data returned from DALL-E API");
            }

            var imageData = response.Data.First();
            var result = new CoverImageResult
            {
                ImageUrl = imageData.Url ?? throw new InvalidOperationException("No image URL returned"),
                RevisedPrompt = imageData.RevisedPrompt,
                Description = description
            };

            _logger.LogInformation("Cover image generated successfully. URL: {Url}", result.ImageUrl);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cover image");
            throw;
        }
    }

    /// <summary>
    /// Generates cover image from markdown repository
    /// </summary>
    public async Task<CoverImageResult> GenerateCoverFromMarkdownAsync(
        string markdownContent,
        string? genre = null,
        string size = "1024x1024",
        CancellationToken cancellationToken = default)
    {
        var publishingService = new PublishingService(_openAIClient, _logger);
        var description = await publishingService.GenerateCoverImageDescriptionAsync(
            markdownContent,
            genre,
            cancellationToken);

        // Combine the description with style and mood information for better results
        var enhancedPrompt = $"{description.Description}. Style: {description.Style}. Mood: {description.Mood}. " +
                           $"Color palette: {string.Join(", ", description.ColorPalette)}. " +
                           $"Professional book cover design, high quality, suitable for publishing.";

        return await GenerateCoverImageAsync(enhancedPrompt, size, cancellationToken: cancellationToken);
    }
}

public class CoverImageResult
{
    public string ImageUrl { get; set; } = string.Empty;
    public string? RevisedPrompt { get; set; }
    public string Description { get; set; } = string.Empty;
}
