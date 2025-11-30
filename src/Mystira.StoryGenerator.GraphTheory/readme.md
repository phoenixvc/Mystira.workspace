# Mystira.Graphs — Core Graph & State-Space Library

This library provides a small, focused set of types for working with **directed graphs** and **frontier-merged state-space graphs**.

It is intentionally **mathy and domain-agnostic**: there is no “scene”, “story”, or “NPC” language in the core types. Mystira-specific logic lives in a thin adapter layer that uses these primitives.

---

## Layering Overview

There are three conceptual layers:

1. **Core Graph Layer**  
   Pure directed graphs and generic algorithms.
2. **State-Space / Frontier-Merged Layer**  
   Generic state-space exploration with state equivalence and merging.
3. **Mystira Story Adapter Layer**  
   (Lives elsewhere) Adapts Mystira stories + state transitions into the state-space layer.

Each layer depends only “downwards”, so you can unit test them in isolation.

---

## 1. Core Graph Layer

Namespace: `Mystira.Graphs`

### Types

- `Edge<TNode, TEdgeLabel>`
- `DirectedGraph<TNode, TEdgeLabel>`
- `GraphAlgorithms` (static extensions)

#### `Edge<TNode, TEdgeLabel>`

Immutable record for a directed edge:

```csharp
public sealed record Edge<TNode, TEdgeLabel>(TNode From, TNode To, TEdgeLabel Label);
```

- `TNode`: node identifier (e.g. `string`, `Guid`, or a record type).
- `TEdgeLabel`: any metadata for that edge (or a dummy/unit type if not needed).

#### `DirectedGraph<TNode, TEdgeLabel>`

Immutable, directed graph:

```csharp
public sealed class DirectedGraph<TNode, TEdgeLabel>
    where TNode : notnull
{
    public IReadOnlyCollection<TNode> Nodes { get; }
    public IReadOnlyCollection<Edge<TNode, TEdgeLabel>> Edges { get; }

    public static DirectedGraph<TNode, TEdgeLabel> FromEdges(
        IEnumerable<Edge<TNode, TEdgeLabel>> edges,
        IEnumerable<TNode>? nodes = null,
        IEqualityComparer<TNode>? comparer = null);

    public IReadOnlyList<Edge<TNode, TEdgeLabel>> GetOutgoingEdges(TNode node);
    public IReadOnlyList<Edge<TNode, TEdgeLabel>> GetIncomingEdges(TNode node);

    public IEnumerable<TNode> GetSuccessors(TNode node);
    public IEnumerable<TNode> GetPredecessors(TNode node);

    public int OutDegree(TNode node);
    public int InDegree(TNode node);

    public IEnumerable<TNode> Roots();
    public IEnumerable<TNode> Terminals();
}
```

Key points:

- Built via `FromEdges` and then **read-only**.
- Adjacency (incoming/outgoing) is precomputed for efficient queries.
- No mutation methods (`AddNode`, `RemoveNode`, etc.) — this is a pure value object.

#### `GraphAlgorithms`

Static class with generic algorithms implemented as extension methods on `DirectedGraph<,>`:

- `BreadthFirst(...)`
- `DepthFirst(...)`
- `TopologicalSort()`
- `HasCycle()`
- `EnumeratePaths(...)`

Example:

```csharp
var graph = DirectedGraph<string, string>.FromEdges(edges);

// BFS from roots
var bfsOrder = graph.BreadthFirst(graph.Roots());

// Detect cycles
bool hasCycle = graph.HasCycle();

// Enumerate simple paths from a given root to terminals
foreach (var path in graph.EnumeratePaths("start_scene"))
{
    Console.WriteLine(string.Join(" -> ", path));
}
```

This layer has **no concept of story or state**. It’s the foundation for everything else.

---

## 2. State-Space / Frontier-Merged Layer

This layer turns your branching system into a **state-space graph**, then merges states that are “equivalent enough” according to a user-provided abstraction.

### Types

- `SceneStateNode<TSceneId, TStateSig>`
- `SceneTransition<TSceneId, TEdgeLabel, TState>`
- `FrontierMergedGraphResult<TSceneId, TState, TStateSig, TEdgeLabel>`
- `FrontierMergedGraphBuilder` (static with `Build`)

#### `SceneStateNode<TSceneId, TStateSig>`

Represents a node in a frontier-merged state-space graph:

```csharp
public sealed record SceneStateNode<TSceneId, TStateSig>(
    TSceneId SceneId,
    TStateSig StateSignature)
    where TSceneId : notnull
    where TStateSig : notnull;
```

- `SceneId`: identifier of the underlying “location” (in Mystira: a scene).
- `StateSignature`: **abstract state** used for merging.
    - e.g. `(known_entities, critical_flags, time_bucket)`.

Two concrete states that share the same `(SceneId, StateSignature)` are merged into one node.

#### `SceneTransition<TSceneId, TEdgeLabel, TState>`

Represents a single transition in the state-space:

```csharp
public sealed record SceneTransition<TSceneId, TEdgeLabel, TState>(
    TSceneId ToScene,
    TEdgeLabel Label,
    TState NextState);
```

This is produced by your domain logic when exploring from `(scene, state)`.

#### `FrontierMergedGraphResult<...>`

Result object returned by the builder:

```csharp
public sealed class FrontierMergedGraphResult<TSceneId, TState, TStateSig, TEdgeLabel>
    where TSceneId : notnull
    where TStateSig : notnull
{
    public DirectedGraph<SceneStateNode<TSceneId, TStateSig>, TEdgeLabel> Graph { get; }

    public IReadOnlyDictionary<SceneStateNode<TSceneId, TStateSig>, TState> RepresentativeState { get; }

    public IReadOnlyCollection<SceneStateNode<TSceneId, TStateSig>> TerminalNodes { get; }

    // ctor omitted for brevity
}
```

- `Graph`: the merged graph over `(SceneId, StateSignature)` nodes.
- `RepresentativeState`: a concrete `TState` example per merged node.
- `TerminalNodes`: nodes considered endings / dead-ends / depth-limited.

You can run:

- Effective path counting,
- Path-by-path consistency checks,
- Entity timeline analysis, etc.

…on this merged graph without enumerating every raw path separately.

#### `FrontierMergedGraphBuilder.Build(...)`

Static factory that constructs the merged graph by exploring the state-space:

```csharp
public static class FrontierMergedGraphBuilder
{
    public static FrontierMergedGraphResult<TSceneId, TState, TStateSig, TEdgeLabel> Build<TSceneId, TState, TStateSig, TEdgeLabel>(
        TSceneId initialSceneId,
        TState initialState,
        Func<TSceneId, TState, IEnumerable<SceneTransition<TSceneId, TEdgeLabel, TState>>> getTransitions,
        Func<TState, TStateSig> stateSignature,
        Func<TSceneId, bool>? isTerminalScene = null,
        int? maxDepth = null)
        where TSceneId : notnull
        where TStateSig : notnull;
}
```

Parameters:

- `initialSceneId`: where exploration starts.
- `initialState`: initial concrete state.
- `getTransitions(scene, state)`: domain-specific transition function.
- `stateSignature(state)`: abstraction function for merging.
- `isTerminalScene(sceneId)`: optional predicate for endings.
- `maxDepth`: optional exploration depth limit.

Returns:

- `FrontierMergedGraphResult<...>` as described above.

---

## 3. Mystira Story Adapter Layer (conceptual)

This lives outside `Mystira.Graphs`, in the Mystira story engine.

Here you:

1. Define concrete types:

   ```csharp
   public record StoryState(/* known entities, flags, time, etc. */);
   public record StateSignature(/* canonical subset of StoryState */);

   public enum TransitionKind { Narrative, ChoiceOption, RollOutcome }

   public record TransitionLabel(
       TransitionKind Kind,
       string? ChoiceId,
       string? RollId);
   ```

2. Provide the transition function:

   ```csharp
   IEnumerable<SceneTransition<string, TransitionLabel, StoryState>> BuildSceneTransitions(
       string sceneId,
       StoryState state)
   {
       var scene = story.ScenesById[sceneId];

       // For each outgoing branch from this scene:
       // - compute nextSceneId,
       // - compute nextState,
       // - emit SceneTransition<string, TransitionLabel, StoryState>.
   }
   ```

3. Provide the state signature function:

   ```csharp
   StateSignature ComputeStateSignature(StoryState state)
   {
       // Keep only what matters for merging:
       // - known entities
       // - critical flags
       // - coarse time
       // Return as a value-type with stable equality.
   }
   ```

4. Build the merged graph:

   ```csharp
   var result = FrontierMergedGraphBuilder.Build<string, StoryState, StateSignature, TransitionLabel>(
       initialSceneId: "start_scene",
       initialState: initialStoryState,
       getTransitions: BuildSceneTransitions,
       stateSignature: ComputeStateSignature,
       isTerminalScene: sceneId => story.IsEndingScene(sceneId),
       maxDepth: story.MaxScenes
   );

   var mergedGraph     = result.Graph;
   var terminalNodes   = result.TerminalNodes;
   var repStates       = result.RepresentativeState;
   ```

5. Run analyses on `mergedGraph`:

    - Count effective paths using DP on the merged graph.
    - For each root→terminal path, collect scenes and states and run:
        - static checks (intro-before-use, no resurrected NPCs, no backward time),
        - LLM-based narrative/emotional consistency checks.

---

## Design Principles

- **Stateless core**: `DirectedGraph` and algorithms are immutable and side-effect-free.
- **Domain-free core**: No story/NPC/child-development concepts in the graph layer.
- **Abstraction-driven merging**:
    - You choose what goes into `StateSignature`.
    - The merged graph is correct for exactly those properties that depend only on that abstraction.
- **Composable**:
    - You can use the core graph layer for other subsystems (e.g. DM tools, puzzle graphs, quest logic) without pulling in Mystira-specific code.

---

## Next Steps / TODOs

- Add a `FrontierMergedPathCounter` helper that:
    - Takes a `FrontierMergedGraphResult`,
    - Does DP over the merged graph,
    - Returns path counts per terminal node.
- Add small test fixtures:
    - Tiny branching story with known number of raw and merged paths.
    - Unit tests for merging logic via `StateSignature`.
