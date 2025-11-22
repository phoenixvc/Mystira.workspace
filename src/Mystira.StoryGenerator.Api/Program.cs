using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Api.Services;
using Mystira.StoryGenerator.Llm.Services.Intent;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Llm.Services.Instructions;
using IInstructionBlockService = Mystira.StoryGenerator.Llm.Services.Instructions.IInstructionBlockService;

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
builder.Services.AddHttpClient<Mystira.StoryGenerator.Llm.Services.LLM.AzureOpenAIService>();

// Register LLM services (in Llm project) and expose Domain interfaces
builder.Services.AddScoped<Mystira.StoryGenerator.Llm.Services.LLM.ILLMService, Mystira.StoryGenerator.Llm.Services.LLM.AzureOpenAIService>();
builder.Services.AddScoped<ILLMService>(sp => (ILLMService)sp.GetRequiredService<Mystira.StoryGenerator.Llm.Services.LLM.ILLMService>());
builder.Services.AddScoped<ILLMServiceFactory, Mystira.StoryGenerator.Llm.Services.LLM.LLMServiceFactory>();

// Story schema provider abstraction (also implements Domain interface)
builder.Services.AddScoped<IStorySchemaProvider, FileStorySchemaProvider>();
builder.Services.AddScoped<IStorySchemaProvider>(sp => (IStorySchemaProvider)sp.GetRequiredService<IStorySchemaProvider>());
// Story validation service (Domain interface)
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
// Adapter to provide Domain IInstructionBlockService
builder.Services.AddScoped<Mystira.StoryGenerator.Domain.Services.IInstructionBlockService, InstructionBlockAdapter>();
// Register Intent router implementation from Llm project for Domain interface
builder.Services.AddScoped<IIntentRouterService, IntentRouterService>();
builder.Services.AddScoped<ICommandIntentRouter, CommandIntentRouter>();

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
