using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;

namespace OpenAIShared;

/// <summary>
/// Circuit breaker service for OpenAI API calls
/// Prevents cascading failures when OpenAI API is down
/// </summary>
public class CircuitBreakerService
{
    private readonly ILogger<CircuitBreakerService> _logger;
    private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreaker;

    public CircuitBreakerService(ILogger<CircuitBreakerService> logger)
    {
        _logger = logger;

        // Configure circuit breaker
        _circuitBreaker = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (result, duration) =>
                {
                    _logger.LogWarning(
                        "Circuit breaker opened. Will remain open for {Duration}s. Reason: {Reason}",
                        duration.TotalSeconds,
                        result.Exception?.Message ?? result.Result?.StatusCode.ToString());
                },
                onReset: () =>
                {
                    _logger.LogInformation("Circuit breaker reset. Requests will flow through.");
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation("Circuit breaker half-open. Testing connection...");
                });
    }

    /// <summary>
    /// Executes an async operation with circuit breaker protection
    /// </summary>
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        try
        {
            return await _circuitBreaker.ExecuteAsync(operation);
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogError(ex, "Circuit breaker is open. Request rejected.");
            throw new InvalidOperationException("Service temporarily unavailable. Please try again later.", ex);
        }
    }

    /// <summary>
    /// Gets the current circuit breaker state
    /// </summary>
    public CircuitState State => _circuitBreaker.CircuitState;
}
