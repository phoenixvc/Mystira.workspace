using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Contracts.StoryConsistency;
using Mystira.StoryGenerator.Domain.Services;
using System.Diagnostics;
using System.Text.Encodings.Web;

namespace Mystira.StoryGenerator.Llm.Console.Tests;

internal static class StoryContinuityFileRunner
{
    /// <summary>
    /// Runs full story continuity analysis (prefix summaries + SRL + comparison) for a scenario file.
    /// Usage: --story-continuity-file [<path>]
    /// If no path is provided, defaults to test_data/Test-Story-UnintroducedEntities-Small.yaml
    /// </summary>
    public static async Task<int> RunAsync(IServiceProvider services, ILogger logger, string[] args)
    {
        try
        {
            int flagIdx = Array.FindIndex(args, a => a.Equals("--story-continuity-file", StringComparison.OrdinalIgnoreCase)
                                                  || a.Equals("story-continuity-file", StringComparison.OrdinalIgnoreCase));
            if (flagIdx < 0)
            {
                logger.LogError("--story-continuity-file flag not found");
                return 2;
            }

            var factory = services.GetRequiredService<IScenarioFactory>();
            var continuityService = services.GetRequiredService<IStoryContinuityService>();

            string[] includedConfidences = ["medium", "high"];
            string[] includedEntityTypes = ["character", "location", "item"]; // "character", "location", "item", "concept"
            bool properNounsOnly = true;

            string defaultPath = Path.Combine("test_data", "Test-Story-UnintroducedEntities-Small.yaml");
            string path = (flagIdx + 1) < args.Length && !args[flagIdx + 1].StartsWith("--")
                ? args[flagIdx + 1]
                : defaultPath;

            if (!File.Exists(path))
            {
                logger.LogError("Story continuity: File not found: {Path}", path);
                return 2;
            }

            var format = Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".json" => ScenarioContentFormat.Json,
                ".yaml" or ".yml" => ScenarioContentFormat.Yaml,
                _ => ScenarioContentFormat.Yaml
            };

            var swTotal = Stopwatch.StartNew();
            logger.LogInformation("Story continuity: Loading scenario from {Path}", path);

            var swLoad = Stopwatch.StartNew();
            var content = await File.ReadAllTextAsync(path);
            var scenario = await factory.CreateFromContentAsync(content, format);
            swLoad.Stop();
            logger.LogInformation("Story continuity: Loaded scenario in {ElapsedMs} ms", swLoad.ElapsedMilliseconds);

            logger.LogInformation("Story continuity: Running analysis...");
            var swAnalysis = Stopwatch.StartNew();
            var issues = await continuityService.AnalyzeAsync(scenario);
            // Post-processing filter on issues
            issues = Application.StoryConsistencyAnalysis.ContinuityAnalyzer.EntityContinuityIssueFiltering
                .Filter(issues,
                    confidences: includedConfidences,
                    entityTypes: includedEntityTypes,
                    properNounsOnly: properNounsOnly);
            swAnalysis.Stop();
            logger.LogInformation("Story continuity: Analysis completed in {ElapsedMs} ms", swAnalysis.ElapsedMilliseconds);

            issues ??= Array.Empty<EntityContinuityIssue>();

            logger.LogInformation("Story continuity: Found {Count} issue(s).", issues.Count);
            foreach (var issue in issues)
            {
                var evidence = string.IsNullOrWhiteSpace(issue.EvidenceSpan) ? "" : $" | evidence: '{issue.EvidenceSpan}'";
                logger.LogInformation(" - Scene {SceneId}: [{Type}] {EntityType} '{Name}' — {Detail}{Evidence}",
                    issue.SceneId,
                    issue.IssueType,
                    issue.EntityType,
                    issue.EntityName,
                    issue.Detail,
                    evidence);
            }

            // Dump JSON array of issues to stdout for automation
            var jsonOptions = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
            // Prevent escaping of common ASCII characters like the apostrophe (\u0027)
            jsonOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            jsonOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase));
            var json = System.Text.Json.JsonSerializer.Serialize(issues, jsonOptions);
            System.Console.WriteLine(json);

            swTotal.Stop();
            logger.LogInformation("Story continuity: End-to-end duration {ElapsedMs} ms", swTotal.ElapsedMilliseconds);

            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Story continuity analysis failed.");
            return 1;
        }
    }
}
