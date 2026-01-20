using Microsoft.OpenApi.Models;
using OpenAIShared;
using RequirementsAssistant.Core;

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
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
