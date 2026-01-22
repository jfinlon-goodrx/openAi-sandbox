using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OpenAIShared;
using OpenAIShared.Tests.TestHelpers;
using Xunit;

namespace OpenAIShared.Tests;

public class VisionServiceTests
{
    private readonly Mock<ILogger<VisionService>> _mockLogger;
    private readonly OpenAIConfiguration _config;

    public VisionServiceTests()
    {
        _mockLogger = new Mock<ILogger<VisionService>>();
        _config = new OpenAIConfiguration
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.openai.com/v1/"
        };
    }

    [Fact]
    public async Task AnalyzeImageAsync_WithValidImageUrl_ReturnsAnalysis()
    {
        // Arrange
        var responseJson = OpenAIMockHelper.CreateChatCompletionResponse("This is a test image showing a cat.");
        var httpClient = OpenAIMockHelper.CreateMockHttpClient(responseJson);
        var service = new VisionService(httpClient, _mockLogger.Object, _config);

        // Act
        var result = await service.AnalyzeImageAsync("https://example.com/image.jpg", "What is in this image?");

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("cat");
    }

    [Fact]
    public async Task AnalyzeImageAsync_WithBase64Image_ReturnsAnalysis()
    {
        // Arrange
        var responseJson = OpenAIMockHelper.CreateChatCompletionResponse("Image analysis result");
        var httpClient = OpenAIMockHelper.CreateMockHttpClient(responseJson);
        var service = new VisionService(httpClient, _mockLogger.Object, _config);
        var base64Image = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("fake image data"));

        // Act
        var result = await service.AnalyzeImageFromBase64Async(base64Image, "Describe this image");

        // Assert
        result.Should().NotBeNullOrEmpty();
    }
}
