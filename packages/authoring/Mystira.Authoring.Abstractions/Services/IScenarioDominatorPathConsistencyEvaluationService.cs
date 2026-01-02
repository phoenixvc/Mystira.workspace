using Mystira.Authoring.Abstractions.Models.Scenario;
using Mystira.Contracts.StoryGenerator.StoryConsistency;

namespace Mystira.Authoring.Abstractions.Services;

/// <summary>
/// Service for evaluating consistency along dominator paths within scenarios.
/// Dominator paths are critical paths where one scene must precede another on all possible routes.
/// </summary>
public interface IScenarioDominatorPathConsistencyEvaluationService
{
    /// <summary>
    /// Evaluates consistency across all dominator paths in a scenario.
    /// </summary>
    /// <param name="scenario">The scenario to evaluate.</param>
    /// <param name="request">Evaluation request parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dominator path consistency evaluation response.</returns>
    Task<EvaluateDominatorPathConsistencyResponse> EvaluateAsync(
        Scenario scenario,
        EvaluateDominatorPathConsistencyRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates consistency for a specific dominator path.
    /// </summary>
    /// <param name="scenario">The scenario containing the path.</param>
    /// <param name="targetSceneId">The target scene ID (the dominated scene).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Analysis result for the dominator path to the target scene.</returns>
    Task<DominatorPathAnalysisResult> EvaluateDominatorPathAsync(
        Scenario scenario,
        string targetSceneId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the guaranteed entity state at a scene based on dominator analysis.
    /// This represents what entities must be present/absent regardless of the path taken.
    /// </summary>
    /// <param name="scenario">The scenario containing the scene.</param>
    /// <param name="sceneId">The scene ID to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The guaranteed entity state at the scene.</returns>
    Task<DominatorPathEntityState> GetGuaranteedEntityStateAsync(
        Scenario scenario,
        string sceneId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Computes the dominator tree for a scenario.
    /// </summary>
    /// <param name="scenario">The scenario to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary mapping each scene ID to its immediate dominator scene ID (null if none).</returns>
    Task<IReadOnlyDictionary<string, string?>> ComputeDominatorTreeAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default);
}
