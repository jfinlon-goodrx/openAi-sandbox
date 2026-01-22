using Microsoft.OpenApi.Models;
using OpenAIShared;
using AdvertisingAgency.Core;
using Shared.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Advertising Agency Assistant API",
        Version = "v1",
        Description = "API for advertising workflows: ad copy, campaign strategy, audience analysis, brand voice, and creative briefs"
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

builder.Services.AddScoped<AdvertisingService>(sp =>
{
    var openAIClient = sp.GetRequiredService<OpenAIClient>();
    var logger = sp.GetRequiredService<ILogger<AdvertisingService>>();
    var model = builder.Configuration["OpenAI:DefaultModel"] ?? "gpt-4-turbo-preview";
    return new AdvertisingService(openAIClient, logger, model);
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
