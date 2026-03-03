namespace Mystira.Contracts.StoryGenerator.StoryConsistency;

public sealed class EntityContinuityIssue
{
    public string SceneId { get; init; } = string.Empty;
    public string EntityName { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public EntityContinuityIssueType IssueType { get; init; }
    public string Detail { get; init; } = string.Empty;
    public string? EvidenceSpan { get; init; }
    // Post-analysis filtering fields (copied from SRL classification)
    public bool IsProperNoun { get; init; }
    public string Confidence { get; init; } = string.Empty; // "high" | "medium" | "low"
    public List<string> SemanticRoles { get; init; } = new();
}

public enum EntityContinuityIssueType
{
    UsedButNotGuaranteedIntroduced,
    ReintroducedButAlreadyGuaranteed,
    RemovedButNotGuaranteedPresent,
    NewButUsedAsKnown,
    NewButAmbiguousUsage
}
