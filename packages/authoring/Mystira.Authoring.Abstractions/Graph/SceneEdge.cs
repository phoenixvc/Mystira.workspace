using Mystira.Authoring.Abstractions.Models.Scenario;
using Mystira.Shared.GraphTheory;

namespace Mystira.Authoring.Abstractions.Graph;

/// <summary>
/// Represents an edge between two scenes in a scenario graph.
/// </summary>
/// <param name="From">The source scene.</param>
/// <param name="To">The target scene.</param>
/// <param name="Label">The edge label (typically the choice text that leads to this transition).</param>
public sealed record SceneEdge(Scene From, Scene To, string Label) : IEdge<Scene, string>;
