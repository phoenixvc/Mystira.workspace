namespace Mystira.StoryGenerator.Application.StoryConsistencyAnalysis.PrefixSummary;

using System;
using System.Collections.Generic;
using System.Linq;
using Contracts.Entities;
using Contracts.StoryConsistency;

public static class PrefixSummaryMerger
{
    /// <summary>
    /// For each scene, compute the set of entities that are guaranteed
    /// to be active at entry to that scene across all prefixes that end there.
    ///
    /// For scene v:
    ///   must_active[v] = ⋂ { DefinitelyPresentEntities(prefix) | prefix ends at v }
    ///
    /// Entities are compared using the provided entityComparer (or default).
    /// </summary>
    public static IReadOnlyDictionary<string, HashSet<SceneEntity>> ComputeMustActiveEntitiesByScene(
        IEnumerable<ScenarioPathPrefixSummary> prefixSummaries,
        IEqualityComparer<SceneEntity>? entityComparer = null)
    {
        if (prefixSummaries == null) throw new ArgumentNullException(nameof(prefixSummaries));

        entityComparer ??= EqualityComparer<SceneEntity>.Default;

        var byScene = new Dictionary<string, HashSet<SceneEntity>>(StringComparer.Ordinal);
        var initialized = new HashSet<string>(StringComparer.Ordinal);

        foreach (var summary in prefixSummaries)
        {
            var sceneId = summary.PrefixSceneId;
            if (string.IsNullOrWhiteSpace(sceneId))
                continue;

            var activeSet = new HashSet<SceneEntity>(entityComparer);

            foreach (var e in summary.DefinitelyPresentEntities ?? Enumerable.Empty<SceneEntity>())
            {
                if (e != null)
                    activeSet.Add(e);
            }

            if (!initialized.Contains(sceneId))
            {
                byScene[sceneId] = activeSet;
                initialized.Add(sceneId);
            }
            else
            {
                byScene[sceneId].IntersectWith(activeSet);
            }
        }

        return byScene;
    }

    /// <summary>
    /// For each scene, compute the set of entities that may be active
    /// at entry to that scene on at least one prefix, but are not guaranteed
    /// to be active on all prefixes.
    ///
    /// For scene v:
    ///   P_i = DefinitelyPresent ∪ MaybePresent for prefix i ending at v
    ///   union_all[v] = ⋃ P_i
    ///   must_active[v] = as computed by ComputeMustActiveEntitiesByScene
    ///   maybe_active[v] = union_all[v] \ must_active[v]
    ///
    /// Entities are compared using the provided entityComparer (or default).
    /// </summary>
    public static IReadOnlyDictionary<string, HashSet<SceneEntity>> ComputeMaybeActiveEntitiesByScene(
        IEnumerable<ScenarioPathPrefixSummary> prefixSummaries,
        IEqualityComparer<SceneEntity>? entityComparer = null)
    {
        if (prefixSummaries == null) throw new ArgumentNullException(nameof(prefixSummaries));

        entityComparer ??= EqualityComparer<SceneEntity>.Default;

        var mustActive = ComputeMustActiveEntitiesByScene(prefixSummaries, entityComparer);
        var unionAll = new Dictionary<string, HashSet<SceneEntity>>(StringComparer.Ordinal);

        foreach (var summary in prefixSummaries)
        {
            var sceneId = summary.PrefixSceneId;
            if (string.IsNullOrWhiteSpace(sceneId))
                continue;

            var presentThisPrefix = new HashSet<SceneEntity>(entityComparer);

            foreach (var e in summary.DefinitelyPresentEntities ?? Enumerable.Empty<SceneEntity>())
            {
                if (e != null)
                    presentThisPrefix.Add(e);
            }

            foreach (var e in summary.MaybePresentEntities ?? Enumerable.Empty<SceneEntity>())
            {
                if (e != null)
                    presentThisPrefix.Add(e);
            }

            if (!unionAll.TryGetValue(sceneId, out var unionSet))
            {
                unionAll[sceneId] = presentThisPrefix;
            }
            else
            {
                unionSet.UnionWith(presentThisPrefix);
            }
        }

        var maybeActive = new Dictionary<string, HashSet<SceneEntity>>(StringComparer.Ordinal);

        foreach (var kvp in unionAll)
        {
            var sceneId = kvp.Key;
            var unionSet = kvp.Value;

            var maybeSet = new HashSet<SceneEntity>(unionSet, entityComparer);

            if (mustActive.TryGetValue(sceneId, out var mustSet))
                maybeSet.ExceptWith(mustSet);

            maybeActive[sceneId] = maybeSet;
        }

        return maybeActive;
    }

    /// <summary>
    /// For each scene, compute the set of entities that are definitely
    /// absent/removed at entry to that scene across all prefixes that end there.
    ///
    /// For scene v:
    ///   absent[v] = ⋂ { DefinitelyAbsentEntities(prefix) | prefix ends at v }
    ///
    /// Entities are compared using the provided entityComparer (or default).
    /// </summary>
    public static IReadOnlyDictionary<string, HashSet<SceneEntity>> ComputeDefinitelyRemovedEntitiesByScene(
        IEnumerable<ScenarioPathPrefixSummary> prefixSummaries,
        IEqualityComparer<SceneEntity>? entityComparer = null)
    {
        if (prefixSummaries == null) throw new ArgumentNullException(nameof(prefixSummaries));

        entityComparer ??= EqualityComparer<SceneEntity>.Default;

        var byScene = new Dictionary<string, HashSet<SceneEntity>>(StringComparer.Ordinal);
        var initialized = new HashSet<string>(StringComparer.Ordinal);

        foreach (var summary in prefixSummaries)
        {
            var sceneId = summary.PrefixSceneId;
            if (string.IsNullOrWhiteSpace(sceneId))
                continue;

            var absentSet = new HashSet<SceneEntity>(entityComparer);

            foreach (var e in summary.DefinitelyAbsentEntities ?? Enumerable.Empty<SceneEntity>())
            {
                if (e != null)
                    absentSet.Add(e);
            }

            if (!initialized.Contains(sceneId))
            {
                byScene[sceneId] = absentSet;
                initialized.Add(sceneId);
            }
            else
            {
                byScene[sceneId].IntersectWith(absentSet);
            }
        }

        return byScene;
    }
}
