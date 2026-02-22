using Mystira.StoryGenerator.RagIndexer.Interfaces;

namespace Mystira.StoryGenerator.RagIndexer.Services;

public class RetryPolicyService : IRetryPolicyService
{
    private readonly ILoggerService _logger;

    public RetryPolicyService(ILoggerService logger)
    {
        _logger = logger;
    }

    public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, string operationName, int maxRetries = 3, int delayMs = 1000)
    {
        int attempt = 0;
        
        while (true)
        {
            attempt++;
            try
            {
                return await operation();
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                _logger.LogWarning($"Attempt {attempt} failed for {operationName}. Retrying in {delayMs}ms... Error: {ex.Message}");
                await Task.Delay(delayMs * attempt); // Exponential backoff
            }
        }
    }
}