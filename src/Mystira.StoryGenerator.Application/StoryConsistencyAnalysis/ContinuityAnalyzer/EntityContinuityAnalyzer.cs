using Mystira.StoryGenerator.Contracts.StoryConsistency;

namespace Mystira.StoryGenerator.Application.StoryConsistencyAnalysis.ContinuityAnalyzer;

public static class EntityContinuityAnalyzer
{
    /// <summary>
    /// Combine prefix-summaries and local SRL classifications to flag
    /// potential entity continuity issues.
    ///
    /// - If an entity is marked "already_known" in SRL but NOT in the
    ///   prefix "must-active" set at that scene, we flag:
    ///   UsedButNotGuaranteedIntroduced.
    ///
    /// - If an entity is marked "new" but IS in the prefix must-active
    ///   set, we flag: ReintroducedButAlreadyGuaranteed.
    ///
    /// - If an entity is marked "removed" but is NOT in the prefix
    ///   must-active set, we flag: RemovedButNotGuaranteedPresent.
    ///
    /// You can tune which of these you care about / treat as warnings
    /// vs. info vs. errors.
    /// </summary>
    public static IReadOnlyList<EntityContinuityIssue> FindIssues(
        IReadOnlyDictionary<string, HashSet<string>> mustActiveByScene,
        IReadOnlyDictionary<string, SemanticRoleLabellingClassification> srlByScene,
        Func<SrlEntityClassification, bool>? filter)
    {
        var issues = new List<EntityContinuityIssue>();

        foreach (var (sceneId, srlResult) in srlByScene)
        {
            mustActiveByScene.TryGetValue(sceneId, out var mustActive);
            mustActive ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var entity in srlResult.EntityClassifications)
            {
                // skip entities not actually present
                if (!entity.PresentInScene)
                    continue;

                // fitler entities if needed
                if (filter != null && !filter(entity))
                    continue;

                var name = entity.Name?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var isGuaranteedActive = mustActive.Contains(name);

                // 1. Used but not guaranteed introduced
                if (entity.IntroductionStatus.Equals("already_known", StringComparison.OrdinalIgnoreCase)
                    && !isGuaranteedActive)
                {
                    issues.Add(new EntityContinuityIssue
                    {
                        SceneId = sceneId,
                        EntityName = name,
                        EntityType = entity.Type,
                        IssueType = EntityContinuityIssueType.UsedButNotGuaranteedIntroduced,
                        Detail = $"Entity '{name}' is treated as already-known in this scene, " +
                                 "but is not guaranteed to be active on all prefixes leading here.",
                        EvidenceSpan = entity.EvidenceSpan
                    });
                }

                // 2. Reintroduced but already guaranteed present
                if (entity.IntroductionStatus.Equals("new", StringComparison.OrdinalIgnoreCase)
                    && isGuaranteedActive)
                {
                    issues.Add(new EntityContinuityIssue
                    {
                        SceneId = sceneId,
                        EntityName = name,
                        EntityType = entity.Type,
                        IssueType = EntityContinuityIssueType.ReintroducedButAlreadyGuaranteed,
                        Detail = $"Entity '{name}' is marked as newly introduced here, " +
                                 "but prefix summaries say it must already be active on all paths.",
                        EvidenceSpan = entity.EvidenceSpan
                    });
                }

                // 3. New but used as known
                var isNew = entity.IntroductionStatus.Equals("new", StringComparison.OrdinalIgnoreCase);
                var usageStyle = entity.LocalUsageStyle; // "clear_introduction" | "already_known_style" | "ambiguous"
                if (isNew
                    && !isGuaranteedActive
                    && string.Equals(usageStyle, "already_known_style", StringComparison.OrdinalIgnoreCase))
                {
                    issues.Add(new EntityContinuityIssue
                    {
                        SceneId = sceneId,
                        EntityName = name,
                        EntityType = entity.Type,
                        IssueType = EntityContinuityIssueType.NewButUsedAsKnown,
                        Detail = $"Entity '{name}' is marked as newly introduced in this scene, but the local wording " +
                                 "treats it as if the audience already knows them. This suggests the entity should have " +
                                 "been introduced earlier on this branch.",
                        EvidenceSpan = entity.EvidenceSpan
                    });
                }

                // 4. New but local usage is ambiguous
                if (isNew
                    && !isGuaranteedActive
                    && string.Equals(usageStyle, "ambiguous", StringComparison.OrdinalIgnoreCase))
                {
                    issues.Add(new EntityContinuityIssue
                    {
                        SceneId = sceneId,
                        EntityName = name,
                        EntityType = entity.Type,
                        IssueType = EntityContinuityIssueType.NewButAmbiguousUsage,
                        Detail = $"Entity '{name}' is marked as newly introduced in this scene, but the local wording " +
                                 "is ambiguous about whether the audience already knows them. This might suggest the " +
                                 "entity should have been introduced earlier on this branch.",
                        EvidenceSpan = entity.EvidenceSpan
                    });
                }

                // 5. Removed but not guaranteed present
                if (entity.RemovalStatus.Equals("removed", StringComparison.OrdinalIgnoreCase)
                    && !isGuaranteedActive)
                {
                    issues.Add(new EntityContinuityIssue
                    {
                        SceneId = sceneId,
                        EntityName = name,
                        EntityType = entity.Type,
                        IssueType = EntityContinuityIssueType.RemovedButNotGuaranteedPresent,
                        Detail = $"Entity '{name}' is removed here, but prefix summaries do not " +
                                 "guarantee that it was present on all paths.",
                        EvidenceSpan = entity.EvidenceSpan
                    });
                }
            }
        }

        return issues;
    }
}
