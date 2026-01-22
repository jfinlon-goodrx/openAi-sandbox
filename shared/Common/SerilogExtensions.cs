using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Shared.Common;

/// <summary>
/// Serilog configuration extensions
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// Configures Serilog for structured logging
    /// </summary>
    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
            .WriteTo.Console(new CompactJsonFormatter())
            .WriteTo.File(
                new CompactJsonFormatter(),
                "logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();

        builder.Host.UseSerilog();

        return builder;
    }

    /// <summary>
    /// Adds Serilog request logging middleware
    /// </summary>
    public static WebApplication UseSerilogRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.GetLevel = (httpContext, elapsed, ex) => ex != null
                ? LogEventLevel.Error
                : httpContext.Response.StatusCode > 499
                    ? LogEventLevel.Error
                    : LogEventLevel.Information;
            options.EnrichFromRequest = (httpContext, logEvent) =>
            {
                logEvent.SetProperty("RequestHost", httpContext.Request.Host.Value);
                logEvent.SetProperty("RequestScheme", httpContext.Request.Scheme);
                if (httpContext.Items.TryGetValue("CorrelationId", out var correlationId))
                {
                    logEvent.SetProperty("CorrelationId", correlationId);
                }
            };
        });

        return app;
    }
}
