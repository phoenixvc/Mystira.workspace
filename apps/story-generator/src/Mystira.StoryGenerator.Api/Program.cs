using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Wolverine;
using Mystira.StoryGenerator.Api.Infrastructure.Agents;
using Mystira.StoryGenerator.Api.Services;
using Mystira.StoryGenerator.Api.Services.ContinuityAsync;
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
using Mystira.Shared.Configuration;

var builder = WebApplication.CreateBuilder(args);

var keyVaultUrl = Environment.GetEnvironmentVariable("KEY_VAULT_URL");
if (!string.IsNullOrWhiteSpace(keyVaultUrl) && string.IsNullOrWhiteSpace(builder.Configuration["KeyVault:Name"]))
{
    if (!Uri.TryCreate(keyVaultUrl, UriKind.Absolute, out var keyVaultUri) || string.IsNullOrWhiteSpace(keyVaultUri.Host))
    {
        throw new InvalidOperationException($"Invalid KEY_VAULT_URL '{keyVaultUrl}'. Provide a full Key Vault URL like 'https://<vault-name>.vault.azure.net/'.");
    }

    var keyVaultName = keyVaultUri.Host.Split('.')[0];
    if (string.IsNullOrWhiteSpace(keyVaultName))
    {
        throw new InvalidOperationException($"Invalid KEY_VAULT_URL '{keyVaultUrl}'. Unable to derive Key Vault name from host '{keyVaultUri.Host}'.");
    }

    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["KeyVault:Name"] = keyVaultName,
    });
}

builder.Host.AddKeyVaultConfiguration();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Mystira.Core.Ports.Services.ICurrentUserService, CurrentUserService>();

builder.Host.UseWolverine(opts =>
{
    opts.Discovery.IncludeAssembly(
        typeof(Mystira.StoryGenerator.Application.Handlers.Stories.GenerateStoryCommandHandler).Assembly);
    opts.Policies.UseDurableLocalQueues();
});

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

    options.AddPolicy("BlazerOrigin", policy =>
    {
        policy.WithOrigins("https://localhost:7043")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
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
builder.Services.Configure<FoundryAgentConfig>(builder.Configuration.GetSection(FoundryAgentConfig.SectionName));
builder.Services.AddFoundryAgentServices();

builder.Services.Configure<CosmosDbConfig>(builder.Configuration.GetSection(CosmosDbConfig.SectionName));
builder.Services.AddCosmosDbConfiguration();

// Register Agent Orchestrator services
var isDevelopment = builder.Environment.IsDevelopment();
builder.Services.AddScoped<IAgentOrchestrator, AgentOrchestrator>();
builder.Services.AddSingleton<IAgentStreamPublisher>(sp =>
    isDevelopment ? new InMemoryStreamPublisher() : new InMemoryStreamPublisher() /* TODO: SignalRStreamPublisher for production */);

// Rate limiting for story agent endpoints
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("story-agent", config =>
    {
        config.Window = TimeSpan.FromMinutes(1);
        config.PermitLimit = 10;
    });
});

builder.Services.AddMystiraAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddMystiraEntraIdAuthentication(builder.Configuration);

var app = builder.Build();

app.UseCors("AllowFrontend");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Story Generator Agent API V1");
    c.RoutePrefix = "swagger";
});

// Redirect root to swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks("/health").WithName("Health");

app.MapGet("/ping", () => Results.Ok(new { status = "ok" }))
   .WithName("Ping")
   .AllowAnonymous(); // Keep ping endpoint public for health checks

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
   .RequireAuthorization()
   .RequireRateLimiting("story-agent");

app.Run();

// Expose Program for integration testing with WebApplicationFactory
namespace Mystira.StoryGenerator.Api
{
    public partial class Program { }
}
