using Mystira.StoryGenerator.Contracts.Entities;
using Mystira.StoryGenerator.Contracts.StoryConsistency;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Domain.Services;

public interface ISemanticRoleLabellingLlmService
{
    public Task<SemanticRoleLabellingClassification?> ClassifyAsync(
        Scene scene,
        IEnumerable<SceneEntity> candidateEntities,
        IEnumerable<SceneEntity> knownActiveEntities,
        IEnumerable<SceneEntity> knownRemovedEntities,
        CancellationToken cancellationToken = default);
}
