using Microsoft.OpenApi.Models;
using OpenAIShared;
using RetroAnalyzer.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Retrospective Analyzer API",
        Version = "v1",
        Description = "API for analyzing sprint retrospective comments"
    });
});

builder.Services.AddSignalR();
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
builder.Services.AddScoped<RetroAnalyzerService>(sp =>
{
    var openAIClient = sp.GetRequiredService<OpenAIClient>();
    var logger = sp.GetRequiredService<ILogger<RetroAnalyzerService>>();
    var model = builder.Configuration["OpenAI:DefaultModel"] ?? "gpt-4-turbo-preview";
    return new RetroAnalyzerService(openAIClient, logger, model);
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
