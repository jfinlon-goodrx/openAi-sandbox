using System.IO.Compression;

namespace Shared.Common;

/// <summary>
/// Response compression middleware
/// </summary>
public class ResponseCompressionMiddleware
{
    private readonly RequestDelegate _next;

    public ResponseCompressionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var acceptEncoding = context.Request.Headers["Accept-Encoding"].ToString();

        if (acceptEncoding.Contains("gzip", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.Headers.Append("Content-Encoding", "gzip");
            var originalBodyStream = context.Response.Body;
            using var compressedStream = new GZipStream(originalBodyStream, CompressionLevel.Fastest);
            context.Response.Body = compressedStream;
            await _next(context);
            await compressedStream.FlushAsync();
        }
        else if (acceptEncoding.Contains("br", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.Headers.Append("Content-Encoding", "br");
            var originalBodyStream = context.Response.Body;
            using var compressedStream = new BrotliStream(originalBodyStream, CompressionLevel.Fastest);
            context.Response.Body = compressedStream;
            await _next(context);
            await compressedStream.FlushAsync();
        }
        else
        {
            await _next(context);
        }
    }
}

/// <summary>
/// Extension methods for response compression
/// </summary>
public static class ResponseCompressionExtensions
{
    /// <summary>
    /// Adds response compression middleware
    /// </summary>
    public static IApplicationBuilder UseResponseCompression(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ResponseCompressionMiddleware>();
    }
}
