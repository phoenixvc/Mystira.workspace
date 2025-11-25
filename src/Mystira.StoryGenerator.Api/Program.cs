using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Api.Services;
using Mystira.StoryGenerator.Application.Services;
using Mystira.StoryGenerator.Llm.Services.Intent;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Llm.Services;
using Mystira.StoryGenerator.Llm.Services.Instructions;
using Mystira.StoryGenerator.Llm.Services.LLM;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining(typeof(Mystira.StoryGenerator.Application.Handlers.Stories.GenerateStoryCommandHandler)));

builder.Services.AddOptions<AiSettings>()
    .Bind(builder.Configuration.GetSection(AiSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<InstructionSearchSettings>()
    .Bind(builder.Configuration.GetSection(InstructionSearchSettings.SectionName));

// Register HttpClient for LLM services (moved to Llm project)
// Increase default timeout to 180 seconds to avoid premature cancellations on long-running LLM calls
builder.Services.AddHttpClient<AzureOpenAIService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(180);
});

// Register LLM services (in Llm project) and expose Domain interfaces
builder.Services.AddScoped<ILLMService, AzureOpenAIService>();
builder.Services.AddScoped<ILLMServiceFactory, LLMServiceFactory>();

// Story schema provider abstraction (also implements Domain interface)
builder.Services.AddScoped<IStorySchemaProvider, FileStorySchemaProvider>();
// Story validation service (Domain interface) implemented in Application layer
builder.Services.AddScoped<IStoryValidationService, StoryValidationService>();

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
builder.Services.AddScoped<IInstructionBlockService, InstructionBlockService>();
// Register Intent router implementation from Llm project for Domain interface
builder.Services.AddScoped<IIntentClassificationService, StoryIntentClassifier>();
builder.Services.AddScoped<ICommandIntentRouter, CommandIntentRouter>();
// Register Chat Orchestration Service
builder.Services.AddScoped<IChatOrchestrationService, ChatOrchestrationService>();

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

// Expose Program for integration testing with WebApplicationFactory
public partial class Program { }
