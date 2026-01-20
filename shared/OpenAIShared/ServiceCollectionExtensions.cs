using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAIShared.Configuration;

namespace OpenAIShared;

/// <summary>
/// Extension methods for registering OpenAI services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds OpenAI client services to the service collection
    /// </summary>
    public static IServiceCollection AddOpenAIServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind configuration
        services.Configure<OpenAIConfiguration>(
            configuration.GetSection(OpenAIConfiguration.SectionName));

        // Register HTTP client with retry policy
        services.AddHttpClient<OpenAIClient>((serviceProvider, client) =>
        {
            var config = serviceProvider.GetRequiredService<IOptions<OpenAIConfiguration>>().Value;
            client.BaseAddress = new Uri(config.BaseUrl);
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.ApiKey);
            client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
        });

        // Register the client - will be resolved via HttpClient factory
        services.AddScoped<OpenAIClient>(serviceProvider =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(OpenAIClient));
            var config = serviceProvider.GetRequiredService<IOptions<OpenAIConfiguration>>();
            var logger = serviceProvider.GetRequiredService<ILogger<OpenAIClient>>();
            return new OpenAIClient(httpClient, config, logger);
        });

        return services;
    }
}
