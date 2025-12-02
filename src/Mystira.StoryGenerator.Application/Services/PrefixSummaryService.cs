using System.Text;
using Mystira.StoryGenerator.Application.Scenarios;
using Mystira.StoryGenerator.Contracts.StoryConsistency;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Application.Services;

public class PrefixSummaryService
{
    private readonly IPrefixSummaryLlmService _prefixSummaryService;

    public PrefixSummaryService(IPrefixSummaryLlmService prefixSummaryService)
    {
        _prefixSummaryService = prefixSummaryService;
    }

    /// <summary>
    /// Generates unique story prefixes from a <see cref="ScenarioGraph"/>,
    /// then calls the provided <see cref="IPrefixSummaryService"/> once per
    /// unique prefix to obtain heavyweight LLM summaries.
    ///
    /// This uses compressed paths to avoid enumerating all raw paths,
    /// and deduplicates prefixes by their sequence of scene IDs.
    /// </summary>
    /// <param name="scenario">
    /// The underlying <see cref="Scenario"/> (used to get scene descriptions).
    /// </param>
    /// <param name="graph">
    /// The scenario graph built from that scenario.
    /// </param>
    /// <param name="prefixSummaryService">
    /// Service that calls the heavyweight LLM to generate prefix summaries.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A collection of <see cref="PrefixSummary"/> objects, one for each
    /// unique prefix discovered in the graph.
    /// </returns>
    public async Task<IReadOnlyList<ScenarioPathPrefixSummary>> GeneratePrefixSummariesAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default)
    {
        if (scenario == null) throw new ArgumentNullException(nameof(scenario));

        // Build graph and a quick lookup from scene id → Scene
        var graph = ScenarioGraph.FromScenario(scenario);
        var scenesById = scenario.Scenes.ToDictionary(s => s.Id);

        // Use your existing compressed paths helper
        var compressedPaths = graph.GetCompressedPaths().ToList();

        var results = new List<ScenarioPathPrefixSummary>();

        // Enumerate each full path
        for (var pathIndex = 0; pathIndex < compressedPaths.Count; pathIndex++)
        {
            var compressedPath = compressedPaths[pathIndex];

            // Reconstruct the ordered list of Scene instances for this path
            var sceneSequence = compressedPath.SceneIds
                .Select(id => scenesById[id])
                .ToList();

            // Now generate all non-empty prefixes of this path
            for (var prefixLength = 1; prefixLength <= sceneSequence.Count; prefixLength++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var prefixScenes = sceneSequence.Take(prefixLength);

                // Call your heavyweight LLM-backed service
                var summary = await _prefixSummaryService.SummarizeAsync(prefixScenes, cancellationToken);

                if (summary != null)
                    results.Add(summary);
            }
        }

        return results;
    }

    /// <summary>
    /// Renders a prefix as a simple narrative text that the LLM can
    /// reason over. You can evolve this format over time (include
    /// answers, roll results, etc.).
    /// </summary>
    /// <param name="prefixSceneIds">Ordered scene IDs in the prefix.</param>
    /// <param name="scenesById">Lookup table for scenes by ID.</param>
    /// <returns>Concatenated textual prefix.</returns>
    private static string BuildPrefixText(
        IReadOnlyList<string> prefixSceneIds,
        IReadOnlyDictionary<string, Scene> scenesById)
    {
        var sb = new StringBuilder();

        for (int i = 0; i < prefixSceneIds.Count; i++)
        {
            var sceneId = prefixSceneIds[i];
            if (!scenesById.TryGetValue(sceneId, out var scene))
            {
                // Optionally throw if this should never happen
                continue;
            }

            sb.AppendLine($"Scene {scene.Id}:");
            sb.AppendLine(scene.Description);
            sb.AppendLine();

            // OPTIONAL: if you want to include branch labels/answers,
            // you'll need to track which choice/roll outcome led here.
            // That’s extra wiring between ScenarioPath and this method.
        }

        return sb.ToString().TrimEnd();
    }
}
