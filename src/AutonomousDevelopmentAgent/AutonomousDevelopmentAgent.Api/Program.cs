using AutonomousDevelopmentAgent.Core;
using Microsoft.OpenApi.Models;
using OpenAIShared;
using Shared.Common;
using Shared.Integrations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Autonomous Development Agent API",
        Version = "v1",
        Description = "API for autonomous code analysis, improvement, and PR creation"
    });
});

builder.Services.AddOpenAIServices(builder.Configuration);
builder.Services.AddCommonServices(builder.Configuration);

// Register GitHub integration if token is provided
var githubToken = builder.Configuration["GitHub:Token"];
if (!string.IsNullOrEmpty(githubToken))
{
    builder.Services.AddHttpClient<DevOpsAssistant.Core.GitHubIntegration>((serviceProvider, client) =>
    {
        client.BaseAddress = new Uri("https://api.github.com");
        client.DefaultRequestHeaders.Add("User-Agent", "AutonomousDevelopmentAgent");
        client.DefaultRequestHeaders.Add("Authorization", $"token {githubToken}");
    });
    builder.Services.AddScoped<DevOpsAssistant.Core.GitHubIntegration>(sp =>
    {
        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(nameof(DevOpsAssistant.Core.GitHubIntegration));
        var logger = sp.GetRequiredService<ILogger<DevOpsAssistant.Core.GitHubIntegration>>();
        return new DevOpsAssistant.Core.GitHubIntegration(httpClient, logger, githubToken!);
    });
}

// Register Slack integration if webhook is provided
var slackWebhook = builder.Configuration["Slack:WebhookUrl"];
if (!string.IsNullOrEmpty(slackWebhook))
{
    builder.Services.AddScoped<SlackIntegration>(sp =>
    {
        var logger = sp.GetRequiredService<ILogger<SlackIntegration>>();
        return new SlackIntegration(slackWebhook!, logger);
    });
}

builder.Services.AddScoped<AutonomousDevelopmentAgent>(sp =>
{
    var openAIClient = sp.GetRequiredService<OpenAIClient>();
    var logger = sp.GetRequiredService<ILogger<AutonomousDevelopmentAgent>>();
    var githubIntegration = sp.GetService<DevOpsAssistant.Core.GitHubIntegration>();
    var slackIntegration = sp.GetService<SlackIntegration>();
    var model = builder.Configuration["OpenAI:DefaultModel"] ?? "gpt-4-turbo-preview";
    return new AutonomousDevelopmentAgent(openAIClient, logger, githubIntegration, slackIntegration, model);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCommonMiddleware();
app.MapControllers();

app.Run();
