using Mystira.StoryGenerator.Contracts.Entities;
using Mystira.StoryGenerator.Contracts.StoryConsistency;

public static class PrefixSummaryAggregator
{
    public static PrefixAggregateResult AggregatePerScenePresence(
        IEnumerable<ScenarioPathPrefixSummary> prefixSummaries,
        IEqualityComparer<SceneEntity>? entityComparer = null)
    {
        if (prefixSummaries == null) throw new ArgumentNullException(nameof(prefixSummaries));

        entityComparer ??= EqualityComparer<SceneEntity>.Default;

        // For each sceneId we track:
        // - unionAll: ⋃ P_i
        // - interAll: ⋂ P_i
        var unionAll = new Dictionary<string, HashSet<SceneEntity>>(StringComparer.Ordinal);
        var interAll = new Dictionary<string, HashSet<SceneEntity>>(StringComparer.Ordinal);

        foreach (var pathSummary in prefixSummaries)
        {
            if (pathSummary.PrefixSceneIds.Count == 0)
                continue;

            var sceneId = pathSummary.PrefixSceneId;

            // P_i = DefinitelyPresent ∪ MaybePresent for this prefix
            var presentThisPrefix = new HashSet<SceneEntity>(entityComparer);
            presentThisPrefix.UnionWith(pathSummary.DefinitelyPresentEntities);
            presentThisPrefix.UnionWith(pathSummary.MaybePresentEntities);

            if (!unionAll.TryGetValue(sceneId, out var unionSet))
            {
                unionSet = new HashSet<SceneEntity>(presentThisPrefix, entityComparer);
                unionAll[sceneId] = unionSet;

                var interSet = new HashSet<SceneEntity>(presentThisPrefix, entityComparer);
                interAll[sceneId] = interSet;
            }
            else
            {
                unionSet.UnionWith(presentThisPrefix);
                interAll[sceneId].IntersectWith(presentThisPrefix);
            }
        }

        // Now derive must + maybe from unionAll / interAll
        var must = new Dictionary<string, HashSet<SceneEntity>>(StringComparer.Ordinal);
        var maybe = new Dictionary<string, HashSet<SceneEntity>>(StringComparer.Ordinal);

        foreach (var kvp in unionAll)
        {
            var sceneId = kvp.Key;
            var unionSet = kvp.Value;
            var interSet = interAll[sceneId];

            // must = intersection (entities present on all prefixes)
            must[sceneId] = new HashSet<SceneEntity>(interSet, entityComparer);

            // maybe = union \ must (entities present on some but not all prefixes)
            var maybeSet = new HashSet<SceneEntity>(unionSet, entityComparer);
            maybeSet.ExceptWith(interSet);
            maybe[sceneId] = maybeSet;
        }

        return new PrefixAggregateResult(must, maybe);
    }

    public static Dictionary<string, HashSet<SceneEntity>> BuildPerSceneDefinitelyAbsent(
        IEnumerable<ScenarioPathPrefixSummary> prefixSummaries,
        IEqualityComparer<SceneEntity>? entityComparer = null)
    {
        entityComparer ??= EqualityComparer<SceneEntity>.Default;

        var perSceneAbsent = new Dictionary<string, HashSet<SceneEntity>>(StringComparer.Ordinal);
        var firstSeen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var pathSummary in prefixSummaries)
        {
            if (pathSummary.PrefixSceneIds.Count == 0)
                continue;

            var sceneId = pathSummary.PrefixSceneId;

            if (!perSceneAbsent.TryGetValue(sceneId, out var acc))
            {
                acc = new HashSet<SceneEntity>(
                    pathSummary.DefinitelyAbsentEntities,
                    entityComparer);

                perSceneAbsent[sceneId] = acc;
                firstSeen.Add(sceneId);
            }
            else
            {
                var current = new HashSet<SceneEntity>(
                    pathSummary.DefinitelyAbsentEntities,
                    entityComparer);

                if (firstSeen.Contains(sceneId))
                {
                    firstSeen.Remove(sceneId);
                    acc.IntersectWith(current);
                }
                else
                    acc.IntersectWith(current);
            }
        }

        return perSceneAbsent;
    }

    public static Dictionary<string, HashSet<SceneEntity>> ComputeMustActivePerScene(
        IEnumerable<ScenarioPathPrefixSummary> prefixSummaries,
        IEqualityComparer<SceneEntity>? entityComparer = null)
    {
        return AggregatePerScenePresence(prefixSummaries, entityComparer).MustActivePerScene;
    }

    public static Dictionary<string, HashSet<SceneEntity>> ComputeMaybeActivePerScene(
        IEnumerable<ScenarioPathPrefixSummary> prefixSummaries,
        IEqualityComparer<SceneEntity>? entityComparer = null)
    {
        return AggregatePerScenePresence(prefixSummaries, entityComparer).MaybePresentPerScene;
    }
}

public sealed record PrefixAggregateResult(
    Dictionary<string, HashSet<SceneEntity>> MustActivePerScene,
    Dictionary<string, HashSet<SceneEntity>> MaybePresentPerScene);
