using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Mystira.StoryGenerator.Application.Scenarios;
using Mystira.StoryGenerator.Application.Services;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Llm.Services.DominatorBasedConsistency;

namespace Mystira.StoryGenerator.Llm.Console.Tests;

internal static class ConsistencyFileRunner
{
    public static async Task<int> RunAsync(IServiceProvider services, ILogger logger, string[] args)
    {
        // CLI: --consistency-file <path> [--format yaml|json]
        int fileArgIndex = Array.FindIndex(args, a => a.Equals("--consistency-file", StringComparison.OrdinalIgnoreCase)
                                               || a.Equals("consistency-file", StringComparison.OrdinalIgnoreCase));
        if (fileArgIndex < 0)
        {
            logger.LogError("--consistency-file flag not found");
            return 2;
        }

        var path = (fileArgIndex + 1) < args.Length ? args[fileArgIndex + 1] : null;
        if (string.IsNullOrWhiteSpace(path))
        {
            logger.LogError("--consistency-file requires a file path argument.");
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
        var evaluator = services.GetRequiredService<ScenarioConsistencyLlmEvaluator>();

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

        // Parallel evaluation with progress reporting
        var results = new StoryConsistencyEvaluation[paths.Count];
        int processed = 0;
        int total = paths.Count;
        var degree = Math.Max(2, Math.Min(Environment.ProcessorCount, 8));
        using var gate = new SemaphoreSlim(degree, degree);
        var tasks = new List<Task>();

        for (int i = 0; i < paths.Count; i++)
        {
            var localIndex = i; // capture
            await gate.WaitAsync();
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var p = paths[localIndex];
                    var pathStr = string.Join(" -> ", p.SceneIds);

                    // Output scenario and LLM result paragraphs
                    logger.LogInformation("\n[{Index}] Path: {PathStr}", localIndex + 1, pathStr);
                    logger.LogInformation("[{Index}] Scenario Text\n{Story}\n", localIndex + 1, p.Story);

                    var result = await evaluator.EvaluateConsistencyAsync(p.Story);
                    string resultJson = result == null
                        ? "<no result>"
                        : System.Text.Json.JsonSerializer.Serialize(
                            result,
                            new System.Text.Json.JsonSerializerOptions
                            {
                                WriteIndented = true,
                                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                            });

                    logger.LogInformation("[{Index}] LLM Output (JSON)\n{Json}\n", localIndex + 1, resultJson);

                    results[localIndex] = new StoryConsistencyEvaluation
                    {
                        Path = pathStr,
                        PathContent = p.Story,
                        Result = result
                    };
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[{Index}] Exception while evaluating path", localIndex + 1);
                    // Still record an empty result to keep array aligned
                    var p = paths[localIndex];
                    results[localIndex] = new StoryConsistencyEvaluation
                    {
                        Path = string.Join(" -> ", p.SceneIds),
                        PathContent = p.Story,
                        Result = null
                    };
                }
                finally
                {
                    var done = Interlocked.Increment(ref processed);
                    logger.LogInformation("Progress: {Done}/{Total} paths processed", done, total);
                    gate.Release();
                }
            }));
        }

        await Task.WhenAll(tasks);

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
