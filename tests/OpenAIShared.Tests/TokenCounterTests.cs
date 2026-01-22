using FluentAssertions;
using OpenAIShared;
using Xunit;

namespace OpenAIShared.Tests;

public class TokenCounterTests
{
    [Fact]
    public void EstimateTokenCount_WithSimpleText_ReturnsApproximateCount()
    {
        // Arrange
        var text = "Hello, world!";

        // Act
        var count = TokenCounter.EstimateTokenCount(text);

        // Assert
        count.Should().BeGreaterThan(0);
        count.Should().BeLessOrEqualTo(text.Length); // Rough approximation
    }

    [Fact]
    public void EstimateTokenCount_WithEmptyString_ReturnsZero()
    {
        // Arrange
        var text = string.Empty;

        // Act
        var count = TokenCounter.EstimateTokenCount(text);

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void EstimateTokenCount_WithNull_ReturnsZero()
    {
        // Arrange
        string? text = null;

        // Act
        var count = TokenCounter.EstimateTokenCount(text!);

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void EstimateTokenCount_WithLongText_ReturnsReasonableCount()
    {
        // Arrange
        var text = string.Join(" ", Enumerable.Repeat("Hello world", 100));

        // Act
        var count = TokenCounter.EstimateTokenCount(text);

        // Assert
        count.Should().BeGreaterThan(100);
        count.Should().BeLessThan(text.Length);
    }

    [Fact]
    public void EstimateTokenCount_WithCode_HandlesSpecialCharacters()
    {
        // Arrange
        var code = "public class Test { public void Method() { } }";

        // Act
        var count = TokenCounter.EstimateTokenCount(code);

        // Assert
        count.Should().BeGreaterThan(0);
    }
}
