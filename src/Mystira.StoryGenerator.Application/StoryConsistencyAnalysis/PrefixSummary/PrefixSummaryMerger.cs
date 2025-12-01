using Mystira.StoryGenerator.Contracts.StoryConsistency;

namespace Mystira.StoryGenerator.Application.StoryConsistencyAnalysis.PrefixSummary;

public static class PrefixSummaryMerger
{
    /// <summary>
    /// For each scene, compute the set of entities that are guaranteed
    /// to be active at entry to that scene across all prefixes that end there.
    ///
    /// We key entities just by (normalized) name for now. If you want more
    /// safety, you can key by (name, type) pair.
    /// </summary>
    public static IReadOnlyDictionary<string, HashSet<string>> ComputeMustActiveEntitiesByScene(
        IReadOnlyList<ScenarioPathPrefixSummary> prefixSummaries)
    {
        var byScene = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        var initialized = new HashSet<string>(StringComparer.Ordinal);

        foreach (var summary in prefixSummaries)
        {
            var sceneId = summary.PathSceneIds.Last();

            var activeNames = summary.DefinitelyPresentEntities
                .Select(e => e.Name.Trim())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!initialized.Contains(sceneId))
            {
                // First prefix for this scene: initialize with its active set
                byScene[sceneId] = activeNames;
                initialized.Add(sceneId);
            }
            else
            {
                // Intersect with existing must-set
                byScene[sceneId].IntersectWith(activeNames);
            }
        }

        return byScene;
    }
}
