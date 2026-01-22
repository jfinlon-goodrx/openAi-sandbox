using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OpenAIShared;
using OpenAIShared.Tests.TestHelpers;
using Xunit;

namespace OpenAIShared.Tests;

public class ModerationServiceTests
{
    private readonly Mock<ILogger<ModerationService>> _mockLogger;
    private readonly OpenAIConfiguration _config;

    public ModerationServiceTests()
    {
        _mockLogger = new Mock<ILogger<ModerationService>>();
        _config = new OpenAIConfiguration
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.openai.com/v1/"
        };
    }

    [Fact]
    public async Task ModerateContentAsync_WithSafeContent_ReturnsNotFlagged()
    {
        // Arrange
        var moderationResponse = new
        {
            id = "modr-123",
            model = "text-moderation-latest",
            results = new[]
            {
                new
                {
                    flagged = false,
                    categories = new { },
                    category_scores = new { }
                }
            }
        };
        var responseJson = JsonSerializer.Serialize(moderationResponse);
        var httpClient = OpenAIMockHelper.CreateMockHttpClient(responseJson);
        var service = new ModerationService(httpClient, _mockLogger.Object, _config);

        // Act
        var result = await service.ModerateContentAsync("This is safe content");

        // Assert
        result.Should().NotBeNull();
        result.Flagged.Should().BeFalse();
    }

    [Fact]
    public async Task ModerateContentAsync_WithUnsafeContent_ReturnsFlagged()
    {
        // Arrange
        var moderationResponse = new
        {
            id = "modr-123",
            model = "text-moderation-latest",
            results = new[]
            {
                new
                {
                    flagged = true,
                    categories = new { hate = true },
                    category_scores = new { hate = 0.9 }
                }
            }
        };
        var responseJson = JsonSerializer.Serialize(moderationResponse);
        var httpClient = OpenAIMockHelper.CreateMockHttpClient(responseJson);
        var service = new ModerationService(httpClient, _mockLogger.Object, _config);

        // Act
        var result = await service.ModerateContentAsync("Unsafe content");

        // Assert
        result.Should().NotBeNull();
        result.Flagged.Should().BeTrue();
    }
}
