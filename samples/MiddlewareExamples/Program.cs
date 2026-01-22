using Microsoft.OpenApi.Models;
using OpenAIShared;
using Shared.Common;
using RequirementsAssistant.Core;

namespace Samples.MiddlewareExamples;

/// <summary>
/// Example Program.cs showing how to use all the new middleware and features
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Requirements Assistant API",
                Version = "v1",
                Description = "API demonstrating middleware and improvements"
            });
        });

        // Configure OpenAI
        builder.Services.AddOpenAIServices(builder.Configuration);

        // Register application services
        builder.Services.AddScoped<RequirementsService>(sp =>
        {
            var openAIClient = sp.GetRequiredService<OpenAIClient>();
            var logger = sp.GetRequiredService<ILogger<RequirementsService>>();
            var model = builder.Configuration["OpenAI:DefaultModel"] ?? "gpt-4-turbo-preview";
            return new RequirementsService(openAIClient, logger, model);
        });

        // Add health checks
        builder.Services.AddOpenAIHealthChecks();

        // Add CORS
        builder.Services.AddOpenAICors(builder.Configuration);

        // Add authentication
        builder.Services.AddAuthentication("ApiKey")
            .AddApiKeyAuthentication(builder.Configuration);
        builder.Services.AddAuthorization();

        var app = builder.Build();

        // Configure HTTP pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Middleware pipeline (order matters!)
        app.UseCorrelationId();              // 1. Add correlation IDs
        app.UseRequestResponseLogging();      // 2. Log requests/responses
        app.UseResponseCompression();         // 3. Compress responses
        app.UseOpenAICors();                  // 4. CORS
        app.UseRateLimiting(options =>        // 5. Rate limiting
        {
            options.MaxRequests = 100;
            options.WindowSeconds = 60;
        });
        app.UseHttpsRedirection();
        app.UseAuthentication();              // 6. Authentication
        app.UseAuthorization();               // 7. Authorization

        // Health checks
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        // Controllers
        app.MapControllers();

        app.Run();
    }
}
