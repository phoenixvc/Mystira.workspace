namespace Mystira.StoryGenerator.GraphTheory.DataFlowAnalysis;

/// <summary>
/// Generic forward data-flow analysis utilities for directed graphs.
/// </summary>
public static class DataFlowAnalysis
{
    /// <summary>
    /// Computes, for each node in a directed graph, the set of entities that
    /// must have been introduced on all possible paths from a designated
    /// start node to that node.
    ///
    /// This is a classic forward "must" data-flow analysis:
    /// <code>
    ///   must[start] = introduced(start) \ removed(start)
    ///   must[v]     = (⋂ must[p]) ∪ introduced(v) \ removed(v)
    /// </code>
    /// where <c>p</c> ranges over all predecessors of <c>v</c>.
    ///
    /// Intuition:
    /// An entity is in <c>must[v]</c> if and only if it has definitely
    /// been introduced along every path that can reach <c>v</c>, taking
    /// into account any explicit removals.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The entity identifier type (e.g., string, Guid, etc.).
    /// Must support a stable equality semantics suitable for use
    /// in <see cref="HashSet{T}"/>.
    /// </typeparam>
    /// <param name="nodes">
    /// The complete set of graph nodes, keyed by node id.
    /// The graph may contain cycles; the analysis will iterate
    /// until a fixed point is reached.
    /// </param>
    /// <param name="startSceneId">
    /// The id of the designated start node. This is treated as the
    /// entry point of the analysis; entities present in
    /// <see cref="DataFlowNode{TEntity}.IntroducedEntities"/> for this node
    /// are considered introduced at the start of all paths.
    /// </param>
    /// <returns>
    /// A dictionary mapping each node id to the final "must-have-been
    /// introduced" set at that node, after applying all introductions
    /// and removals propagated from its predecessors.
    ///
    /// More formally, for each node <c>v</c>, the resulting set
    /// <c>must[v]</c> contains exactly those entities that:
    /// <list type="bullet">
    ///   <item>have been introduced on every path from the start node to <c>v</c>, and</item>
    ///   <item>have not been removed along any such path after their introduction.</item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="startSceneId"/> does not exist in
    /// <paramref name="nodes"/>.
    /// </exception>
    ///
    /// <remarks>
    /// Usage examples:
    /// <list type="bullet">
    ///   <item>Static analysis: computing "definitely initialized" variables at each program point.</item>
    ///   <item>Graph validation: checking that resources are always acquired before use along all paths.</item>
    ///   <item>State tracking: identifying entities that must be active / present when reaching a node.</item>
    /// </list>
    ///
    /// Complexity:
    /// The algorithm is a standard worklist-based fixed-point iteration.
    /// In the worst case, each edge may cause a node to be reprocessed
    /// multiple times until sets stabilize. For finite graphs with
    /// monotone set operations (union / intersection / difference),
    /// the analysis always terminates.
    /// </remarks>
    public static Dictionary<string, HashSet<TEntity>> ComputeMustIntroducedSets<TEntity>(
        IReadOnlyDictionary<string, DataFlowNode<TEntity>> nodes,
        string startSceneId)
    {
        if (!nodes.TryGetValue(startSceneId, out var startNode))
            throw new ArgumentException($"Start scene '{startSceneId}' not found in node set.", nameof(startSceneId));

        // Result map: nodeId -> must-set
        var must = nodes.Keys.ToDictionary(
            id => id,
            id => new HashSet<TEntity>());

        // Initialize start node:
        // must[start] = introduced(start) \ removed(start)
        must[startSceneId].UnionWith(startNode.IntroducedEntities);
        must[startSceneId].ExceptWith(startNode.RemovedEntities);

        // Worklist for classic iterative data-flow
        var worklist = new Queue<string>();

        // Start with the start node
        worklist.Enqueue(startSceneId);

        // Optionally enqueue its successors initially
        foreach (var succId in startNode.SuccessorIds)
        {
            if (nodes.ContainsKey(succId))
                worklist.Enqueue(succId);
        }

        // To avoid enqueuing the same node too many times
        var inQueue = new HashSet<string>(worklist);

        while (worklist.Count > 0)
        {
            var sceneId = worklist.Dequeue();
            inQueue.Remove(sceneId);

            var node = nodes[sceneId];

            // Compute new must-set for this node from its predecessors
            var newMust = new HashSet<TEntity>();

            if (node.PredecessorIds.Count == 0)
            {
                // Root nodes (no predecessors).
                // For the designated start node, we already know the intended
                // initialization; for other roots, treat them as starting
                // from their own introduced set.
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
                // Start from intersection of must[p] for all predecessors p
                bool first = true;
                foreach (var predId in node.PredecessorIds)
                {
                    if (!nodes.ContainsKey(predId))
                        continue; // Optionally: throw if graph integrity must be strict

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

                // Add entities introduced at this node
                newMust.UnionWith(node.IntroducedEntities);

                // Remove entities explicitly removed at this node
                newMust.ExceptWith(node.RemovedEntities);
            }

            // If the must-set changed, propagate to successors
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
