using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ImageRefinementAssistant.Core;

/// <summary>
/// Service for saving images and metadata to local storage
/// </summary>
public class ImageStorageService
{
    private readonly ILogger<ImageStorageService> _logger;
    private readonly string _storagePath;

    public ImageStorageService(
        ILogger<ImageStorageService> logger,
        string? storagePath = null)
    {
        _logger = logger;
        
        if (!string.IsNullOrWhiteSpace(storagePath))
        {
            _storagePath = storagePath;
        }
        else
        {
            var downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads",
                "ImageRefinementAssistant");
            _storagePath = downloadsPath;
        }

        // Ensure storage directory exists
        if (!string.IsNullOrWhiteSpace(_storagePath) && !Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
            _logger.LogInformation("Created storage directory: {StoragePath}", _storagePath);
        }
    }

    /// <summary>
    /// Exports a refinement result: saves all images and metadata to a local folder
    /// </summary>
    public async Task<ExportResult> ExportRefinementResultAsync(
        ImageRefinementResult result,
        string? customFolderName = null,
        CancellationToken cancellationToken = default)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
        var folderName = customFolderName ?? $"refinement_{timestamp}";
        var exportPath = Path.Combine(_storagePath, folderName);

        // Create export directory
        Directory.CreateDirectory(exportPath);
        _logger.LogInformation("Exporting refinement result to: {ExportPath}", exportPath);

        var savedImages = new List<SavedImageInfo>();
        var httpClient = new HttpClient();

        try
        {
            // Save all images
            foreach (var (image, index) in result.AllGeneratedImages.Select((img, idx) => (img, idx)))
            {
                try
                {
                    var imageInfo = await SaveImageAsync(
                        httpClient,
                        image,
                        index + 1,
                        exportPath,
                        cancellationToken);
                    savedImages.Add(imageInfo);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to save image {Index}: {ImageUrl}", index + 1, image.ImageUrl);
                }
            }

            // Save metadata as JSON
            var metadataPath = Path.Combine(exportPath, "metadata.json");
            await SaveMetadataJsonAsync(result, savedImages, metadataPath, cancellationToken);

            // Save metadata as CSV for easy viewing
            var csvPath = Path.Combine(exportPath, "metadata.csv");
            await SaveMetadataCsvAsync(result, savedImages, csvPath, cancellationToken);

            // Save summary text file
            var summaryPath = Path.Combine(exportPath, "summary.txt");
            await SaveSummaryTextAsync(result, exportPath, summaryPath, cancellationToken);

            _logger.LogInformation(
                "Export complete. Saved {ImageCount} images and metadata to {ExportPath}",
                savedImages.Count,
                exportPath);

            return new ExportResult
            {
                ExportPath = exportPath,
                ImagesSaved = savedImages.Count,
                MetadataPath = metadataPath,
                CsvPath = csvPath,
                SummaryPath = summaryPath,
                SavedImages = savedImages
            };
        }
        finally
        {
            httpClient.Dispose();
        }
    }

    /// <summary>
    /// Saves a single image from URL to local file
    /// </summary>
    private async Task<SavedImageInfo> SaveImageAsync(
        HttpClient httpClient,
        ImageEvaluationResult image,
        int rank,
        string exportPath,
        CancellationToken cancellationToken)
    {
        var imageResponse = await httpClient.GetAsync(image.ImageUrl, cancellationToken);
        imageResponse.EnsureSuccessStatusCode();

        var imageBytes = await imageResponse.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = imageResponse.Content.Headers.ContentType?.MediaType ?? "image/png";
        var extension = GetFileExtension(contentType);

        // Create safe filename
        var safePrompt = SanitizeFileName(
            image.Prompt.Length > 40 ? image.Prompt.Substring(0, 40) : image.Prompt);
        var fileName = $"image_{rank:D2}_{safePrompt}.{extension}";
        var filePath = Path.Combine(exportPath, fileName);

        await File.WriteAllBytesAsync(filePath, imageBytes, cancellationToken);

        return new SavedImageInfo
        {
            Rank = rank,
            FileName = fileName,
            FilePath = filePath,
            ImageUrl = image.ImageUrl,
            Prompt = image.Prompt,
            RevisedPrompt = image.RevisedPrompt,
            QualityScore = image.QualityScore,
            RelevanceScore = image.RelevanceScore,
            OverallScore = image.OverallScore,
            IsTopImage = false // Will be set later
        };
    }

    /// <summary>
    /// Saves metadata as JSON
    /// </summary>
    private async Task SaveMetadataJsonAsync(
        ImageRefinementResult result,
        List<SavedImageInfo> savedImages,
        string metadataPath,
        CancellationToken cancellationToken)
    {
        // Mark top images
        var topImageUrls = result.TopImages?.Select(img => img.ImageUrl).ToHashSet() ?? new HashSet<string>();
        foreach (var savedImage in savedImages)
        {
            savedImage.IsTopImage = topImageUrls.Contains(savedImage.ImageUrl);
        }

        var exportData = new
        {
            ExportDate = DateTime.UtcNow,
            OriginalDescription = result.OriginalDescription,
            TotalImagesGenerated = result.TotalImagesGenerated,
            ImagesEvaluated = result.ImagesEvaluated,
            TopImagesCount = result.TopImages?.Count ?? 0,
            Summary = result.Summary,
            Images = savedImages.Select(img => new
            {
                img.Rank,
                img.FileName,
                img.Prompt,
                img.RevisedPrompt,
                img.QualityScore,
                img.RelevanceScore,
                img.OverallScore,
                img.IsTopImage,
                img.ImageUrl,
                Strengths = result.AllGeneratedImages
                    .FirstOrDefault(i => i.ImageUrl == img.ImageUrl)?.Strengths ?? new List<string>(),
                Weaknesses = result.AllGeneratedImages
                    .FirstOrDefault(i => i.ImageUrl == img.ImageUrl)?.Weaknesses ?? new List<string>()
            }).ToList()
        };

        var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(metadataPath, json, cancellationToken);
    }

    /// <summary>
    /// Saves metadata as CSV for easy viewing in Excel/spreadsheet apps
    /// </summary>
    private async Task SaveMetadataCsvAsync(
        ImageRefinementResult result,
        List<SavedImageInfo> savedImages,
        string csvPath,
        CancellationToken cancellationToken)
    {
        var csv = new StringBuilder();
        
        // Header
        csv.AppendLine("Rank,FileName,Prompt,RevisedPrompt,QualityScore,RelevanceScore,OverallScore,IsTopImage,ImageUrl");

        // Mark top images
        var topImageUrls = result.TopImages?.Select(img => img.ImageUrl).ToHashSet() ?? new HashSet<string>();
        
        // Data rows
        foreach (var savedImage in savedImages.OrderBy(img => img.Rank))
        {
            savedImage.IsTopImage = topImageUrls.Contains(savedImage.ImageUrl);
            
            var prompt = EscapeCsvField(savedImage.Prompt);
            var revisedPrompt = EscapeCsvField(savedImage.RevisedPrompt ?? "");
            
            csv.AppendLine(
                $"{savedImage.Rank}," +
                $"{EscapeCsvField(savedImage.FileName)}," +
                $"{prompt}," +
                $"{revisedPrompt}," +
                $"{savedImage.QualityScore:F2}," +
                $"{savedImage.RelevanceScore:F2}," +
                $"{savedImage.OverallScore:F2}," +
                $"{(savedImage.IsTopImage ? "Yes" : "No")}," +
                $"{EscapeCsvField(savedImage.ImageUrl)}");
        }

        await File.WriteAllTextAsync(csvPath, csv.ToString(), cancellationToken);
    }

    /// <summary>
    /// Saves a human-readable summary text file
    /// </summary>
    private async Task SaveSummaryTextAsync(
        ImageRefinementResult result,
        string exportPath,
        string summaryPath,
        CancellationToken cancellationToken)
    {
        var summary = new StringBuilder();
        summary.AppendLine("Image Refinement Assistant - Export Summary");
        summary.AppendLine("=".PadRight(60, '='));
        summary.AppendLine();
        summary.AppendLine($"Export Date: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}");
        summary.AppendLine($"Original Description: {result.OriginalDescription}");
        summary.AppendLine();
        summary.AppendLine($"Total Images Generated: {result.TotalImagesGenerated}");
        summary.AppendLine($"Images Evaluated: {result.ImagesEvaluated}");
        summary.AppendLine($"Top Images Selected: {result.TopImages?.Count ?? 0}");
        summary.AppendLine();
        summary.AppendLine("Summary:");
        summary.AppendLine(result.Summary);
        summary.AppendLine();
        summary.AppendLine("=".PadRight(60, '='));
        summary.AppendLine();
        summary.AppendLine("Top Images:");
        summary.AppendLine();

        if (result.TopImages != null)
        {
            foreach (var (topImage, index) in result.TopImages.Select((img, idx) => (img, idx + 1)))
            {
                var savedImage = result.AllGeneratedImages
                    .Select((img, idx) => (img, idx + 1))
                    .FirstOrDefault(x => x.img.ImageUrl == topImage.ImageUrl);
                
                summary.AppendLine($"{index}. Rank #{savedImage.Item2} - Score: {topImage.OverallScore:F2}/10.0");
                summary.AppendLine($"   Prompt: {topImage.Prompt}");
                if (!string.IsNullOrEmpty(topImage.RevisedPrompt) && topImage.RevisedPrompt != topImage.Prompt)
                {
                    summary.AppendLine($"   Revised: {topImage.RevisedPrompt}");
                }
                summary.AppendLine($"   Quality: {topImage.QualityScore:F2}/10 | Relevance: {topImage.RelevanceScore:F2}/10");
                if (topImage.Strengths != null && topImage.Strengths.Any())
                {
                    summary.AppendLine($"   Strengths: {string.Join(", ", topImage.Strengths.Take(3))}");
                }
                summary.AppendLine();
            }
        }

        summary.AppendLine("=".PadRight(60, '='));
        summary.AppendLine();
        summary.AppendLine("All Images (sorted by score):");
        summary.AppendLine();

        foreach (var (image, index) in result.AllGeneratedImages
            .OrderByDescending(img => img.OverallScore)
            .Select((img, idx) => (img, idx + 1)))
        {
            var isTop = result.TopImages?.Any(t => t.ImageUrl == image.ImageUrl) ?? false;
            summary.AppendLine($"{index}. Rank #{index} - Score: {image.OverallScore:F2}/10.0 {(isTop ? "[TOP]" : "")}");
            summary.AppendLine($"   Prompt: {image.Prompt}");
            summary.AppendLine($"   Quality: {image.QualityScore:F2}/10 | Relevance: {image.RelevanceScore:F2}/10");
            summary.AppendLine();
        }

        summary.AppendLine();
        summary.AppendLine($"Export Location: {exportPath}");
        summary.AppendLine($"Images saved: {result.AllGeneratedImages.Count}");
        summary.AppendLine($"Metadata files: metadata.json, metadata.csv, summary.txt");

        await File.WriteAllTextAsync(summaryPath, summary.ToString());
    }

    private string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        sanitized = sanitized.Replace(" ", "_");
        sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"_{2,}", "_");
        return sanitized.Trim('_');
    }

    private string GetFileExtension(string contentType)
    {
        return contentType.ToLowerInvariant() switch
        {
            "image/png" => "png",
            "image/jpeg" or "image/jpg" => "jpg",
            "image/gif" => "gif",
            "image/webp" => "webp",
            _ => "png"
        };
    }

    private string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return "";

        // If field contains comma, quote, or newline, wrap in quotes and escape quotes
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }
}

/// <summary>
/// Information about a saved image
/// </summary>
public class SavedImageInfo
{
    public int Rank { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string? RevisedPrompt { get; set; }
    public double QualityScore { get; set; }
    public double RelevanceScore { get; set; }
    public double OverallScore { get; set; }
    public bool IsTopImage { get; set; }
}

/// <summary>
/// Result of exporting a refinement result
/// </summary>
public class ExportResult
{
    public string ExportPath { get; set; } = string.Empty;
    public int ImagesSaved { get; set; }
    public string MetadataPath { get; set; } = string.Empty;
    public string CsvPath { get; set; } = string.Empty;
    public string SummaryPath { get; set; } = string.Empty;
    public List<SavedImageInfo> SavedImages { get; set; } = new();
}
