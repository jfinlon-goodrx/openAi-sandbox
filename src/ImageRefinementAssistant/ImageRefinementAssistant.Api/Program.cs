using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OpenAIShared;
using OpenAIShared.Configuration;
using ImageRefinementAssistant.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Image Refinement Assistant API",
        Version = "v1",
        Description = "API for generating and refining images using DALL-E and GPT-4 Vision. " +
                      "Generates multiple images from prompts, evaluates them systematically, " +
                      "and returns the top candidates for human selection."
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

// Register OpenAI services (includes VisionService)
builder.Services.AddOpenAIServices(builder.Configuration);

// Register Image Evaluation Service
builder.Services.AddScoped<ImageEvaluationService>(sp =>
{
    var visionService = sp.GetRequiredService<VisionService>();
    var logger = sp.GetRequiredService<ILogger<ImageEvaluationService>>();
    return new ImageEvaluationService(visionService, logger);
});

// Register Image Refinement Service
builder.Services.AddScoped<ImageRefinementService>(sp =>
{
    var openAIClient = sp.GetRequiredService<OpenAIClient>();
    var evaluationService = sp.GetRequiredService<ImageEvaluationService>();
    var logger = sp.GetRequiredService<ILogger<ImageRefinementService>>();
    return new ImageRefinementService(openAIClient, evaluationService, logger);
});

// Register Image Storage Service
builder.Services.AddScoped<ImageStorageService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<ImageStorageService>>();
    var storagePath = builder.Configuration["ImageRefinement:StoragePath"];
    return new ImageStorageService(logger, storagePath);
});

// Register ZIP Export Service (singleton to manage temp files)
builder.Services.AddSingleton<ZipExportService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<ZipExportService>>();
    return new ZipExportService(logger);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors();

app.MapControllers();

app.Run();
