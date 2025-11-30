namespace Mystira.StoryGenerator.Domain.Services;

public interface ILlmConsistencyEvaluator
{
    Task<ConsistencyEvaluationResult?> EvaluateConsistencyAsync(string scenarioContent, CancellationToken cancellationToken = default);
}
