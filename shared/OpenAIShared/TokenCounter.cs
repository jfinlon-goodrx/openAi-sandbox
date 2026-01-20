using System.Text;

namespace OpenAIShared;

/// <summary>
/// Provides token counting functionality for OpenAI models
/// Uses a simple approximation: ~4 characters per token
/// For production, consider using tiktoken library
/// </summary>
public static class TokenCounter
{
    private const double AverageCharsPerToken = 4.0;

    /// <summary>
    /// Estimates the number of tokens in a text string
    /// </summary>
    public static int EstimateTokenCount(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        // Simple approximation: divide character count by average chars per token
        return (int)Math.Ceiling(text.Length / AverageCharsPerToken);
    }

    /// <summary>
    /// Estimates the number of tokens in multiple text strings
    /// </summary>
    public static int EstimateTokenCount(params string[] texts)
    {
        return texts.Sum(EstimateTokenCount);
    }

    /// <summary>
    /// Estimates the number of tokens in a collection of messages
    /// </summary>
    public static int EstimateTokenCount(IEnumerable<ChatMessage> messages)
    {
        return messages.Sum(m => EstimateTokenCount(m.Content ?? string.Empty));
    }
}

/// <summary>
/// Represents a chat message
/// </summary>
public class ChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
