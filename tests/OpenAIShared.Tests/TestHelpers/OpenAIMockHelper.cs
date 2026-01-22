using System.Net;
using System.Text;
using System.Text.Json;
using Moq;
using Moq.Protected;
using OpenAIShared;

namespace OpenAIShared.Tests.TestHelpers;

/// <summary>
/// Helper class for mocking OpenAI API responses in tests
/// </summary>
public static class OpenAIMockHelper
{
    /// <summary>
    /// Creates a mock HttpClient that returns a successful chat completion response
    /// </summary>
    public static HttpClient CreateMockHttpClient(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            });

        return new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://api.openai.com/v1/")
        };
    }

    /// <summary>
    /// Creates a mock chat completion response JSON
    /// </summary>
    public static string CreateChatCompletionResponse(string content, string model = "gpt-4-turbo-preview", int totalTokens = 100)
    {
        var response = new
        {
            id = "chatcmpl-123",
            @object = "chat.completion",
            created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            model = model,
            choices = new[]
            {
                new
                {
                    index = 0,
                    message = new
                    {
                        role = "assistant",
                        content = content
                    },
                    finish_reason = "stop"
                }
            },
            usage = new
            {
                prompt_tokens = 50,
                completion_tokens = 50,
                total_tokens = totalTokens
            }
        };

        return JsonSerializer.Serialize(response);
    }

    /// <summary>
    /// Creates a mock embedding response JSON
    /// </summary>
    public static string CreateEmbeddingResponse(float[] embedding, string model = "text-embedding-ada-002")
    {
        var response = new
        {
            @object = "list",
            data = new[]
            {
                new
                {
                    @object = "embedding",
                    embedding = embedding,
                    index = 0
                }
            },
            model = model,
            usage = new
            {
                prompt_tokens = 10,
                total_tokens = 10
            }
        };

        return JsonSerializer.Serialize(response);
    }

    /// <summary>
    /// Creates a mock transcription response JSON
    /// </summary>
    public static string CreateTranscriptionResponse(string text, string language = "en")
    {
        var response = new
        {
            text = text,
            language = language
        };

        return JsonSerializer.Serialize(response);
    }

    /// <summary>
    /// Creates a mock rate limit error response
    /// </summary>
    public static string CreateRateLimitErrorResponse()
    {
        var error = new
        {
            error = new
            {
                message = "Rate limit exceeded",
                type = "rate_limit_error",
                code = "rate_limit_exceeded"
            }
        };

        return JsonSerializer.Serialize(error);
    }

    /// <summary>
    /// Creates a mock authentication error response
    /// </summary>
    public static string CreateAuthErrorResponse()
    {
        var error = new
        {
            error = new
            {
                message = "Incorrect API key provided",
                type = "invalid_request_error",
                code = "invalid_api_key"
            }
        };

        return JsonSerializer.Serialize(error);
    }
}
