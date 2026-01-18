using Mystira.Contracts.StoryGenerator.StoryConsistency;

namespace Mystira.Contracts.StoryGenerator.Services;

/// <summary>
/// LLM-based service for evaluating consistency along dominator paths.
/// </summary>
public interface IDominatorPathConsistencyLlmService
{
    /// <summary>
    /// Evaluates narrative consistency along a dominator path using LLM.
    /// </summary>
    /// <param name="path">The path to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The consistency evaluation result.</returns>
    Task<DominatorPathConsistencyResult> EvaluateAsync(
        ScenarioPath path,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Compares two paths for consistency.
    /// </summary>
    /// <param name="path1">First path.</param>
    /// <param name="path2">Second path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The comparison result.</returns>
    Task<PathComparisonResult> ComparePathsAsync(
        ScenarioPath path1,
        ScenarioPath path2,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of dominator path consistency evaluation.
/// </summary>
public class DominatorPathConsistencyResult
{
    /// <summary>
    /// Whether the path is consistent.
    /// </summary>
    public bool IsConsistent { get; set; }

    /// <summary>
    /// Consistency score (0.0 to 1.0).
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// List of inconsistencies found.
    /// </summary>
    public List<string> Inconsistencies { get; set; } = new();

    /// <summary>
    /// Explanation of the evaluation.
    /// </summary>
    public string? Explanation { get; set; }

    /// <summary>
    /// Entity-specific issues.
    /// </summary>
    public List<EntityIntroductionViolation> EntityViolations { get; set; } = new();
}

/// <summary>
/// Result of comparing two paths.
/// </summary>
public class PathComparisonResult
{
    /// <summary>
    /// Whether the paths are compatible.
    /// </summary>
    public bool AreCompatible { get; set; }

    /// <summary>
    /// Compatibility score (0.0 to 1.0).
    /// </summary>
    public double CompatibilityScore { get; set; }

    /// <summary>
    /// Differences between the paths.
    /// </summary>
    public List<string> Differences { get; set; } = new();

    /// <summary>
    /// Conflicts between the paths.
    /// </summary>
    public List<string> Conflicts { get; set; } = new();
}
