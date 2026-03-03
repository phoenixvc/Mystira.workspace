using Mystira.StoryGenerator.Contracts.StoryConsistency;

namespace Mystira.StoryGenerator.Application.StoryConsistencyAnalysis.ContinuityAnalyzer;

public static class EntityContinuityIssueFiltering
{
    /// <summary>
    /// Filters issues by confidence, entity types, proper-nouns-only, and optional role whitelist.
    /// - confidences/types compare using OrdinalIgnoreCase.
    /// - If properNounsOnly is true, keeps only issues where IsProperNoun == true.
    /// - If roleWhitelist is provided and non-empty, keeps issues where any SemanticRoles contains one of the roles (OrdinalIgnoreCase).
    /// </summary>
    public static IReadOnlyList<EntityContinuityIssue> Filter(
        IEnumerable<EntityContinuityIssue> issues,
        IEnumerable<string>? confidences = null,
        IEnumerable<string>? entityTypes = null,
        bool properNounsOnly = false,
        IEnumerable<string>? roleWhitelist = null)
    {
        if (issues == null) return Array.Empty<EntityContinuityIssue>();

        var confSet = confidences != null
            ? new HashSet<string>(confidences.Where(c => !string.IsNullOrWhiteSpace(c)), StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var typeSet = entityTypes != null
            ? new HashSet<string>(entityTypes.Where(t => !string.IsNullOrWhiteSpace(t)), StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var roleSet = roleWhitelist != null
            ? new HashSet<string>(roleWhitelist.Where(r => !string.IsNullOrWhiteSpace(r)), StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        bool hasConf = confSet.Count > 0;
        bool hasTypes = typeSet.Count > 0;
        bool hasRoles = roleSet.Count > 0;

        return issues.Where(i =>
            (!hasConf || confSet.Contains(i.Confidence)) &&
            (!hasTypes || typeSet.Contains(i.EntityType)) &&
            (!properNounsOnly || i.IsProperNoun) &&
            (!hasRoles || (i.SemanticRoles?.Any(r => roleSet.Contains(r)) ?? false))
        ).ToList();
    }
}
