using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OpenAIShared;
using OpenAIShared.Tests.TestHelpers;
using Xunit;

namespace OpenAIShared.Tests;

public class OpenAIClientTests
{
    private readonly Mock<ILogger<OpenAIClient>> _mockLogger;
    private readonly OpenAIConfiguration _config;

    public OpenAIClientTests()
    {
        _mockLogger = new Mock<ILogger<OpenAIClient>>();
        _config = new OpenAIConfiguration
        {
            ApiKey = "test-api-key",
            BaseUrl = "https://api.openai.com/v1/"
        };
    }

    [Fact]
    public async Task GetChatCompletionAsync_WithValidRequest_ReturnsResponse()
    {
        // Arrange
        var responseJson = OpenAIMockHelper.CreateChatCompletionResponse("Test response");
        var httpClient = OpenAIMockHelper.CreateMockHttpClient(responseJson);
        var client = new OpenAIClient(httpClient, _mockLogger.Object, _config);

        var request = new ChatCompletionRequest
        {
            Model = "gpt-4-turbo-preview",
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = "Hello" }
            }
        };

        // Act
        var response = await client.GetChatCompletionAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Choices.Should().NotBeEmpty();
        response.Choices.First().Message.Content.Should().Be("Test response");
    }

    [Fact]
    public async Task GetChatCompletionAsync_WithRateLimitError_ThrowsException()
    {
        // Arrange
        var errorJson = OpenAIMockHelper.CreateRateLimitErrorResponse();
        var httpClient = OpenAIMockHelper.CreateMockHttpClient(errorJson, HttpStatusCode.TooManyRequests);
        var client = new OpenAIClient(httpClient, _mockLogger.Object, _config);

        var request = new ChatCompletionRequest
        {
            Model = "gpt-4-turbo-preview",
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = "Hello" }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetChatCompletionAsync(request));
    }

    [Fact]
    public async Task GetChatCompletionAsync_WithAuthError_ThrowsException()
    {
        // Arrange
        var errorJson = OpenAIMockHelper.CreateAuthErrorResponse();
        var httpClient = OpenAIMockHelper.CreateMockHttpClient(errorJson, HttpStatusCode.Unauthorized);
        var client = new OpenAIClient(httpClient, _mockLogger.Object, _config);

        var request = new ChatCompletionRequest
        {
            Model = "gpt-4-turbo-preview",
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = "Hello" }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetChatCompletionAsync(request));
    }

    [Fact]
    public async Task GetEmbeddingsAsync_WithValidRequest_ReturnsEmbeddings()
    {
        // Arrange
        var embedding = new float[] { 0.1f, 0.2f, 0.3f };
        var responseJson = OpenAIMockHelper.CreateEmbeddingResponse(embedding);
        var httpClient = OpenAIMockHelper.CreateMockHttpClient(responseJson);
        var client = new OpenAIClient(httpClient, _mockLogger.Object, _config);

        var request = new EmbeddingRequest
        {
            Model = "text-embedding-ada-002",
            Input = "Test text"
        };

        // Act
        var response = await client.GetEmbeddingsAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Data.Should().NotBeEmpty();
        response.Data.First().Embedding.Should().BeEquivalentTo(embedding);
    }

    [Fact]
    public async Task TranscribeAudioAsync_WithValidRequest_ReturnsTranscript()
    {
        // Arrange
        var transcriptText = "This is a test transcription";
        var responseJson = OpenAIMockHelper.CreateTranscriptionResponse(transcriptText);
        var httpClient = OpenAIMockHelper.CreateMockHttpClient(responseJson);
        var client = new OpenAIClient(httpClient, _mockLogger.Object, _config);

        using var audioStream = new MemoryStream(Encoding.UTF8.GetBytes("fake audio data"));

        // Act
        var response = await client.TranscribeAudioAsync(audioStream, "test.mp3");

        // Assert
        response.Should().NotBeNull();
        response.Text.Should().Be(transcriptText);
    }
}
