using System.Collections.ObjectModel;
using Mystira.StoryGenerator.GraphTheory.Graph;

namespace Mystira.StoryGenerator.GraphTheory.FrontierMergedGraph;

/// <summary>
/// <para>
/// Provides factory methods for constructing frontier-merged state-space graphs.
/// </para>
/// <para>
/// A frontier-merged graph is built by: <br />
/// - Starting from an initial (scene, concreteState), <br />
/// - Exploring successors using a user-supplied transition function, and <br />
/// - Merging any two concrete states that share the same (sceneId, stateSignature).
/// </para>
/// <para>
/// The resulting graph is defined over <see cref="SceneStateNode{TSceneId, TStateSig}"/>
/// nodes and can be used for effective path counting, consistency checks, and
/// end-state analysis, without enumerating every raw path individually.
/// </para>
/// </summary>
public static class FrontierMergedGraphBuilder
{
    /// <summary>
    /// <para>
    /// Builds a frontier-merged directed graph over (sceneId, stateSignature)
    /// nodes by exploring the state-space from an initial scene and state.
    /// </para>
    /// <para>
    /// During exploration, whenever two concrete states reach the same scene
    /// with the same abstract state signature, they are merged into a single
    /// <see cref="SceneStateNode{TSceneId, TStateSig}"/> node. This prevents
    /// combinatorial explosion in branching systems while preserving the
    /// information required for downstream checks.
    /// </para>
    /// </summary>
    /// <typeparam name="TSceneId">
    /// <para>
    /// The type of scene identifiers (for example, string or GUID).
    /// </para>
    /// </typeparam>
    /// <typeparam name="TState">
    /// <para>
    /// The type of the concrete story state used during exploration.
    /// This is your full state object (entities, flags, time, etc.).
    /// </para>
    /// </typeparam>
    /// <typeparam name="TStateSig">
    /// <para>
    /// The type of the abstract state signature used to decide when two
    /// concrete states can be merged. It should capture exactly the parts
    /// of state that matter for your invariants (for example, known entities,
    /// critical flags, time bucket).
    /// </para>
    /// </typeparam>
    /// <typeparam name="TEdgeLabel">
    /// <para>
    /// The type of labels attached to edges in the merged graph, typically
    /// capturing transition kind and metadata (choice ID, roll outcome, etc.).
    /// </para>
    /// </typeparam>
    /// <param name="initialSceneId">
    /// <para>
    /// The starting scene identifier for exploration.
    /// </para>
    /// </param>
    /// <param name="initialState">
    /// <para>
    /// The initial concrete story state at <paramref name="initialSceneId"/>.
    /// </para>
    /// </param>
    /// <param name="getTransitions">
    /// <para>
    /// A function that, given a current scene identifier and concrete state,
    /// returns the outgoing transitions (successor scenes, labels, and next
    /// concrete states).
    /// </para>
    /// <para>
    /// This encapsulates all domain-specific branching logic (choices, roll
    /// outcomes, conditional gating, and state updates).
    /// </para>
    /// </param>
    /// <param name="stateSignature">
    /// <para>
    /// A function that maps a concrete <typeparamref name="TState"/> to an
    /// abstract <typeparamref name="TStateSig"/> used for merging.
    /// </para>
    /// <para>
    /// Two concrete states that yield the same <typeparamref name="TStateSig"/>
    /// for the same scene are treated as equivalent and represented by a single
    /// node in the merged graph.
    /// </para>
    /// </param>
    /// <param name="isTerminalScene">
    /// <para>
    /// Optional predicate to mark scenes as terminal (for example, ending or
    /// special final scenes). When this returns <c>true</c> for a scene, the
    /// corresponding node is marked as terminal and not expanded further.
    /// </para>
    /// <para>
    /// If omitted, only dead-ends (no outgoing transitions) and nodes where
    /// the maximum depth is reached are considered terminal.
    /// </para>
    /// </param>
    /// <param name="maxDepth">
    /// <para>
    /// Optional maximum exploration depth. When provided, any node reached at
    /// depth greater than or equal to this value is marked as terminal and
    /// not expanded further.
    /// </para>
    /// <para>
    /// This is primarily a safety mechanism in the presence of cycles or
    /// unexpected branching behaviour.
    /// </para>
    /// </param>
    /// <returns>
    /// <para>
    /// A <see cref="FrontierMergedGraphResult{TSceneId, TState, TStateSig, TEdgeLabel}"/>
    /// containing:
    /// </para>
    /// <para>
    /// - The merged directed graph over <see cref="SceneStateNode{TSceneId, TStateSig}"/> nodes,
    /// - A representative concrete state for each merged node, and
    /// - The set of terminal nodes (endings, dead-ends, or depth-limited nodes).
    /// </para>
    /// </returns>
    public static FrontierMergedGraphResult<TSceneId, TState, TStateSig, TEdgeLabel> Build<TSceneId, TState, TStateSig, TEdgeLabel>(
        TSceneId initialSceneId,
        TState initialState,
        Func<TSceneId, TState, IEnumerable<SceneTransition<TSceneId, TEdgeLabel, TState>>> getTransitions,
        Func<TState, TStateSig> stateSignature,
        Func<TSceneId, bool>? isTerminalScene = null,
        int? maxDepth = null)
        where TSceneId : notnull
        where TStateSig : notnull
    {
        isTerminalScene ??= (_scene) => false;

        // Containers to build the immutable graph
        var nodes = new HashSet<SceneStateNode<TSceneId, TStateSig>>();
        var edges = new List<Edge<SceneStateNode<TSceneId, TStateSig>, TEdgeLabel>>();

        // Map from merged node → a representative underlying state
        var representativeState = new Dictionary<SceneStateNode<TSceneId, TStateSig>, TState>();

        // Track which nodes are terminal
        var terminalNodes = new HashSet<SceneStateNode<TSceneId, TStateSig>>();

        // BFS / frontier queue of (node, underlying state, depth)
        var queue = new Queue<(SceneStateNode<TSceneId, TStateSig> Node, TState State, int Depth)>();

        // Initialize with the root
        var initialSig = stateSignature(initialState);
        var initialNode = new SceneStateNode<TSceneId, TStateSig>(initialSceneId, initialSig);

        nodes.Add(initialNode);
        representativeState[initialNode] = initialState;
        queue.Enqueue((initialNode, initialState, depth: 0));

        while (queue.Count > 0)
        {
            var (currentNode, currentState, depth) = queue.Dequeue();
            var currentSceneId = currentNode.SceneId;

            // Respect maxDepth if supplied
            if (maxDepth.HasValue && depth >= maxDepth.Value)
            {
                terminalNodes.Add(currentNode);
                continue;
            }

            // Check terminal condition (e.g., ending scenes)
            if (isTerminalScene(currentSceneId))
            {
                terminalNodes.Add(currentNode);
                continue;
            }

            // Get outgoing transitions from the underlying story logic
            var outgoingTransitions = getTransitions(currentSceneId, currentState)?.ToList()
                                      ?? new List<SceneTransition<TSceneId, TEdgeLabel, TState>>();

            if (outgoingTransitions.Count == 0)
            {
                // Dead-end in the graph
                terminalNodes.Add(currentNode);
                continue;
            }

            foreach (var transition in outgoingTransitions)
            {
                var nextSceneId = transition.ToScene;
                var edgeLabel   = transition.Label;
                var nextState   = transition.NextState;

                var nextSig  = stateSignature(nextState);
                var nextNode = new SceneStateNode<TSceneId, TStateSig>(nextSceneId, nextSig);

                // If we've never seen this merged node, register it and enqueue for exploration
                if (!nodes.Contains(nextNode))
                {
                    nodes.Add(nextNode);
                    representativeState[nextNode] = nextState;
                    queue.Enqueue((nextNode, nextState, depth + 1));
                }
                else
                {
                    // We already have this (scene, signature) – frontier merging happens here.
                    // Optionally: you could merge some aggregated info in representativeState, if needed.
                }

                // Add edge in the merged graph
                edges.Add(new Edge<SceneStateNode<TSceneId, TStateSig>, TEdgeLabel>(currentNode, nextNode, edgeLabel));
            }
        }

        // Build the immutable DirectedGraph
        var graph = DirectedGraph<SceneStateNode<TSceneId, TStateSig>, TEdgeLabel>.FromEdges(
            edges,
            nodes: nodes);

        return new FrontierMergedGraphResult<TSceneId, TState, TStateSig, TEdgeLabel>(
            graph,
            new ReadOnlyDictionary<SceneStateNode<TSceneId, TStateSig>, TState>(representativeState),
            new ReadOnlyCollection<SceneStateNode<TSceneId, TStateSig>>(terminalNodes.ToList()));
    }
}
