using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Domain.Graph;

// Legacy type kept for compatibility; prefer string edge labels for scenario graphs.
public sealed record SceneEdge(Scene From, Scene To, string Label) : IEdge<Scene, string>;
