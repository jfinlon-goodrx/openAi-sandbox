using Microsoft.AspNetCore.Mvc;
using ImageRefinementAssistant.Core;
using OpenAIShared;
using Microsoft.Extensions.DependencyInjection;

namespace ImageRefinementAssistant.Api.Controllers;

/// <summary>
/// API controller for image refinement workflow
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ImageRefinementController : ControllerBase
{
    private readonly ImageRefinementService _refinementService;
    private readonly ImageStorageService _storageService;
    private readonly ZipExportService _zipExportService;
    private readonly ILogger<ImageRefinementController> _logger;

    public ImageRefinementController(
        ImageRefinementService refinementService,
        ImageStorageService storageService,
        ZipExportService zipExportService,
        ILogger<ImageRefinementController> logger)
    {
        _refinementService = refinementService;
        _storageService = storageService;
        _zipExportService = zipExportService;
        _logger = logger;
    }

    /// <summary>
    /// Generates multiple images from prompts, evaluates them, and returns the top candidates
    /// </summary>
    /// <param name="request">Request containing prompts and refinement parameters</param>
    /// <returns>Refinement result with top images</returns>
    [HttpPost("refine")]
    public async Task<ActionResult<ImageRefinementResult>> RefineImages(
        [FromBody] ImageRefinementRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.Prompts == null || !request.Prompts.Any())
            {
                return BadRequest(new { error = "At least one prompt is required" });
            }

            if (request.NumberOfImagesToGenerate < 1)
            {
                return BadRequest(new { error = "NumberOfImagesToGenerate must be at least 1" });
            }

            if (request.TopImagesToReturn < 1 || request.TopImagesToReturn > request.NumberOfImagesToGenerate)
            {
                return BadRequest(new { error = "TopImagesToReturn must be between 1 and NumberOfImagesToGenerate" });
            }

            _logger.LogInformation(
                "Processing refinement request: {PromptCount} prompts, {GenerateCount} images, top {TopCount}",
                request.Prompts.Count,
                request.NumberOfImagesToGenerate,
                request.TopImagesToReturn);

            var result = await _refinementService.RefineImagesAsync(request, cancellationToken);

            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Image refinement request was cancelled");
            return StatusCode(499, new { error = "Request was cancelled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refining images");
            return StatusCode(500, new { error = "Failed to refine images", message = ex.Message });
        }
    }

    /// <summary>
    /// Exports refinement result: saves all images and metadata to a local folder
    /// </summary>
    /// <param name="result">The refinement result to export</param>
    /// <param name="folderName">Optional custom folder name (defaults to timestamp-based name)</param>
    /// <returns>Export result with paths and saved image information</returns>
    [HttpPost("export")]
    public async Task<ActionResult<ExportResult>> ExportRefinementResult(
        [FromBody] ImageRefinementResult result,
        [FromQuery] string? folderName = null,
        [FromQuery] string? customPath = null)
    {
        try
        {
            if (result == null || result.AllGeneratedImages == null || !result.AllGeneratedImages.Any())
            {
                return BadRequest(new { error = "Refinement result with images is required" });
            }

            _logger.LogInformation(
                "Exporting refinement result: {ImageCount} images to folder: {FolderName}, custom path: {CustomPath}",
                result.AllGeneratedImages.Count,
                folderName ?? "auto",
                customPath ?? "default");

            // If custom path is provided, create a new storage service instance with that path
            ImageStorageService storageService = _storageService;
            if (!string.IsNullOrWhiteSpace(customPath))
            {
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<ImageStorageService>>();
                storageService = new ImageStorageService(logger, customPath.Trim());
            }

            var exportResult = await storageService.ExportRefinementResultAsync(result, folderName);

            return Ok(exportResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting refinement result");
            return StatusCode(500, new { error = "Failed to export refinement result", message = ex.Message });
        }
    }

    /// <summary>
    /// Exports selected images as a ZIP file
    /// </summary>
    [HttpPost("export-zip")]
    public async Task<ActionResult<ZipExportResult>> ExportZip(
        [FromBody] ZipExportRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.RefinementResult == null || request.RefinementResult.AllGeneratedImages == null || !request.RefinementResult.AllGeneratedImages.Any())
            {
                return BadRequest(new { error = "Refinement result with images is required" });
            }

            _logger.LogInformation(
                "Exporting ZIP: {ImageCount} total images, {SelectedCount} selected",
                request.RefinementResult.AllGeneratedImages.Count,
                request.SelectedImageUrls?.Count ?? 0);

            var result = await _zipExportService.CreateZipFileAsync(
                request.RefinementResult,
                request.SelectedImageUrls ?? new List<string>(),
                cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ZIP export");
            return StatusCode(500, new { error = "Failed to create ZIP file", message = ex.Message });
        }
    }

    /// <summary>
    /// Downloads a ZIP file by ID
    /// </summary>
    [HttpGet("download-zip/{zipFileId}")]
    public IActionResult DownloadZip(string zipFileId)
    {
        try
        {
            var zipFilePath = _zipExportService.GetZipFilePath(zipFileId);
            
            if (zipFilePath == null || !System.IO.File.Exists(zipFilePath))
            {
                _logger.LogWarning("ZIP file not found: {ZipFileId}", zipFileId);
                return NotFound(new { error = "ZIP file not found or expired" });
            }

            var zipFileName = $"image_refinement_{zipFileId.Substring(0, 8)}.zip";
            var fileBytes = System.IO.File.ReadAllBytes(zipFilePath);
            
            _logger.LogInformation("Serving ZIP file: {ZipFileId}, Size: {Size} bytes", zipFileId, fileBytes.Length);

            return File(fileBytes, "application/zip", zipFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading ZIP file: {ZipFileId}", zipFileId);
            return StatusCode(500, new { error = "Failed to download ZIP file", message = ex.Message });
        }
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "ImageRefinementAssistant" });
    }
}

/// <summary>
/// Request for ZIP export
/// </summary>
public class ZipExportRequest
{
    public ImageRefinementResult RefinementResult { get; set; } = null!;
    public List<string>? SelectedImageUrls { get; set; }
}
