using System.Text;
using Microsoft.Extensions.Logging;

namespace PublishingAssistant.Core;

/// <summary>
/// Service for intelligently chunking large documents to minimize token usage
/// </summary>
public class DocumentChunkingService
{
    private readonly ILogger<DocumentChunkingService> _logger;
    private const int DefaultChunkSize = 2000; // ~1500 tokens per chunk (conservative)
    private const int ChunkOverlap = 200; // Overlap to maintain context

    public DocumentChunkingService(ILogger<DocumentChunkingService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Chunks a document intelligently, preserving paragraph and chapter boundaries
    /// </summary>
    public List<DocumentChunk> ChunkDocument(
        string documentContent,
        string documentId,
        int? maxChunkSize = null)
    {
        var chunkSize = maxChunkSize ?? DefaultChunkSize;
        var chunks = new List<DocumentChunk>();

        // Split by chapters first (if present)
        var chapterSections = SplitByChapters(documentContent);

        foreach (var chapter in chapterSections)
        {
            if (chapter.Length <= chunkSize)
            {
                // Chapter fits in one chunk
                chunks.Add(new DocumentChunk
                {
                    Id = Guid.NewGuid().ToString(),
                    DocumentId = documentId,
                    Content = chapter.Content,
                    ChapterNumber = chapter.ChapterNumber,
                    ChunkIndex = chunks.Count,
                    StartPosition = chapter.StartPosition,
                    EndPosition = chapter.EndPosition
                });
            }
            else
            {
                // Split chapter into smaller chunks
                var chapterChunks = SplitChapterIntoChunks(chapter, chunkSize, chunks.Count);
                chunks.AddRange(chapterChunks);
            }
        }

        _logger.LogInformation(
            "Chunked document {DocumentId} into {ChunkCount} chunks",
            documentId,
            chunks.Count);

        return chunks;
    }

    /// <summary>
    /// Splits document by chapters (looks for chapter markers)
    /// </summary>
    private List<ChapterSection> SplitByChapters(string content)
    {
        var chapters = new List<ChapterSection>();
        var lines = content.Split('\n');
        var currentChapter = new StringBuilder();
        var chapterNumber = 0;
        var startPosition = 0;

        foreach (var line in lines)
        {
            // Detect chapter markers (common patterns)
            if (IsChapterMarker(line))
            {
                if (currentChapter.Length > 0)
                {
                    chapters.Add(new ChapterSection
                    {
                        ChapterNumber = chapterNumber++,
                        Content = currentChapter.ToString(),
                        StartPosition = startPosition,
                        EndPosition = startPosition + currentChapter.Length
                    });
                    startPosition += currentChapter.Length;
                    currentChapter.Clear();
                }
            }

            currentChapter.AppendLine(line);
        }

        // Add final chapter
        if (currentChapter.Length > 0)
        {
            chapters.Add(new ChapterSection
            {
                ChapterNumber = chapterNumber,
                Content = currentChapter.ToString(),
                StartPosition = startPosition,
                EndPosition = startPosition + currentChapter.Length
            });
        }

        // If no chapters found, treat entire document as one chapter
        if (chapters.Count == 0)
        {
            chapters.Add(new ChapterSection
            {
                ChapterNumber = 0,
                Content = content,
                StartPosition = 0,
                EndPosition = content.Length
            });
        }

        return chapters;
    }

    /// <summary>
    /// Detects chapter markers in text
    /// </summary>
    private bool IsChapterMarker(string line)
    {
        var trimmed = line.Trim();
        return trimmed.StartsWith("Chapter ", StringComparison.OrdinalIgnoreCase) ||
               trimmed.StartsWith("CHAPTER ", StringComparison.OrdinalIgnoreCase) ||
               trimmed.StartsWith("Part ", StringComparison.OrdinalIgnoreCase) ||
               trimmed.StartsWith("PART ", StringComparison.OrdinalIgnoreCase) ||
               (trimmed.Length < 50 && trimmed.All(c => char.IsUpper(c) || char.IsWhiteSpace(c)) && trimmed.Length > 5);
    }

    /// <summary>
    /// Splits a chapter into smaller chunks, preserving paragraph boundaries
    /// </summary>
    private List<DocumentChunk> SplitChapterIntoChunks(
        ChapterSection chapter,
        int chunkSize,
        int startIndex)
    {
        var chunks = new List<DocumentChunk>();
        var paragraphs = chapter.Content.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        var currentChunk = new StringBuilder();
        var chunkIndex = startIndex;
        var position = chapter.StartPosition;

        foreach (var paragraph in paragraphs)
        {
            // If adding this paragraph would exceed chunk size, finalize current chunk
            if (currentChunk.Length > 0 && 
                currentChunk.Length + paragraph.Length + 2 > chunkSize)
            {
                chunks.Add(new DocumentChunk
                {
                    Id = Guid.NewGuid().ToString(),
                    DocumentId = chapter.ChapterNumber.ToString(),
                    Content = currentChunk.ToString(),
                    ChapterNumber = chapter.ChapterNumber,
                    ChunkIndex = chunkIndex++,
                    StartPosition = position - currentChunk.Length,
                    EndPosition = position
                });

                // Start new chunk with overlap
                var overlap = GetOverlap(currentChunk.ToString(), ChunkOverlap);
                currentChunk.Clear();
                currentChunk.Append(overlap);
            }

            currentChunk.AppendLine(paragraph);
            position += paragraph.Length + 2;
        }

        // Add final chunk
        if (currentChunk.Length > 0)
        {
            chunks.Add(new DocumentChunk
            {
                Id = Guid.NewGuid().ToString(),
                DocumentId = chapter.ChapterNumber.ToString(),
                Content = currentChunk.ToString(),
                ChapterNumber = chapter.ChapterNumber,
                ChunkIndex = chunkIndex,
                StartPosition = position - currentChunk.Length,
                EndPosition = position
            });
        }

        return chunks;
    }

    /// <summary>
    /// Gets overlap text from end of chunk for context preservation
    /// </summary>
    private string GetOverlap(string text, int overlapSize)
    {
        if (text.Length <= overlapSize)
            return text;

        // Get last N characters, but try to end at sentence boundary
        var overlap = text.Substring(text.Length - overlapSize);
        var lastSentenceEnd = Math.Max(
            overlap.LastIndexOf('.'),
            Math.Max(
                overlap.LastIndexOf('!'),
                overlap.LastIndexOf('?')));

        if (lastSentenceEnd > overlapSize / 2)
        {
            return overlap.Substring(lastSentenceEnd + 1);
        }

        return overlap;
    }

    /// <summary>
    /// Estimates token count for a text (rough approximation: 1 token ≈ 4 characters)
    /// </summary>
    public int EstimateTokens(string text)
    {
        return (int)(text.Length / 4.0);
    }
}

public class DocumentChunk
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DocumentId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int ChapterNumber { get; set; }
    public int ChunkIndex { get; set; }
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

internal class ChapterSection
{
    public int ChapterNumber { get; set; }
    public string Content { get; set; } = string.Empty;
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
}
