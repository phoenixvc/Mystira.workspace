using Mystira.Contracts.StoryGenerator.Stories;
using Mystira.Contracts.StoryGenerator.StoryConsistency;

namespace Mystira.Contracts.StoryGenerator.Services;

/// <summary>
/// Service for Semantic Role Labelling (SRL) analysis of scenarios.
/// SRL identifies the semantic relationships between entities and actions in text.
/// </summary>
public interface IScenarioSrlAnalysisService
{
    /// <summary>
    /// Performs SRL analysis on a scenario.
    /// </summary>
    /// <param name="scenario">The scenario to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The SRL analysis result.</returns>
    Task<SrlAnalysisResult> AnalyzeAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs SRL analysis on a single scene.
    /// </summary>
    /// <param name="scene">The scene to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The SRL analysis result for the scene.</returns>
    Task<SceneSrlAnalysis> AnalyzeSceneAsync(
        Scene scene,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes semantic role consistency across a path.
    /// </summary>
    /// <param name="path">The path to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of semantic role inconsistencies.</returns>
    Task<IReadOnlyList<SemanticRoleInconsistency>> AnalyzePathConsistencyAsync(
        ScenarioPath path,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of SRL analysis for a scenario.
/// </summary>
public class SrlAnalysisResult
{
    /// <summary>
    /// SRL analysis for each scene.
    /// </summary>
    public Dictionary<string, SceneSrlAnalysis> SceneAnalyses { get; set; } = new();

    /// <summary>
    /// Overall consistency score.
    /// </summary>
    public double ConsistencyScore { get; set; }

    /// <summary>
    /// Identified inconsistencies.
    /// </summary>
    public List<SemanticRoleInconsistency> Inconsistencies { get; set; } = new();
}

/// <summary>
/// SRL analysis for a single scene.
/// </summary>
public class SceneSrlAnalysis
{
    /// <summary>
    /// Scene ID.
    /// </summary>
    public string SceneId { get; set; } = string.Empty;

    /// <summary>
    /// Identified predicates (actions/events).
    /// </summary>
    public List<PredicateFrame> Predicates { get; set; } = new();

    /// <summary>
    /// Identified agents (who performs actions).
    /// </summary>
    public List<string> Agents { get; set; } = new();

    /// <summary>
    /// Identified patients (who/what is affected).
    /// </summary>
    public List<string> Patients { get; set; } = new();

    /// <summary>
    /// Identified instruments (tools/means used).
    /// </summary>
    public List<string> Instruments { get; set; } = new();

    /// <summary>
    /// Identified locations.
    /// </summary>
    public List<string> Locations { get; set; } = new();
}

/// <summary>
/// Represents a predicate frame in SRL analysis.
/// </summary>
public class PredicateFrame
{
    /// <summary>
    /// The predicate (verb/action).
    /// </summary>
    public string Predicate { get; set; } = string.Empty;

    /// <summary>
    /// Arguments with their semantic roles.
    /// </summary>
    public Dictionary<string, string> Arguments { get; set; } = new();
}

/// <summary>
/// Represents a semantic role inconsistency.
/// </summary>
public class SemanticRoleInconsistency
{
    /// <summary>
    /// Scene IDs where the inconsistency was detected.
    /// </summary>
    public List<string> SceneIds { get; set; } = new();

    /// <summary>
    /// Description of the inconsistency.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Severity of the inconsistency.
    /// </summary>
    public string Severity { get; set; } = string.Empty;

    /// <summary>
    /// The semantic role involved.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Expected value.
    /// </summary>
    public string? Expected { get; set; }

    /// <summary>
    /// Actual value found.
    /// </summary>
    public string? Actual { get; set; }
}
