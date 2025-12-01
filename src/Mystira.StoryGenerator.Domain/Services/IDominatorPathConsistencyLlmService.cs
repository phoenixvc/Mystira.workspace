namespace Mystira.StoryGenerator.Domain.Services;

public interface IDominatorPathConsistencyLlmService
{
    Task<ConsistencyEvaluationResult?> EvaluateConsistencyAsync(string scenarioPathContent, CancellationToken cancellationToken = default);
}
