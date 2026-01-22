using System.Collections.Concurrent;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Shared.Common;

/// <summary>
/// Simple rate limiting middleware using token bucket algorithm
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitOptions _options;
    private readonly ConcurrentDictionary<string, TokenBucket> _buckets = new();

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        RateLimitOptions options)
    {
        _next = next;
        _logger = logger;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var key = GetRateLimitKey(context);

        if (!string.IsNullOrEmpty(key))
        {
            var bucket = _buckets.GetOrAdd(key, _ => new TokenBucket(_options.MaxRequests, _options.WindowSeconds));

            if (!bucket.TryConsume())
            {
                _logger.LogWarning(
                    "Rate limit exceeded for key: {Key}, Requests: {Requests}/{MaxRequests}",
                    key,
                    bucket.CurrentTokens,
                    _options.MaxRequests);

                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers["X-RateLimit-Limit"] = _options.MaxRequests.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = "0";
                context.Response.Headers["Retry-After"] = bucket.SecondsUntilRefill.ToString();

                await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                return;
            }

            // Add rate limit headers
            context.Response.Headers["X-RateLimit-Limit"] = _options.MaxRequests.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = bucket.CurrentTokens.ToString();
        }

        await _next(context);
    }

    private string GetRateLimitKey(HttpContext context)
    {
        // Can be based on IP, API key, user ID, etc.
        return _options.KeySelector?.Invoke(context) ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
    }
}

/// <summary>
/// Token bucket implementation for rate limiting
/// </summary>
public class TokenBucket
{
    private readonly int _maxTokens;
    private readonly int _refillRate; // tokens per second
    private int _tokens;
    private DateTime _lastRefill;

    public TokenBucket(int maxTokens, int windowSeconds)
    {
        _maxTokens = maxTokens;
        _refillRate = maxTokens / windowSeconds;
        _tokens = maxTokens;
        _lastRefill = DateTime.UtcNow;
    }

    public bool TryConsume()
    {
        Refill();
        
        if (_tokens > 0)
        {
            _tokens--;
            return true;
        }

        return false;
    }

    public int CurrentTokens
    {
        get
        {
            Refill();
            return _tokens;
        }
    }

    public int SecondsUntilRefill
    {
        get
        {
            if (_tokens > 0) return 0;
            var elapsed = (DateTime.UtcNow - _lastRefill).TotalSeconds;
            return Math.Max(0, (int)Math.Ceiling(1.0 / _refillRate - elapsed));
        }
    }

    private void Refill()
    {
        var now = DateTime.UtcNow;
        var elapsed = (now - _lastRefill).TotalSeconds;
        
        if (elapsed > 0)
        {
            var tokensToAdd = (int)(elapsed * _refillRate);
            _tokens = Math.Min(_maxTokens, _tokens + tokensToAdd);
            _lastRefill = now;
        }
    }
}

/// <summary>
/// Rate limiting options
/// </summary>
public class RateLimitOptions
{
    public int MaxRequests { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
    public Func<HttpContext, string>? KeySelector { get; set; }
}

/// <summary>
/// Extension methods for rate limiting
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>
    /// Adds rate limiting middleware to the pipeline
    /// </summary>
    public static IApplicationBuilder UseRateLimiting(
        this IApplicationBuilder app,
        Action<RateLimitOptions>? configure = null)
    {
        var options = new RateLimitOptions();
        configure?.Invoke(options);

        return app.UseMiddleware<RateLimitingMiddleware>(options);
    }
}
