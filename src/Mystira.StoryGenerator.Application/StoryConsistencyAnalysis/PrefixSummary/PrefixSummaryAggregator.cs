using Mystira.StoryGenerator.Contracts.Entities;
using Mystira.StoryGenerator.Contracts.StoryConsistency;

namespace Mystira.StoryGenerator.Application.StoryConsistencyAnalysis.PrefixSummary;

public static class PrefixSummaryAggregator
{
    /// <summary>
    /// From a collection of prefix summaries, computes for each scene the set of
    /// entities that are definitely present at that scene across all branches
    /// that reach it.
    ///
    /// For a scene v:
    ///   must[v] = ⋂ { DefinitelyPresentEntities(prefix) | prefix ends at v }
    ///
    /// I.e. an entity is in must[v] iff every prefix that ends at v says
    /// it is definitely present at the end of that prefix.
    /// </summary>
    /// <param name="prefixSummaries">
    /// Sequence of prefix summaries, each ending at some scene
    /// (the last element of PathSceneIds).
    /// </param>
    /// <param name="entityComparer">
    /// Optional equality comparer for SceneEntity; if null, default comparer is used.
    /// You probably want a comparer that keys on (Name, Type, IsProperNoun).
    /// </param>
    /// <returns>
    /// Dictionary mapping sceneId -> set of entities that are
    /// definitely present at that scene across all merged branches.
    /// </returns>
    public static Dictionary<string, HashSet<SceneEntity>> ComputeMustActivePerScene(
        IEnumerable<ScenarioPathPrefixSummary> prefixSummaries,
        IEqualityComparer<SceneEntity>? entityComparer = null)
    {
        if (prefixSummaries == null) throw new ArgumentNullException(nameof(prefixSummaries));

        entityComparer ??= EqualityComparer<SceneEntity>.Default;

        var perSceneMust = new Dictionary<string, HashSet<SceneEntity>>();

        foreach (var pathSummary in prefixSummaries)
        {
            if (pathSummary.PathSceneIds.Count == 0)
                continue; // or throw, depending on how strict you want to be

            var sceneId = pathSummary.PathSceneIds[^1];

            // This branch's "definitely present" set at the scene
            var branchSet = new HashSet<SceneEntity>(
                pathSummary.DefinitelyPresentEntities,
                entityComparer
            );

            if (!perSceneMust.TryGetValue(sceneId, out var acc))
            {
                // First time we see this scene: initialize with this branch's set
                perSceneMust[sceneId] = branchSet;
            }
            else
            {
                // Subsequent branches: intersect with what we already had
                acc.IntersectWith(branchSet);
            }
        }

        return perSceneMust;
    }

    public static Dictionary<string, HashSet<SceneEntity>> BuildPerSceneMaybePresent(
        IEnumerable<ScenarioPathPrefixSummary> prefixSummaries,
        IEqualityComparer<SceneEntity>? entityComparer = null)
    {
        entityComparer ??= EqualityComparer<SceneEntity>.Default;

        var perSceneMaybe = new Dictionary<string, HashSet<SceneEntity>>(StringComparer.Ordinal);

        foreach (var pathSummary in prefixSummaries)
        {
            if (pathSummary.PathSceneIds.Count == 0)
                continue;

            var sceneId = pathSummary.PathSceneIds.Last();

            if (!perSceneMaybe.TryGetValue(sceneId, out var acc))
            {
                acc = new HashSet<SceneEntity>(pathSummary.MaybePresentEntities ?? Enumerable.Empty<SceneEntity>(),
                    entityComparer);
                perSceneMaybe[sceneId] = acc;
            }
            else
            {
                // Union: anything that is maybe-present in any prefix is maybe-present at this scene.
                acc.UnionWith(pathSummary.MaybePresentEntities);
            }
        }

        return perSceneMaybe;
    }

    public static Dictionary<string, HashSet<SceneEntity>> BuildPerSceneDefinitelyAbsent(
        IEnumerable<ScenarioPathPrefixSummary> prefixSummaries,
        IEqualityComparer<SceneEntity>? entityComparer = null)
    {
        entityComparer ??= EqualityComparer<SceneEntity>.Default;

        var perSceneAbsent = new Dictionary<string, HashSet<SceneEntity>>(StringComparer.Ordinal);
        var perSceneIsFirst = new HashSet<string>(StringComparer.Ordinal);

        foreach (var pathSummary in prefixSummaries)
        {
            if (pathSummary.PathSceneIds.Count == 0)
                continue;

            var sceneId = pathSummary.PathSceneIds.Last();

            if (!perSceneAbsent.TryGetValue(sceneId, out var acc))
            {
                // First time we see this scene: initialize with current definitely-absent set
                acc = new HashSet<SceneEntity>(pathSummary.DefinitelyAbsentEntities,
                    entityComparer);
                perSceneAbsent[sceneId] = acc;
                perSceneIsFirst.Add(sceneId);
            }
            else
            {
                // Intersection: to be "definitely absent" at this scene,
                // the entity must be marked definitely-absent in every prefix ending here.
                var current = new HashSet<SceneEntity>(pathSummary.DefinitelyAbsentEntities ?? Enumerable.Empty<SceneEntity>(),
                    entityComparer);

                if (perSceneIsFirst.Contains(sceneId))
                {
                    perSceneIsFirst.Remove(sceneId);
                    acc.IntersectWith(current);
                }
                else
                {
                    acc.IntersectWith(current);
                }
            }
        }

        return perSceneAbsent;
    }
}
