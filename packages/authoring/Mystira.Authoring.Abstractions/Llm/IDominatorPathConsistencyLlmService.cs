using Mystira.Contracts.StoryGenerator.StoryConsistency;

namespace Mystira.Authoring.Abstractions.Llm;

/// <summary>
/// LLM service for evaluating consistency along dominator paths.
/// Analyzes entity states and narrative consistency at dominator tree points.
/// </summary>
public interface IDominatorPathConsistencyLlmService
{
    /// <summary>
    /// Evaluates consistency for a dominator path.
    /// </summary>
    /// <param name="pathSceneIds">The scene IDs in the dominator path.</param>
    /// <param name="pathContent">The concatenated story content for the path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Consistency evaluation result for the dominator path.</returns>
    Task<ConsistencyEvaluationResult> EvaluatePathAsync(
        IEnumerable<string> pathSceneIds,
        string pathContent,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates entity state consistency at a dominator tree node.
    /// Checks that entities in the guaranteed state are properly used.
    /// </summary>
    /// <param name="dominatorSceneId">The dominator scene ID.</param>
    /// <param name="targetSceneId">The target scene being dominated.</param>
    /// <param name="guaranteedEntityState">The guaranteed entity state at the dominator.</param>
    /// <param name="targetSceneContent">The content of the target scene.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of entity continuity issues found.</returns>
    Task<IReadOnlyList<EntityContinuityIssue>> EvaluateEntityStateConsistencyAsync(
        string dominatorSceneId,
        string targetSceneId,
        DominatorPathEntityState guaranteedEntityState,
        string targetSceneContent,
        CancellationToken cancellationToken = default);
}
