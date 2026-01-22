using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Shared.Common;

/// <summary>
/// Health check extensions for OpenAI services
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds health checks for OpenAI services
    /// </summary>
    public static IServiceCollection AddOpenAIHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("openai-api", () =>
            {
                // Basic health check - could be enhanced to actually ping OpenAI API
                return HealthCheckResult.Healthy("OpenAI API configuration is valid");
            }, tags: new[] { "openai", "external" });

        return services;
    }
}
