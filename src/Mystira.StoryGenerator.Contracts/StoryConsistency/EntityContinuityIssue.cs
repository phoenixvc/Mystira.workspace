namespace Mystira.StoryGenerator.Contracts.StoryConsistency;

public sealed class EntityContinuityIssue
{
    public string SceneId { get; init; } = string.Empty;
    public string EntityName { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public EntityContinuityIssueType IssueType { get; init; }
    public string Detail { get; init; } = string.Empty;
    public string? EvidenceSpan { get; init; }
}

public enum EntityContinuityIssueType
{
    UsedButNotGuaranteedIntroduced,
    ReintroducedButAlreadyGuaranteed,
    RemovedButNotGuaranteedPresent,
    NewButUsedAsKnown,
    NewButAmbiguousUsage
}
