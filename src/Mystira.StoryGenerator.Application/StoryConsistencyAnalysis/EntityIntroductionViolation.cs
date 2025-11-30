using Mystira.StoryGenerator.Contracts.Entities;

namespace Mystira.StoryGenerator.Application.StoryConsistencyAnalysis;

/// <summary>
/// Helper record for reporting where an entity is used without being
/// guaranteed to have been introduced on all paths.
/// </summary>
public sealed record EntityIntroductionViolation(string SceneId, SceneEntity Entity);
