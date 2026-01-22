using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Common;

/// <summary>
/// Extension methods for configuring Program.cs with common middleware
/// </summary>
public static class ProgramExtensions
{
    /// <summary>
    /// Adds all common middleware to the application pipeline
    /// </summary>
    public static WebApplication UseCommonMiddleware(this WebApplication app)
    {
        // Correlation IDs (first)
        app.UseCorrelationId();

        // Request/Response logging
        app.UseRequestResponseLogging();

        // Response compression
        app.UseResponseCompression();

        // CORS
        app.UseOpenAICors();

        // Rate limiting
        app.UseRateLimiting();

        // HTTPS redirection
        app.UseHttpsRedirection();

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Health checks
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        return app;
    }

    /// <summary>
    /// Adds all common services
    /// </summary>
    public static IServiceCollection AddCommonServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Health checks
        services.AddOpenAIHealthChecks();

        // CORS
        services.AddOpenAICors(configuration);

        return services;
    }
}
