using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Domain.Services;
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
builder.Services.AddSingleton<ILlmServiceFactory, LLMServiceFactory>();

// Entity classifier and consistency evaluator
builder.Services.AddSingleton<SceneEntityLlmClassifier>();
builder.Services.AddSingleton<ScenarioConsistencyLlmEvaluator>();

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Mystira.Llm.Console");

// CLI: --test-entity-classifier (legacy) or --test-event-classifier
if (args.Any(a => a.Equals("--test-entity-classifier", StringComparison.OrdinalIgnoreCase)
               || a.Equals("test-entity-classifier", StringComparison.OrdinalIgnoreCase)))
{
    var exitCode = await Mystira.StoryGenerator.Llm.Console.Tests.EventClassificationConsoleTests.RunAsync(host.Services, logger);
    return exitCode;
}

// CLI: --test-consistency
if (args.Any(a => a.Equals("--test-consistency", StringComparison.OrdinalIgnoreCase)
               || a.Equals("test-consistency", StringComparison.OrdinalIgnoreCase)))
{
    var exitCode = await Mystira.StoryGenerator.Llm.Console.Tests.ConsistencyConsoleTests.RunAsync(host.Services, logger);
    return exitCode;
}

// Default help
logger.LogInformation("Mystira.StoryGenerator.Llm.Console");
logger.LogInformation("Usage:");
logger.LogInformation("  --test-entity-classifier   Alias for --test-event-classifier");
logger.LogInformation("  --test-consistency         Runs ScenarioConsistencyLlmEvaluator examples and prints assessment & issues");
return 0;
