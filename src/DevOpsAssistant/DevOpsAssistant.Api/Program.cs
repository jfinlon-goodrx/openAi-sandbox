using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using OpenAIShared;
using DevOpsAssistant.Core;
using Shared.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DevOps Assistant API",
        Version = "v1",
        Description = "API for DevOps workflows: log analysis, CI/CD optimization, infrastructure review, and security scanning"
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddOpenAIServices(builder.Configuration);
builder.Services.AddCommonServices(builder.Configuration);

// Register GitHub integration if token is provided
var githubToken = builder.Configuration["GitHub:Token"];
if (!string.IsNullOrEmpty(githubToken))
{
    builder.Services.AddHttpClient<GitHubIntegration>((serviceProvider, client) =>
    {
        client.BaseAddress = new Uri("https://api.github.com");
    });
    builder.Services.AddScoped<GitHubIntegration>(sp =>
    {
        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient(nameof(GitHubIntegration));
        var logger = sp.GetRequiredService<ILogger<GitHubIntegration>>();
        return new GitHubIntegration(httpClient, logger, githubToken!);
    });
}

builder.Services.AddScoped<DevOpsService>(sp =>
{
    var openAIClient = sp.GetRequiredService<OpenAIClient>();
    var logger = sp.GetRequiredService<ILogger<DevOpsService>>();
    var githubIntegration = sp.GetService<GitHubIntegration>();
    var model = builder.Configuration["OpenAI:DefaultModel"] ?? "gpt-4-turbo-preview";
    return new DevOpsService(openAIClient, logger, githubIntegration, model);
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
