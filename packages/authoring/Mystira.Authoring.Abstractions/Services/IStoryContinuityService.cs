using Mystira.Authoring.Abstractions.Models.Consistency;
using Mystira.Authoring.Abstractions.Models.Scenario;

namespace Mystira.Authoring.Abstractions.Services;

/// <summary>
/// Service for analyzing story continuity issues in scenarios.
/// </summary>
public interface IStoryContinuityService
{
    /// <summary>
    /// Analyzes a scenario for entity continuity issues.
    /// </summary>
    /// <param name="scenario">The scenario to analyze.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A list of continuity issues found.</returns>
    Task<IReadOnlyList<EntityContinuityIssue>> AnalyzeAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default);
}
