using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Data;

/// <summary>
/// Extension methods for database configuration
/// </summary>
public static class DataExtensions
{
    /// <summary>
    /// Adds Entity Framework Core with SQL Server
    /// </summary>
    public static IServiceCollection AddApplicationDbContext(
        this IServiceCollection services,
        IConfiguration configuration,
        bool useInMemory = false)
    {
        if (useInMemory)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("OpenAIPlatformDb"));
        }
        else
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' not found.");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
        }

        return services;
    }

    /// <summary>
    /// Migrates database on startup (for development)
    /// </summary>
    public static WebApplication MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
        return app;
    }
}
