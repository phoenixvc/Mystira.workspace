using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Api.Services;
using Mystira.StoryGenerator.Api.Services.LLM;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Contracts.Stories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<AiSettings>()
    .Bind(builder.Configuration.GetSection(AiSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Register HttpClient for LLM services
builder.Services.AddHttpClient<AzureOpenAIService>();
builder.Services.AddHttpClient<GoogleGeminiService>();

// Register LLM services
builder.Services.AddScoped<ILLMService, AzureOpenAIService>();
builder.Services.AddScoped<ILLMService, GoogleGeminiService>();
builder.Services.AddScoped<ILLMServiceFactory, LLMServiceFactory>();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

builder.Services.AddHealthChecks();

// Register services
builder.Services.AddScoped<IStoryValidationService, StoryValidationService>();
builder.Services.AddScoped<IStoryGenerationService, StoryGenerationService>();

var app = builder.Build();

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapHealthChecks("/health").WithName("Health");

app.MapGet("/ping", () => Results.Ok(new { status = "ok" }))
   .WithName("Ping")
   .WithOpenApi();

app.MapPost("/stories/preview", (GenerateStoryRequest request, IOptions<AiSettings> aiOptions) =>
    {
        var settings = aiOptions.Value;
        var response = new GenerateStoryResponse
        {
            Story = $"Story generation is not yet implemented. Prompt: '{request.Prompt}'.",
            Model = $"{settings.DefaultProvider} (preview mode)"
        };

        return Results.Ok(response);
    })
   .WithName("GenerateStoryPreview")
   .WithOpenApi();

app.Run();
