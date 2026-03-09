namespace Mystira.StoryGenerator.Domain.Stories;

// Intentionally left minimal in Domain to avoid taking a dependency on GraphTheory.
// Graph construction extensions live in the Application project under the same
// namespace (Mystira.StoryGenerator.Domain.Stories) so consumers can call
// scenario.ToGraph() when referencing the Application layer.
public static class ScenarioExtensions
{
}
