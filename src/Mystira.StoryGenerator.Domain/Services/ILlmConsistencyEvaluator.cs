namespace Mystira.StoryGenerator.Domain.Services;

public interface ILlmConsistencyEvaluator
{
    Task<ConsistencyEvaluationResult?> EvaluateConsistencyAsync(string scenarioPathContent, CancellationToken cancellationToken = default);
}
