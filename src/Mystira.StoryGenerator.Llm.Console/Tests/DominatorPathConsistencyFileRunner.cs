using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Mystira.StoryGenerator.Application.Scenarios;
using Mystira.StoryGenerator.Application.StoryConsistencyAnalysis;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Llm.Console.Tests;

internal static class DominatorPathConsistencyFileRunner
{
    public static async Task<int> RunAsync(IServiceProvider services, ILogger logger, string[] args)
    {
        // CLI: --dominator-path-consistency-file <path> [--format yaml|json]
        int fileArgIndex = Array.FindIndex(args, a => a.Equals("--dominator-path-consistency-file", StringComparison.OrdinalIgnoreCase)
                                               || a.Equals("dominator-path-consistency-file", StringComparison.OrdinalIgnoreCase));
        if (fileArgIndex < 0)
        {
            logger.LogError("--dominator-path-consistency-file flag not found");
            return 2;
        }

        var path = (fileArgIndex + 1) < args.Length ? args[fileArgIndex + 1] : null;
        if (string.IsNullOrWhiteSpace(path))
        {
            logger.LogError("--dominator-path-consistency-file requires a file path argument.");
            return 2;
        }

        if (!File.Exists(path))
        {
            logger.LogError("File not found: {Path}", path);
            return 2;
        }

        // Determine format
        ScenarioContentFormat format;
        int fmtIdx = Array.FindIndex(args, a => a.Equals("--format", StringComparison.OrdinalIgnoreCase) || a.Equals("-f", StringComparison.OrdinalIgnoreCase));
        if (fmtIdx >= 0 && (fmtIdx + 1) < args.Length)
        {
            var fmt = args[fmtIdx + 1].Trim().ToLowerInvariant();
            format = fmt switch
            {
                "json" => ScenarioContentFormat.Json,
                "yaml" or "yml" => ScenarioContentFormat.Yaml,
                _ => InferFormatFromExtension(path)
            };
        }
        else
        {
            format = InferFormatFromExtension(path);
        }

        var factory = services.GetRequiredService<IScenarioFactory>();
        var domService = services.GetRequiredService<IScenarioDominatorPathConsistencyEvaluationService>();

        var content = await File.ReadAllTextAsync(path);
        var scenario = await factory.CreateFromContentAsync(content, format);
        var graph = ScenarioGraph.FromScenario(scenario);
        var paths = graph.GetCompressedPaths().ToList();

        logger.LogInformation("Loaded scenario with {SceneCount} scenes. Found {PathCount} compressed path(s).", scenario.Scenes.Count, paths.Count);

        // Determine output file
        string? outPath = null;
        int outIdx = Array.FindIndex(args, a => a.Equals("--out", StringComparison.OrdinalIgnoreCase)
                                              || a.Equals("-o", StringComparison.OrdinalIgnoreCase)
                                              || a.Equals("--output-file", StringComparison.OrdinalIgnoreCase));
        if (outIdx >= 0 && (outIdx + 1) < args.Length)
        {
            outPath = args[outIdx + 1];
        }
        else
        {
            var dir = Path.GetDirectoryName(path) ?? ".";
            var nameNoExt = Path.GetFileNameWithoutExtension(path);
            var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            outPath = Path.Combine(dir, $"{nameNoExt}.consistency.{stamp}.json");
        }

        // Build a lookup from SceneIds signature to story text to enrich output later
        var storyBySceneIds = paths.ToDictionary(
            p => string.Join("|", p.SceneIds),
            p => p.Story);

        // Delegate path evaluation to the dominator path consistency service
        var pathResults = await domService.EvaluateAsync(scenario);

        // Prepare final results array for file output
        var results = new List<StoryConsistencyEvaluation>();
        if (pathResults?.PathResults != null)
        {
            int idx = 0;
            foreach (var pr in pathResults.PathResults)
            {
                idx++;
                var pathStr = string.Join(" -> ", pr.SceneIds);
                logger.LogInformation("\n[{Index}] Path: {PathStr}", idx, pathStr);

                var key = string.Join("|", pr.SceneIds);
                storyBySceneIds.TryGetValue(key, out var story);
                story ??= "<story content unavailable>";
                logger.LogInformation("[{Index}] Scenario Text\n{Story}\n", idx, story);

                var resultJson = pr.Result == null
                    ? "<no result>"
                    : System.Text.Json.JsonSerializer.Serialize(
                        pr.Result,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            WriteIndented = true,
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                        });

                logger.LogInformation("[{Index}] LLM Output (JSON)\n{Json}\n", idx, resultJson);

                results.Add(new StoryConsistencyEvaluation
                {
                    Path = pathStr,
                    PathContent = story,
                    Result = pr.Result
                });
            }
        }

        // Write JSON results to file
        try
        {
            var jsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var finalJson = System.Text.Json.JsonSerializer.Serialize(results, jsonOptions);
            await File.WriteAllTextAsync(outPath!, finalJson);
            logger.LogInformation("Wrote consistency evaluation results to: {OutPath}", outPath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to write results to output file.");
        }

        // Also print a short note to stdout for tooling visibility
        System.Console.WriteLine($"Results written to: {outPath}");

        return 0;
    }

    private static ScenarioContentFormat InferFormatFromExtension(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".json" => ScenarioContentFormat.Json,
            ".yaml" or ".yml" => ScenarioContentFormat.Yaml,
            _ => ScenarioContentFormat.Yaml
        };
    }
}
