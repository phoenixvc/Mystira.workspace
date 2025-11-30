using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Application.Scenarios;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Llm.Console.Tests;
using Mystira.StoryGenerator.Llm.Services.DominatorBasedConsistency;
using Mystira.StoryGenerator.Llm.Services.LLM;

var builder = Host.CreateApplicationBuilder(args);

// Configuration: appsettings.json + environment variables
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Options
builder.Services.Configure<AiSettings>(builder.Configuration.GetSection(AiSettings.SectionName));

// LLM services
builder.Services.AddSingleton<ILLMService, AzureOpenAIService>();
builder.Services.AddSingleton<ILLMService, AnthropicAIService>();
builder.Services.AddSingleton<ILlmServiceFactory, LLMServiceFactory>();

// Entity classifier and consistency evaluator
builder.Services.AddSingleton<SceneEntityLlmClassifier>();
builder.Services.AddSingleton<ScenarioPathConsistencyLlmEvaluator>();
// Scenario factory for loading scenarios from YAML/JSON in console tool
builder.Services.AddSingleton<IScenarioFactory, ScenarioFactory>();

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Mystira.Llm.Console");

// Optional CLI: provider and model deployment name to use
// Provider Flags: --provider, -p (e.g., "azure-openai" or "anthropic")
// Model Flags: --model, -m, --deployment, --deployment-name (e.g., deployment/model id)
string providerArg = "";
string deploymentArg = "";

// Parse provider
int providerIdx = Array.FindIndex(args, a =>
    a.Equals("--provider", StringComparison.OrdinalIgnoreCase) ||
    a.Equals("-p", StringComparison.OrdinalIgnoreCase));
if (providerIdx >= 0 && (providerIdx + 1) < args.Length)
{
    var val = args[providerIdx + 1];
    if (!string.IsNullOrWhiteSpace(val))
    {
        providerArg = val.Trim();
    }
}
int modelIdx = Array.FindIndex(args, a =>
    a.Equals("--model", StringComparison.OrdinalIgnoreCase) ||
    a.Equals("-m", StringComparison.OrdinalIgnoreCase) ||
    a.Equals("--deployment", StringComparison.OrdinalIgnoreCase) ||
    a.Equals("--deployment-name", StringComparison.OrdinalIgnoreCase));
if (modelIdx >= 0 && (modelIdx + 1) < args.Length)
{
    var val = args[modelIdx + 1];
    if (!string.IsNullOrWhiteSpace(val))
    {
        deploymentArg = val.Trim();
    }
}

// Apply the chosen provider and deployment/model name to AiSettings so downstream services use it
if (!string.IsNullOrEmpty(providerArg) || !string.IsNullOrEmpty(deploymentArg))
{
    try
    {
        var opts = host.Services.GetRequiredService<IOptions<AiSettings>>();
        var s = opts.Value;
        // Set provider across relevant consumers (console tools use these)
        if (!string.IsNullOrEmpty(providerArg))
        {
            s.DefaultProvider = providerArg; // optional global default
            s.EntityClassifier.Provider = providerArg; // used by ScenarioConsistencyLlmEvaluator (current wiring)
            s.ConsistencyEvaluator.Provider = providerArg; // forward-looking correct section
        }

        // Set deployment across relevant consumers
        if (!string.IsNullOrEmpty(deploymentArg))
        {
            s.AzureOpenAI.DeploymentName = deploymentArg;
            s.EntityClassifier.DeploymentName = deploymentArg;
            s.ConsistencyEvaluator.DeploymentName = deploymentArg;
        }

        if (!string.IsNullOrEmpty(providerArg) && !string.IsNullOrEmpty(deploymentArg))
            logger.LogInformation("Using provider: {Provider}, model deployment: {Deployment}", providerArg, deploymentArg);
        else if (!string.IsNullOrEmpty(providerArg))
            logger.LogInformation("Using provider: {Provider}", providerArg);
        else if (!string.IsNullOrEmpty(deploymentArg))
            logger.LogInformation("Using model deployment: {Deployment}", deploymentArg);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Failed to apply model deployment argument; proceeding with existing configuration.");
    }
}

// CLI: --test-entity-classifier (legacy) or --test-event-classifier
if (args.Any(a => a.Equals("--test-entity-classifier", StringComparison.OrdinalIgnoreCase)
               || a.Equals("test-entity-classifier", StringComparison.OrdinalIgnoreCase)))
{
    var exitCode = await EntityClassificationConsoleTests.RunAsync(host.Services, logger);
    return exitCode;
}

// CLI: --test-consistency
if (args.Any(a => a.Equals("--test-consistency", StringComparison.OrdinalIgnoreCase)
               || a.Equals("test-consistency", StringComparison.OrdinalIgnoreCase)))
{
    var exitCode = await ConsistencyConsoleTests.RunAsync(host.Services, logger);
    return exitCode;
}

// CLI: --consistency-file <path> [--format yaml|json]
int fileArgIndex = Array.FindIndex(args, a => a.Equals("--consistency-file", StringComparison.OrdinalIgnoreCase)
                                       || a.Equals("consistency-file", StringComparison.OrdinalIgnoreCase));
if (fileArgIndex >= 0)
{
    var exitCode = await ConsistencyFileRunner.RunAsync(host.Services, logger, args);
    return exitCode;
}

// Default help
logger.LogInformation("Mystira.StoryGenerator.Llm.Console");
logger.LogInformation("Usage:");
logger.LogInformation("  --test-entity-classifier   Alias for --test-event-classifier");
logger.LogInformation("  --test-consistency         Runs ScenarioConsistencyLlmEvaluator examples and prints assessment & issues");
logger.LogInformation("  --consistency-file <path> [--format yaml|json]   Evaluate consistency over each compressed path of the given scenario file");
logger.LogInformation("  --provider|-p <name>       LLM provider to use (e.g., azure-openai, anthropic)");
logger.LogInformation("  --model|-m <deployment>    Model deployment to use (e.g., gpt-4o, claude-sonnet-4-5)");
return 0;
