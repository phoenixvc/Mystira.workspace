using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Application.StoryConsistencyAnalysis;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Llm.Console.Tests;

internal static class EntityConsistencyFileRunner
{
    /// <summary>
    /// Runs entity introduction consistency evaluation for a scenario file.
    /// Usage: --entity-consistency-file [<path>]
    /// If no path is provided, defaults to test_data/Test-Story-UnintroducedEntities.yaml
    /// </summary>
    public static async Task<int> RunAsync(IServiceProvider services, ILogger logger, string[] args)
    {
        try
        {
            int entityConsistencyIdx = Array.FindIndex(args, a => a.Equals("--entity-consistency-file", StringComparison.OrdinalIgnoreCase)
                                                             || a.Equals("entity-consistency-file", StringComparison.OrdinalIgnoreCase));
            if (entityConsistencyIdx < 0)
            {
                logger.LogError("--entity-consistency-file flag not found");
                return 2;
            }

            var factory = services.GetRequiredService<IScenarioFactory>();
            var svc = services.GetRequiredService<IScenarioEntityConsistencyEvaluationService>();

            string defaultPath = Path.Combine("test_data", "Test-Story-UnintroducedEntities.yaml");
            string path = (entityConsistencyIdx + 1) < args.Length && !args[entityConsistencyIdx + 1].StartsWith("--")
                ? args[entityConsistencyIdx + 1]
                : defaultPath;

            if (!File.Exists(path))
            {
                logger.LogError("Entity consistency: File not found: {Path}", path);
                return 2;
            }

            var format = Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".json" => ScenarioContentFormat.Json,
                ".yaml" or ".yml" => ScenarioContentFormat.Yaml,
                _ => ScenarioContentFormat.Yaml
            };

            logger.LogInformation("Entity consistency: Loading scenario from {Path}", path);
            var content = await File.ReadAllTextAsync(path);
            var scenario = await factory.CreateFromContentAsync(content, format);
            var result = await svc.EvaluateAsync(scenario);

            if (result == null)
            {
                logger.LogWarning("Entity consistency: No result returned.");
                return 1;
            }

            // Print summary and details
            logger.LogInformation("Entity consistency: Found {Count} violation(s).", result.Violations.Count);
            foreach (var v in result.Violations)
            {
                logger.LogInformation(" - Scene {SceneId}: {EntityType} '{Name}' used before introduction",
                    v.SceneId, v.Entity.Type, v.Entity.Name);
            }

            // Also dump JSON to stdout for tooling (serialize enums as camelCase strings)
            var jsonOptions = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
            jsonOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase));
            var json = System.Text.Json.JsonSerializer.Serialize(result, jsonOptions);
            System.Console.WriteLine(json);

            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Entity consistency evaluation failed.");
            return 1;
        }
    }
}
