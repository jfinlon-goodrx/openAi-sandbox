namespace Common;

/// <summary>
/// Helper methods for file operations
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// Reads text from a file, supporting various formats
    /// </summary>
    public static async Task<string> ReadTextFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".txt" or ".md" or ".cs" or ".json" or ".xml" => await File.ReadAllTextAsync(filePath),
            ".pdf" => throw new NotSupportedException("PDF parsing not implemented. Use a PDF library like PdfPig."),
            ".docx" => throw new NotSupportedException("Word document parsing not implemented. Use a library like DocumentFormat.OpenXml."),
            _ => await File.ReadAllTextAsync(filePath)
        };
    }

    /// <summary>
    /// Gets file size in bytes
    /// </summary>
    public static long GetFileSize(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        return new FileInfo(filePath).Length;
    }

    /// <summary>
    /// Validates file extension is allowed
    /// </summary>
    public static bool IsAllowedExtension(string filePath, IEnumerable<string> allowedExtensions)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return allowedExtensions.Any(ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));
    }
}
