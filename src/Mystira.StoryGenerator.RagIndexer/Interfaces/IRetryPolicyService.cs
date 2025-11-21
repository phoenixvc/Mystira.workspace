namespace Mystira.StoryGenerator.RagIndexer.Interfaces;

public interface IRetryPolicyService
{
    Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, string operationName, int maxRetries = 3, int delayMs = 1000);
}