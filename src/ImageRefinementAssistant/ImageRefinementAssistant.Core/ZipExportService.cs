using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace ImageRefinementAssistant.Core;

/// <summary>
/// Service for creating ZIP files containing images and metadata
/// </summary>
public class ZipExportService
{
    private readonly ILogger<ZipExportService> _logger;
    private readonly string _tempZipPath;
    private readonly Dictionary<string, string> _zipFiles = new(); // zipFileId -> filePath
    private readonly Timer _cleanupTimer;

    public ZipExportService(ILogger<ZipExportService> logger)
    {
        _logger = logger;
        
        // Create temp directory for zip files
        _tempZipPath = Path.Combine(Path.GetTempPath(), "ImageRefinementAssistant", "zips");
        Directory.CreateDirectory(_tempZipPath);
        
        // Cleanup old zip files every hour
        _cleanupTimer = new Timer(CleanupOldZipFiles, null, TimeSpan.FromHours(1), TimeSpan.FromHours(1));
        
        // Initial cleanup
        CleanupOldZipFiles(null);
    }

    /// <summary>
    /// Creates a ZIP file containing selected images and metadata
    /// </summary>
    public async Task<ZipExportResult> CreateZipFileAsync(
        ImageRefinementResult result,
        List<string> selectedImageUrls,
        CancellationToken cancellationToken = default)
    {
        var zipFileId = Guid.NewGuid().ToString("N");
        var zipFileName = $"image_refinement_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}.zip";
        var zipFilePath = Path.Combine(_tempZipPath, $"{zipFileId}.zip");

        // Determine which images to include
        var imagesToExport = selectedImageUrls.Count > 0
            ? result.AllGeneratedImages.Where(img => selectedImageUrls.Contains(img.ImageUrl)).ToList()
            : result.AllGeneratedImages.ToList();

        if (!imagesToExport.Any())
        {
            throw new InvalidOperationException("No images selected for export");
        }

        _logger.LogInformation(
            "Creating ZIP file {ZipFileId} with {ImageCount} images",
            zipFileId,
            imagesToExport.Count);

        var httpClient = new HttpClient();
        var savedImages = new List<SavedImageInfo>();

        try
        {
            using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                // Download and add images to ZIP
                foreach (var (image, index) in imagesToExport.Select((img, idx) => (img, idx)))
                {
                    try
                    {
                        var imageInfo = await AddImageToZipAsync(
                            httpClient,
                            zipArchive,
                            image,
                            index + 1,
                            cancellationToken);
                        savedImages.Add(imageInfo);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to add image {Index} to ZIP: {ImageUrl}", index + 1, image.ImageUrl);
                    }
                }

                // Add metadata files
                await AddMetadataToZipAsync(zipArchive, result, savedImages, cancellationToken);
            }

            // Store zip file info for download
            _zipFiles[zipFileId] = zipFilePath;

            _logger.LogInformation(
                "ZIP file created successfully: {ZipFilePath} ({ImageCount} images)",
                zipFilePath,
                savedImages.Count);

            return new ZipExportResult
            {
                ZipFileId = zipFileId,
                ImagesSaved = savedImages.Count,
                ZipFileName = zipFileName
            };
        }
        finally
        {
            httpClient.Dispose();
        }
    }

    /// <summary>
    /// Gets the path to a zip file by ID
    /// </summary>
    public string? GetZipFilePath(string zipFileId)
    {
        return _zipFiles.TryGetValue(zipFileId, out var path) && File.Exists(path) ? path : null;
    }

    /// <summary>
    /// Adds an image to the ZIP archive
    /// </summary>
    private async Task<SavedImageInfo> AddImageToZipAsync(
        HttpClient httpClient,
        ZipArchive zipArchive,
        ImageEvaluationResult image,
        int rank,
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
        var fileName = $"images/image_{rank:D2}_{safePrompt}.{extension}";

        var entry = zipArchive.CreateEntry(fileName);
        using (var entryStream = entry.Open())
        {
            await entryStream.WriteAsync(imageBytes, cancellationToken);
        }

        return new SavedImageInfo
        {
            Rank = rank,
            FileName = fileName,
            FilePath = fileName, // Path within ZIP
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
    /// Adds metadata files to the ZIP archive
    /// </summary>
    private async Task AddMetadataToZipAsync(
        ZipArchive zipArchive,
        ImageRefinementResult result,
        List<SavedImageInfo> savedImages,
        CancellationToken cancellationToken)
    {
        // Mark top images
        var topImageUrls = result.TopImages?.Select(img => img.ImageUrl).ToHashSet() ?? new HashSet<string>();
        foreach (var savedImage in savedImages)
        {
            savedImage.IsTopImage = topImageUrls.Contains(savedImage.ImageUrl);
        }

        // Add metadata.json
        var metadataJson = CreateMetadataJson(result, savedImages);
        var jsonEntry = zipArchive.CreateEntry("metadata.json");
        using (var jsonStream = jsonEntry.Open())
        {
            var jsonBytes = Encoding.UTF8.GetBytes(metadataJson);
            await jsonStream.WriteAsync(jsonBytes, cancellationToken);
        }

        // Add metadata.csv
        var metadataCsv = CreateMetadataCsv(result, savedImages);
        var csvEntry = zipArchive.CreateEntry("metadata.csv");
        using (var csvStream = csvEntry.Open())
        {
            var csvBytes = Encoding.UTF8.GetBytes(metadataCsv);
            await csvStream.WriteAsync(csvBytes, cancellationToken);
        }

        // Add summary.txt
        var summaryText = CreateSummaryText(result, savedImages);
        var summaryEntry = zipArchive.CreateEntry("summary.txt");
        using (var summaryStream = summaryEntry.Open())
        {
            var summaryBytes = Encoding.UTF8.GetBytes(summaryText);
            await summaryStream.WriteAsync(summaryBytes, cancellationToken);
        }
    }

    private string CreateMetadataJson(ImageRefinementResult result, List<SavedImageInfo> savedImages)
    {
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

        return JsonSerializer.Serialize(exportData, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private string CreateMetadataCsv(ImageRefinementResult result, List<SavedImageInfo> savedImages)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Rank,FileName,Prompt,RevisedPrompt,QualityScore,RelevanceScore,OverallScore,IsTopImage,ImageUrl");

        foreach (var savedImage in savedImages.OrderBy(img => img.Rank))
        {
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

        return csv.ToString();
    }

    private string CreateSummaryText(ImageRefinementResult result, List<SavedImageInfo> savedImages)
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
        summary.AppendLine($"Images in ZIP: {savedImages.Count}");
        summary.AppendLine($"Top Images Selected: {result.TopImages?.Count ?? 0}");
        summary.AppendLine();
        summary.AppendLine("Summary:");
        summary.AppendLine(result.Summary);
        summary.AppendLine();
        summary.AppendLine("=".PadRight(60, '='));
        summary.AppendLine();
        summary.AppendLine("Images in ZIP (sorted by score):");
        summary.AppendLine();

        foreach (var (image, index) in savedImages
            .OrderByDescending(img => img.OverallScore)
            .Select((img, idx) => (img, idx + 1)))
        {
            var isTop = result.TopImages?.Any(t => t.ImageUrl == image.ImageUrl) ?? false;
            summary.AppendLine($"{index}. Rank #{image.Rank} - Score: {image.OverallScore:F2}/10.0 {(isTop ? "[TOP]" : "")}");
            summary.AppendLine($"   File: {image.FileName}");
            summary.AppendLine($"   Prompt: {image.Prompt}");
            summary.AppendLine($"   Quality: {image.QualityScore:F2}/10 | Relevance: {image.RelevanceScore:F2}/10");
            summary.AppendLine();
        }

        return summary.ToString();
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

        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }

    private void CleanupOldZipFiles(object? state)
    {
        try
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-24); // Keep files for 24 hours
            
            foreach (var file in Directory.GetFiles(_tempZipPath, "*.zip"))
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTimeUtc < cutoffTime)
                {
                    try
                    {
                        File.Delete(file);
                        _logger.LogInformation("Cleaned up old ZIP file: {FilePath}", file);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete old ZIP file: {FilePath}", file);
                    }
                }
            }

            // Remove from dictionary if file was deleted
            var keysToRemove = _zipFiles
                .Where(kvp => !File.Exists(kvp.Value))
                .Select(kvp => kvp.Key)
                .ToList();
            
            foreach (var key in keysToRemove)
            {
                _zipFiles.Remove(key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during ZIP file cleanup");
        }
    }
}

/// <summary>
/// Result of creating a ZIP export
/// </summary>
public class ZipExportResult
{
    public string ZipFileId { get; set; } = string.Empty;
    public int ImagesSaved { get; set; }
    public string ZipFileName { get; set; } = string.Empty;
}
