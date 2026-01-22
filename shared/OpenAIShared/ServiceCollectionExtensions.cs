using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

        // Register additional services
        services.AddHttpClient<VisionService>((serviceProvider, client) =>
        {
            var config = serviceProvider.GetRequiredService<IOptions<OpenAIConfiguration>>().Value;
            client.BaseAddress = new Uri(config.BaseUrl);
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.ApiKey);
            client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
        });
        services.AddScoped<VisionService>(serviceProvider =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(VisionService));
            var config = serviceProvider.GetRequiredService<IOptions<OpenAIConfiguration>>();
            var logger = serviceProvider.GetRequiredService<ILogger<VisionService>>();
            return new VisionService(httpClient, config, logger);
        });

        services.AddHttpClient<ModerationService>((serviceProvider, client) =>
        {
            var config = serviceProvider.GetRequiredService<IOptions<OpenAIConfiguration>>().Value;
            client.BaseAddress = new Uri(config.BaseUrl);
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.ApiKey);
            client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
        });
        services.AddScoped<ModerationService>(serviceProvider =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(ModerationService));
            var config = serviceProvider.GetRequiredService<IOptions<OpenAIConfiguration>>();
            var logger = serviceProvider.GetRequiredService<ILogger<ModerationService>>();
            return new ModerationService(httpClient, config, logger);
        });

        services.AddHttpClient<BatchService>((serviceProvider, client) =>
        {
            var config = serviceProvider.GetRequiredService<IOptions<OpenAIConfiguration>>().Value;
            client.BaseAddress = new Uri(config.BaseUrl);
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.ApiKey);
            client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
        });
        services.AddScoped<BatchService>(serviceProvider =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(BatchService));
            var config = serviceProvider.GetRequiredService<IOptions<OpenAIConfiguration>>();
            var logger = serviceProvider.GetRequiredService<ILogger<BatchService>>();
            return new BatchService(httpClient, config, logger);
        });

        // RAG Service depends on OpenAIClient
        services.AddScoped<RAGService>(serviceProvider =>
        {
            var openAIClient = serviceProvider.GetRequiredService<OpenAIClient>();
            var logger = serviceProvider.GetRequiredService<ILogger<RAGService>>();
            return new RAGService(openAIClient, logger);
        });

        return services;
    }
}
