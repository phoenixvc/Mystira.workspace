namespace Mystira.Shared.GraphTheory.DataFlow;

/// <summary>
/// Generic forward data-flow analysis utilities for directed graphs.
/// </summary>
public static class DataFlowAnalysis
{
    /// <summary>
    /// Computes, for each node in a directed graph, the set of entities that
    /// must have been introduced on all possible paths from a designated
    /// start node to that node.
    /// </summary>
    /// <typeparam name="TEntity">The entity identifier type.</typeparam>
    /// <param name="nodes">The complete set of graph nodes, keyed by node id.</param>
    /// <param name="startSceneId">The id of the designated start node.</param>
    /// <returns>A dictionary mapping each node id to the "must-have-been-introduced" set.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if startSceneId does not exist in nodes.
    /// </exception>
    public static Dictionary<string, HashSet<TEntity>> ComputeMustIntroducedSets<TEntity>(
        IReadOnlyDictionary<string, DataFlowNode<TEntity>> nodes,
        string startSceneId)
    {
        if (!nodes.TryGetValue(startSceneId, out var startNode))
            throw new ArgumentException($"Start scene '{startSceneId}' not found in node set.", nameof(startSceneId));

        var must = nodes.Keys.ToDictionary(id => id, _ => new HashSet<TEntity>());

        must[startSceneId].UnionWith(startNode.IntroducedEntities);
        must[startSceneId].ExceptWith(startNode.RemovedEntities);

        var worklist = new Queue<string>();
        worklist.Enqueue(startSceneId);

        foreach (var succId in startNode.SuccessorIds)
        {
            if (nodes.ContainsKey(succId))
                worklist.Enqueue(succId);
        }

        var inQueue = new HashSet<string>(worklist);

        while (worklist.Count > 0)
        {
            var sceneId = worklist.Dequeue();
            inQueue.Remove(sceneId);

            var node = nodes[sceneId];
            var newMust = new HashSet<TEntity>();

            if (node.PredecessorIds.Count == 0)
            {
                if (!sceneId.Equals(startSceneId, StringComparison.Ordinal))
                {
                    newMust.UnionWith(node.IntroducedEntities);
                    newMust.ExceptWith(node.RemovedEntities);
                }
                else
                {
                    newMust.UnionWith(startNode.IntroducedEntities);
                    newMust.ExceptWith(startNode.RemovedEntities);
                }
            }
            else
            {
                bool first = true;
                foreach (var predId in node.PredecessorIds)
                {
                    if (!nodes.ContainsKey(predId))
                        continue;

                    var predMust = must[predId];

                    if (first)
                    {
                        newMust = new HashSet<TEntity>(predMust);
                        first = false;
                    }
                    else
                    {
                        newMust.IntersectWith(predMust);
                    }
                }

                newMust.UnionWith(node.IntroducedEntities);
                newMust.ExceptWith(node.RemovedEntities);
            }

            var oldMust = must[sceneId];
            if (!SetEquals(oldMust, newMust))
            {
                must[sceneId] = newMust;

                foreach (var succId in node.SuccessorIds)
                {
                    if (!nodes.ContainsKey(succId))
                        continue;

                    if (!inQueue.Contains(succId))
                    {
                        worklist.Enqueue(succId);
                        inQueue.Add(succId);
                    }
                }
            }
        }

        return must;
    }

    private static bool SetEquals<TEntity>(HashSet<TEntity> a, HashSet<TEntity> b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a.Count != b.Count) return false;
        return a.SetEquals(b);
    }
}
