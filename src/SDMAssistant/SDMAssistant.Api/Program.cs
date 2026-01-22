using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OpenAIShared;
using OpenAIShared.Configuration;
using RequirementsAssistant.Core;
using RetroAnalyzer.Core;
using SDMAssistant.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SDM Assistant API",
        Version = "v1",
        Description = "API for Software Development Manager workflows with Jira and Confluence integration"
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

// Register HTTP clients
builder.Services.AddHttpClient();

// Register services
builder.Services.AddScoped<RequirementsService>(sp =>
{
    var openAIClient = sp.GetRequiredService<OpenAIClient>();
    var logger = sp.GetRequiredService<ILogger<RequirementsService>>();
    return new RequirementsService(openAIClient, logger);
});

builder.Services.AddScoped<EnhancedJiraIntegration>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();
    var logger = sp.GetRequiredService<ILogger<EnhancedJiraIntegration>>();
    var config = builder.Configuration.GetSection("Jira");
    return new EnhancedJiraIntegration(
        httpClient,
        logger,
        config["BaseUrl"] ?? throw new InvalidOperationException("Jira:BaseUrl not configured"),
        config["Username"] ?? throw new InvalidOperationException("Jira:Username not configured"),
        config["ApiToken"] ?? throw new InvalidOperationException("Jira:ApiToken not configured")
    );
});

builder.Services.AddScoped<EnhancedConfluenceIntegration>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();
    var logger = sp.GetRequiredService<ILogger<EnhancedConfluenceIntegration>>();
    var config = builder.Configuration.GetSection("Confluence");
    var requirementsService = sp.GetRequiredService<RequirementsService>();
    
    // Create base ConfluenceIntegration
    var baseConfluence = new ConfluenceIntegration(
        requirementsService,
        httpClient,
        sp.GetRequiredService<ILogger<ConfluenceIntegration>>(),
        config["BaseUrl"] ?? throw new InvalidOperationException("Confluence:BaseUrl not configured"),
        config["Username"] ?? throw new InvalidOperationException("Confluence:Username not configured"),
        config["ApiToken"] ?? throw new InvalidOperationException("Confluence:ApiToken not configured")
    );
    
    return new EnhancedConfluenceIntegration(
        baseConfluence,
        httpClient,
        logger,
        config["BaseUrl"]!,
        config["Username"]!,
        config["ApiToken"]!
    );
});

builder.Services.AddScoped<SDMService>(sp =>
{
    var openAIClient = sp.GetRequiredService<OpenAIClient>();
    var jiraIntegration = sp.GetRequiredService<EnhancedJiraIntegration>();
    var confluenceIntegration = sp.GetRequiredService<EnhancedConfluenceIntegration>();
    var logger = sp.GetRequiredService<ILogger<SDMService>>();
    return new SDMService(openAIClient, jiraIntegration, confluenceIntegration, logger);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
