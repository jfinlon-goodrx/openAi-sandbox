using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OpenAIShared;
using OpenAIShared.Configuration;
using PublishingAssistant.Core;
using Shared.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Publishing Assistant API",
        Version = "v1",
        Description = "API for publishing workflows: reviews, summaries, marketing blurbs, and cover image generation"
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

builder.Services.AddScoped<PublishingService>(sp =>
{
    var openAIClient = sp.GetRequiredService<OpenAIClient>();
    var logger = sp.GetRequiredService<ILogger<PublishingService>>();
    var model = builder.Configuration["OpenAI:DefaultModel"] ?? "gpt-4-turbo-preview";
    return new PublishingService(openAIClient, logger, model);
});

// PDF Processing Service
builder.Services.AddSingleton<PDFProcessingService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<PDFProcessingService>>();
    var storagePath = builder.Configuration["Publishing:StoragePath"];
    return new PDFProcessingService(logger, storagePath);
});

// Document Chunking Service
builder.Services.AddScoped<DocumentChunkingService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<DocumentChunkingService>>();
    return new DocumentChunkingService(logger);
});

// Senior Publishing Agent Service
builder.Services.AddScoped<SeniorPublishingAgentService>(sp =>
{
    var openAIClient = sp.GetRequiredService<OpenAIClient>();
    var ragService = sp.GetRequiredService<RAGService>();
    var chunkingService = sp.GetRequiredService<DocumentChunkingService>();
    var logger = sp.GetRequiredService<ILogger<SeniorPublishingAgentService>>();
    var model = builder.Configuration["OpenAI:DefaultModel"] ?? "gpt-4-turbo-preview";
    return new SeniorPublishingAgentService(openAIClient, ragService, chunkingService, logger, model);
});
builder.Services.AddScoped<CoverImageGenerator>(sp =>
{
    var openAIClient = sp.GetRequiredService<OpenAIClient>();
    var logger = sp.GetRequiredService<ILogger<CoverImageGenerator>>();
    return new CoverImageGenerator(openAIClient, logger);
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
