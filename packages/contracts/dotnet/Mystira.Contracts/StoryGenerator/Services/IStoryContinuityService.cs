using Mystira.Contracts.StoryGenerator.Stories;
using Mystira.Contracts.StoryGenerator.StoryConsistency;

namespace Mystira.Contracts.StoryGenerator.Services;

/// <summary>
/// Service for analyzing and ensuring story continuity.
/// </summary>
public interface IStoryContinuityService
{
    /// <summary>
    /// Analyzes continuity issues in a scenario.
    /// </summary>
    /// <param name="scenario">The scenario to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The continuity analysis result.</returns>
    Task<ContinuityAnalysisResult> AnalyzeAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes continuity for a specific narrative path.
    /// </summary>
    /// <param name="path">The path to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of continuity issues found.</returns>
    Task<IReadOnlyList<ContinuityIssue>> AnalyzePathAsync(
        ScenarioPath path,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Suggests fixes for continuity issues.
    /// </summary>
    /// <param name="issues">The issues to fix.</param>
    /// <param name="scenario">The scenario context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Map of issue ID to suggested fix.</returns>
    Task<Dictionary<string, string>> SuggestFixesAsync(
        IEnumerable<ContinuityIssue> issues,
        Scenario scenario,
        CancellationToken cancellationToken = default);
}
