using Microsoft.OpenApi.Models;
using OpenAIShared;
using MeetingAnalyzer.Core;
using Shared.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Meeting Analyzer API",
        Version = "v1",
        Description = "API for transcribing and analyzing meeting audio"
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

builder.Services.AddScoped<MeetingService>(sp =>
{
    var openAIClient = sp.GetRequiredService<OpenAIClient>();
    var logger = sp.GetRequiredService<ILogger<MeetingService>>();
    var model = builder.Configuration["OpenAI:DefaultModel"] ?? "gpt-4-turbo-preview";
    return new MeetingService(openAIClient, logger, model);
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
