using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace OpenAIShared;

/// <summary>
/// Caching service for OpenAI API responses to reduce costs and improve performance
/// </summary>
public class CachingService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachingService> _logger;
    private readonly TimeSpan _defaultCacheDuration = TimeSpan.FromHours(24);

    public CachingService(
        IMemoryCache cache,
        ILogger<CachingService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Gets a cached response or executes the function and caches the result
    /// </summary>
    public async Task<T> GetOrSetAsync<T>(
        string cacheKey,
        Func<Task<T>> factory,
        TimeSpan? cacheDuration = null)
    {
        if (_cache.TryGetValue(cacheKey, out T? cachedValue) && cachedValue != null)
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return cachedValue;
        }

        _logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);
        var value = await factory();

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = cacheDuration ?? _defaultCacheDuration,
            SlidingExpiration = TimeSpan.FromHours(1)
        };

        _cache.Set(cacheKey, value, options);
        return value;
    }

    /// <summary>
    /// Generates a cache key from a prompt
    /// </summary>
    public static string GenerateCacheKey(string prompt, string model)
    {
        // Simple hash-based key generation
        var hash = prompt.GetHashCode();
        return $"openai:{model}:{hash}";
    }

    /// <summary>
    /// Invalidates cache entries matching a pattern
    /// </summary>
    public void Invalidate(string pattern)
    {
        // Note: IMemoryCache doesn't support pattern-based invalidation
        // In production, consider using Redis or a more advanced cache
        _logger.LogWarning("Pattern-based cache invalidation not fully supported with IMemoryCache. Consider using Redis.");
    }

    /// <summary>
    /// Clears all cache entries
    /// </summary>
    public void Clear()
    {
        if (_cache is MemoryCache memoryCache)
        {
            // MemoryCache doesn't have a Clear method, so we'd need to track keys
            _logger.LogWarning("Full cache clear not directly supported. Consider tracking keys or using Redis.");
        }
    }
}
