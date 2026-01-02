using System.Collections.ObjectModel;

namespace Mystira.Shared.GraphTheory.StateSpace;

/// <summary>
/// <para>
/// Provides factory methods for constructing frontier-merged state-space graphs.
/// </para>
/// <para>
/// A frontier-merged graph merges any two concrete states that share the same
/// (sceneId, stateSignature), preventing combinatorial explosion while preserving
/// information required for downstream checks.
/// </para>
/// </summary>
public static class FrontierMergedGraphBuilder
{
    /// <summary>
    /// Builds a frontier-merged directed graph by exploring the state-space from an initial scene and state.
    /// </summary>
    /// <typeparam name="TSceneId">The type of scene identifiers.</typeparam>
    /// <typeparam name="TState">The type of the concrete state.</typeparam>
    /// <typeparam name="TStateSig">The type of the abstract state signature.</typeparam>
    /// <typeparam name="TEdgeLabel">The type of labels attached to edges.</typeparam>
    /// <param name="initialSceneId">The starting scene identifier.</param>
    /// <param name="initialState">The initial concrete state.</param>
    /// <param name="getTransitions">Function that returns outgoing transitions for a given scene and state.</param>
    /// <param name="stateSignature">Function that maps concrete state to abstract signature.</param>
    /// <param name="isTerminalScene">Optional predicate to mark scenes as terminal.</param>
    /// <param name="maxDepth">Optional maximum exploration depth.</param>
    /// <returns>A FrontierMergedGraphResult containing the merged graph and metadata.</returns>
    public static FrontierMergedGraphResult<TSceneId, TState, TStateSig, TEdgeLabel> Build<TSceneId, TState, TStateSig, TEdgeLabel>(
        TSceneId initialSceneId,
        TState initialState,
        Func<TSceneId, TState, IEnumerable<StateTransition<TSceneId, TEdgeLabel, TState>>> getTransitions,
        Func<TState, TStateSig> stateSignature,
        Func<TSceneId, bool>? isTerminalScene = null,
        int? maxDepth = null)
        where TSceneId : notnull
        where TStateSig : notnull
    {
        isTerminalScene ??= _ => false;

        var nodes = new HashSet<StateNode<TSceneId, TStateSig>>();
        var edges = new List<Edge<StateNode<TSceneId, TStateSig>, TEdgeLabel>>();
        var representativeState = new Dictionary<StateNode<TSceneId, TStateSig>, TState>();
        var terminalNodes = new HashSet<StateNode<TSceneId, TStateSig>>();

        var queue = new Queue<(StateNode<TSceneId, TStateSig> Node, TState State, int Depth)>();

        var initialSig = stateSignature(initialState);
        var initialNode = new StateNode<TSceneId, TStateSig>(initialSceneId, initialSig);

        nodes.Add(initialNode);
        representativeState[initialNode] = initialState;
        queue.Enqueue((initialNode, initialState, depth: 0));

        while (queue.Count > 0)
        {
            var (currentNode, currentState, depth) = queue.Dequeue();
            var currentSceneId = currentNode.SceneId;

            if (maxDepth.HasValue && depth >= maxDepth.Value)
            {
                terminalNodes.Add(currentNode);
                continue;
            }

            if (isTerminalScene(currentSceneId))
            {
                terminalNodes.Add(currentNode);
                continue;
            }

            var outgoingTransitions = getTransitions(currentSceneId, currentState)?.ToList()
                                      ?? new List<StateTransition<TSceneId, TEdgeLabel, TState>>();

            if (outgoingTransitions.Count == 0)
            {
                terminalNodes.Add(currentNode);
                continue;
            }

            foreach (var transition in outgoingTransitions)
            {
                var nextSceneId = transition.ToScene;
                var edgeLabel = transition.Label;
                var nextState = transition.NextState;

                var nextSig = stateSignature(nextState);
                var nextNode = new StateNode<TSceneId, TStateSig>(nextSceneId, nextSig);

                if (!nodes.Contains(nextNode))
                {
                    nodes.Add(nextNode);
                    representativeState[nextNode] = nextState;
                    queue.Enqueue((nextNode, nextState, depth + 1));
                }

                edges.Add(new Edge<StateNode<TSceneId, TStateSig>, TEdgeLabel>(currentNode, nextNode, edgeLabel));
            }
        }

        var graph = DirectedGraph<StateNode<TSceneId, TStateSig>, TEdgeLabel>.FromEdges(edges, nodes: nodes);

        return new FrontierMergedGraphResult<TSceneId, TState, TStateSig, TEdgeLabel>(
            graph,
            new ReadOnlyDictionary<StateNode<TSceneId, TStateSig>, TState>(representativeState),
            new ReadOnlyCollection<StateNode<TSceneId, TStateSig>>(terminalNodes.ToList()));
    }
}
