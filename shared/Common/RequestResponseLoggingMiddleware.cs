using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Shared.Common;

/// <summary>
/// Middleware to log HTTP requests and responses
/// </summary>
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.GetCorrelationId() ?? "unknown";
        var stopwatch = Stopwatch.StartNew();

        // Log request
        await LogRequestAsync(context, correlationId);

        // Capture response body
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
        }
        finally
        {
            // Log response
            await LogResponseAsync(context, correlationId, stopwatch.ElapsedMilliseconds);

            // Copy response back to original stream
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private async Task LogRequestAsync(HttpContext context, string correlationId)
    {
        var request = context.Request;

        _logger.LogInformation(
            "HTTP Request - Method: {Method}, Path: {Path}, Query: {QueryString}, CorrelationId: {CorrelationId}",
            request.Method,
            request.Path,
            request.QueryString,
            correlationId);

        // Log request body for non-GET requests (be careful with large bodies)
        if (request.Method != "GET" && request.ContentLength > 0 && request.ContentLength < 1024)
        {
            request.EnableBuffering();
            var buffer = new byte[request.ContentLength.Value];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            request.Body.Position = 0;

            var bodyAsText = Encoding.UTF8.GetString(buffer);
            _logger.LogDebug("Request Body: {Body}", bodyAsText);
        }
    }

    private async Task LogResponseAsync(HttpContext context, string correlationId, long durationMs)
    {
        var response = context.Response;

        // Read response body
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        _logger.LogInformation(
            "HTTP Response - Status: {StatusCode}, Duration: {Duration}ms, CorrelationId: {CorrelationId}",
            response.StatusCode,
            durationMs,
            correlationId);

        if (!string.IsNullOrEmpty(responseBody) && responseBody.Length < 1024)
        {
            _logger.LogDebug("Response Body: {Body}", responseBody);
        }
    }
}

/// <summary>
/// Extension methods for request/response logging
/// </summary>
public static class RequestResponseLoggingExtensions
{
    /// <summary>
    /// Adds request/response logging middleware to the pipeline
    /// </summary>
    public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestResponseLoggingMiddleware>();
    }
}
