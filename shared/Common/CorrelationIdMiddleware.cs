using System.Diagnostics;

namespace Shared.Common;

/// <summary>
/// Middleware to add correlation IDs to requests for tracking
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private const string CorrelationIdProperty = "CorrelationId";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get or create correlation ID
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault() 
                           ?? Guid.NewGuid().ToString();

        // Add to response headers
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        // Add to trace context
        Activity.Current?.SetTag(CorrelationIdProperty, correlationId);

        // Store in HttpContext items for access in controllers/services
        context.Items[CorrelationIdProperty] = correlationId;

        await _next(context);
    }
}

/// <summary>
/// Extension methods for correlation ID support
/// </summary>
public static class CorrelationIdExtensions
{
    /// <summary>
    /// Gets the correlation ID from HttpContext
    /// </summary>
    public static string? GetCorrelationId(this HttpContext context)
    {
        return context.Items.TryGetValue("CorrelationId", out var value) 
            ? value?.ToString() 
            : null;
    }

    /// <summary>
    /// Adds correlation ID middleware to the pipeline
    /// </summary>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
