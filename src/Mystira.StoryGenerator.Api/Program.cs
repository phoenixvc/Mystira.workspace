using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Api.Services;
using Mystira.StoryGenerator.Application;
using Mystira.StoryGenerator.Application.Infrastructure.Agents;
using Mystira.StoryGenerator.Application.Scenarios;
using Mystira.StoryGenerator.Application.Services;
using Mystira.StoryGenerator.Application.StoryConsistencyAnalysis.Legacy;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Llm.Services.ConsistencyEvaluators;
using Mystira.StoryGenerator.Llm.Services.LLM;
using Mystira.StoryGenerator.Llm.Services.StoryInstructionsRag;
using Mystira.StoryGenerator.Llm.Services.StoryIntentClassification;
using Mystira.StoryGenerator.Api.Services;
using Mystira.StoryGenerator.Api.Services.ContinuityAsync;

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

builder.Services.AddOptions<LlmRateLimitOptions>()
    .Bind(builder.Configuration.GetSection(LlmRateLimitOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<InstructionSearchSettings>()
    .Bind(builder.Configuration.GetSection(InstructionSearchSettings.SectionName));

// Register HttpClient for LLM services (moved to Llm project)
// Increase default timeout to 300 seconds to avoid premature cancellations on long-running LLM calls
builder.Services.AddHttpClient<AzureOpenAIService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(300);
});

builder.Services.AddHttpClient<AnthropicAIService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(300);
});

// Register LLM services (in Llm project) and expose Domain interfaces
// Note: LLM factory is used by singleton LLM consumers, so keep these as singletons
builder.Services.AddSingleton<ILLMService, AzureOpenAIService>();
builder.Services.AddSingleton<ILLMService, AnthropicAIService>();
builder.Services.AddSingleton<ILlmServiceFactory, LLMServiceFactory>();

// Story schema provider abstraction (also implements Domain interface)
builder.Services.AddScoped<IStorySchemaProvider, FileStorySchemaProvider>();
// Story validation service (Domain interface) implemented in Application layer
builder.Services.AddScoped<IStoryValidationService, StoryValidationService>();
// Scenario factory for creating Domain scenarios from JSON or YAML content
builder.Services.AddScoped<IScenarioFactory, ScenarioFactory>();

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
builder.Services.AddScoped<ILlmIntentLlmClassificationService, StoryLlmIntentLlmClassifier>();
builder.Services.AddScoped<ICommandRouter, CommandIntentRouter>();
// Register Chat Orchestration Service
builder.Services.AddScoped<IChatOrchestrationService, ChatOrchestrationService>();

// Register consistency evaluation services
builder.Services.AddScoped<IDominatorPathConsistencyLlmService, DominatorPathConsistencyLlmService>();
builder.Services.AddScoped<IEntityLlmClassificationService, SceneEntityLlmClassifierService>();
builder.Services.AddScoped<IScenarioEntityConsistencyEvaluationService, ScenarioEntityConsistencyEvaluationService>();
builder.Services.AddScoped<IScenarioDominatorPathConsistencyEvaluationService, ScenarioDominatorPathConsistencyEvaluationService>();
builder.Services.AddScoped<IScenarioConsistencyEvaluationService, ScenarioConsistencyEvaluationService>();

// Register story continuity service
builder.Services.AddScoped<IStoryContinuityService, StoryContinuityService>();

// Register story continuity dependencies (prefix summaries + SRL pipeline)
builder.Services.AddScoped<IPrefixSummaryService, ScenarioPrefixSummaryService>();
builder.Services.AddScoped<IScenarioSrlAnalysisService, ScenarioSrlAnalysisService>();
builder.Services.AddSingleton<IPrefixSummaryLlmService, PrefixSummaryLlmService>();
builder.Services.AddSingleton<ISemanticRoleLabellingLlmService, SemanticRoleLabellingLlmService>();

// Async continuity infrastructure (in-memory)
builder.Services.AddSingleton<IContinuityOperationStore, InMemoryContinuityOperationStore>();
builder.Services.AddSingleton<IContinuityBackgroundQueue, ContinuityBackgroundQueue>();
builder.Services.AddHostedService<ContinuityWorker>();

// Register Azure AI Foundry Agent services
builder.Services.AddOptions<FoundryAgentConfig>()
    .Bind(builder.Configuration.GetSection(FoundryAgentConfig.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddCosmosDbConfiguration();

// Register Foundry services using the configuration
var foundryConfig = builder.Configuration.GetSection(FoundryAgentConfig.SectionName).Get<FoundryAgentConfig>();
if (foundryConfig != null)
{
    builder.Services.AddFoundryAgentServices(foundryConfig);
}

// Register Agent Orchestrator services
var isDevelopment = builder.Environment.IsDevelopment();
builder.Services.AddScoped<IAgentOrchestrator, AgentOrchestrator>();
builder.Services.AddSingleton<IAgentStreamPublisher>(sp => 
    isDevelopment ? new InMemoryStreamPublisher() : new InMemoryStreamPublisher() /* TODO: SignalRStreamPublisher for production */);

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
namespace Mystira.StoryGenerator.Api
{
    public partial class Program { }
}
