using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OpenAIShared;
using OpenAIShared.Configuration;
using PublishingAssistant.Core;

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

// Add Vision Service
builder.Services.AddHttpClient<VisionService>((serviceProvider, client) =>
{
    var config = serviceProvider.GetRequiredService<IOptions<OpenAIConfiguration>>().Value;
    client.BaseAddress = new Uri(config.BaseUrl);
    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", config.ApiKey);
});
builder.Services.AddScoped<VisionService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient(nameof(VisionService));
    var config = sp.GetRequiredService<IOptions<OpenAIConfiguration>>();
    var logger = sp.GetRequiredService<ILogger<VisionService>>();
    return new VisionService(httpClient, config, logger);
});

builder.Services.AddScoped<PublishingService>(sp =>
{
    var openAIClient = sp.GetRequiredService<OpenAIClient>();
    var logger = sp.GetRequiredService<ILogger<PublishingService>>();
    var model = builder.Configuration["OpenAI:DefaultModel"] ?? "gpt-4-turbo-preview";
    return new PublishingService(openAIClient, logger, model);
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

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
