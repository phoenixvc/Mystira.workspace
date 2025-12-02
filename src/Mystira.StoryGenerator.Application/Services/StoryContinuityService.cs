using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Application.StoryConsistencyAnalysis;
using Mystira.StoryGenerator.Contracts.Entities;
using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Application.Services;

/// <summary>
/// Service that evaluates story continuity and returns formatted continuity issues.
/// </summary>
public interface IStoryContinuityService
{
    /// <summary>
    /// Evaluates continuity issues in a scenario and returns formatted results.
    /// </summary>
    Task<EvaluateStoryContinuityResponse> EvaluateAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of the story continuity service.
/// </summary>
public class StoryContinuityService : IStoryContinuityService
{
    private readonly IScenarioConsistencyEvaluationService _consistencyEvaluationService;
    private readonly ILogger<StoryContinuityService> _logger;

    public StoryContinuityService(
        IScenarioConsistencyEvaluationService consistencyEvaluationService,
        ILogger<StoryContinuityService> logger)
    {
        _consistencyEvaluationService = consistencyEvaluationService ?? throw new ArgumentNullException(nameof(consistencyEvaluationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<EvaluateStoryContinuityResponse> EvaluateAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        try
        {
            _logger.LogInformation("Starting story continuity evaluation for scenario {ScenarioId}", scenario.Id);

            var result = await _consistencyEvaluationService.EvaluateAsync(scenario, cancellationToken);

            var continuityIssues = ConvertToStoryContinuityIssues(result);

            _logger.LogInformation(
                "Story continuity evaluation completed for {ScenarioId} with {IssueCount} issues",
                scenario.Id,
                continuityIssues.Count);

            return new EvaluateStoryContinuityResponse
            {
                Success = result.IsSuccessful,
                Issues = continuityIssues,
                Error = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during story continuity evaluation for scenario {ScenarioId}", scenario.Id);
            return new EvaluateStoryContinuityResponse
            {
                Success = false,
                Issues = [],
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// Converts consistency evaluation results to continuity issues.
    /// </summary>
    private List<StoryContinuityIssue> ConvertToStoryContinuityIssues(
        ScenarioConsistencyEvaluationResult result)
    {
        var issues = new List<StoryContinuityIssue>();

        // Convert entity introduction violations
        if (result.EntityIntroductionResult?.Violations != null)
        {
            foreach (var violation in result.EntityIntroductionResult.Violations)
            {
                var issue = ConvertViolationToIssue(violation, result.EntityIntroductionResult.SceneClassifications);
                issues.Add(issue);
            }
        }

        return issues;
    }

    /// <summary>
    /// Converts a single entity introduction violation to a continuity issue.
    /// </summary>
    private StoryContinuityIssue ConvertViolationToIssue(
        ScenarioEntityIntroductionValidator.SceneReferenceViolation violation,
        IReadOnlyDictionary<string, SceneEntityClassificationData> sceneClassifications)
    {
        var entityType = violation.Entity.Type.ToString().ToLowerInvariant();
        var confidenceStr = violation.Entity.Confidence.ToString().ToLowerInvariant();

        // Determine issue type based on context
        var issueType = DetermineIssueType(violation, sceneClassifications);

        // Get semantic roles if available in the classification data
        var semanticRoles = GetSemanticRoles(violation.SceneId, violation.Entity, sceneClassifications);

        var detail = GenerateIssueDetail(violation, issueType);
        var evidenceSpan = GenerateEvidenceSpan(violation);

        return new StoryContinuityIssue
        {
            SceneId = violation.SceneId,
            EntityName = violation.Entity.Name,
            EntityType = entityType,
            IssueType = issueType,
            Detail = detail,
            EvidenceSpan = evidenceSpan,
            IsPronoun = IsPronoun(violation.Entity.Name),
            Confidence = confidenceStr,
            SemanticRoles = semanticRoles
        };
    }

    /// <summary>
    /// Determines the issue type based on the violation context.
    /// </summary>
    private string DetermineIssueType(
        ScenarioEntityIntroductionValidator.SceneReferenceViolation violation,
        IReadOnlyDictionary<string, SceneEntityClassificationData> sceneClassifications)
    {
        // Check if entity was introduced as new in this scene
        if (sceneClassifications.TryGetValue(violation.SceneId, out var classification))
        {
            var isNewlyIntroduced = classification.IntroducedEntities
                .Any(e => e.Type == violation.Entity.Type && e.Name == violation.Entity.Name);

            if (isNewlyIntroduced)
            {
                return "reintroducedButAlreadyGuaranteed";
            }
        }

        // Default to "used but not guaranteed introduced" 
        return "usedButNotGuaranteedIntroduced";
    }

    /// <summary>
    /// Gets semantic roles for an entity (placeholder for now).
    /// In a full implementation, this would come from semantic role labeling.
    /// </summary>
    private string[] GetSemanticRoles(
        string sceneId,
        SceneEntity entity,
        IReadOnlyDictionary<string, SceneEntityClassificationData> sceneClassifications)
    {
        // Placeholder: return empty array
        // In a full implementation, this would call the semantic role labeling service
        return [];
    }

    /// <summary>
    /// Generates a detailed description of the continuity issue.
    /// </summary>
    private string GenerateIssueDetail(
        ScenarioEntityIntroductionValidator.SceneReferenceViolation violation,
        string issueType)
    {
        var entityType = violation.Entity.Type.ToString().ToLowerInvariant();
        
        if (issueType == "reintroducedButAlreadyGuaranteed")
        {
            return $"Entity '{violation.Entity.Name}' is marked as newly introduced here, but prefix summaries say it must already be active on all paths.";
        }

        return $"Entity '{violation.Entity.Name}' is treated as already-known in this scene, but is not guaranteed to be active on all prefixes leading here.";
    }

    /// <summary>
    /// Generates an evidence span from the violation (placeholder).
    /// </summary>
    private string GenerateEvidenceSpan(
        ScenarioEntityIntroductionValidator.SceneReferenceViolation violation)
    {
        // Placeholder: return entity name
        // In a full implementation, this would extract the actual text span from the scene
        return violation.Entity.Name;
    }

    /// <summary>
    /// Determines if a name appears to be a pronoun or contains one.
    /// </summary>
    private bool IsPronoun(string name)
    {
        var pronouns = new[] { "he", "she", "it", "they", "him", "her", "them", "his", "hers", "their", "theirs", "itself" };
        var nameLower = name.ToLowerInvariant();
        return pronouns.Any(p => nameLower == p || nameLower.Contains($" {p} "));
    }
}
