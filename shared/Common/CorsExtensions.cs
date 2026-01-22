namespace Shared.Common;

/// <summary>
/// CORS configuration extensions
/// </summary>
public static class CorsExtensions
{
    /// <summary>
    /// Adds CORS configuration for OpenAI Platform projects
    /// </summary>
    public static IServiceCollection AddOpenAICors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                            ?? new[] { "http://localhost:3000", "http://localhost:5000", "http://localhost:5001" };

        services.AddCors(options =>
        {
            options.AddPolicy("OpenAIPlatform", builder =>
            {
                builder
                    .WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// Uses CORS middleware
    /// </summary>
    public static IApplicationBuilder UseOpenAICors(this IApplicationBuilder app)
    {
        return app.UseCors("OpenAIPlatform");
    }
}
