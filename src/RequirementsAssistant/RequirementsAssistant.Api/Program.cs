using Microsoft.OpenApi.Models;
using OpenAIShared;
using RequirementsAssistant.Core;
using Shared.Common;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Requirements Assistant API",
        Version = "v1",
        Description = "API for processing requirements documents and generating user stories"
    });
});

// Configure OpenAI
builder.Services.AddOpenAIServices(builder.Configuration);

// Add health checks
builder.Services.AddOpenAIHealthChecks();

// Add JWT authentication (optional - can use API key instead)
// builder.Services.AddJwtAuthentication(builder.Configuration);
// builder.Services.AddScoped<JwtTokenService>();

// Register application services
builder.Services.AddScoped<RequirementsService>(sp =>
{
    var openAIClient = sp.GetRequiredService<OpenAIClient>();
    var logger = sp.GetRequiredService<ILogger<RequirementsService>>();
    var model = builder.Configuration["OpenAI:DefaultModel"] ?? "gpt-4-turbo-preview";
    return new RequirementsService(openAIClient, logger, model);
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add correlation IDs and logging
app.UseCorrelationId();
app.UseRequestResponseLogging();

// Add health checks
app.MapHealthChecks("/health");

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
