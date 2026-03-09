using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace Mystira.Shared.Resilience;

/// <summary>
/// Factory for creating standardized Polly resilience policies.
/// Consolidates patterns from App.PWA and StoryGenerator.
/// </summary>
public static class PolicyFactory
{
    /// <summary>
    /// Creates a standard HTTP resilience policy with retry, circuit breaker, and timeout.
    /// Each call creates independent policy instances (important for per-client circuit breakers).
    /// </summary>
    /// <param name="clientName">Name of the HTTP client for logging purposes.</param>
    /// <param name="options">Optional resilience configuration. Uses defaults if null.</param>
    /// <param name="logger">Optional logger for policy events.</param>
    /// <returns>Combined async policy wrapping timeout, retry, and circuit breaker.</returns>
    public static IAsyncPolicy<HttpResponseMessage> CreateStandardHttpPolicy(
        string clientName,
        ResilienceOptions? options = null,
        ILogger? logger = null)
    {
        options ??= new ResilienceOptions();

        var retryPolicy = CreateRetryPolicy(clientName, options, logger);
        var circuitBreakerPolicy = CreateCircuitBreakerPolicy(clientName, options, logger);
        var timeoutPolicy = CreateTimeoutPolicy(options.TimeoutSeconds);

        // Combined policy: timeout wraps retry wraps circuit breaker
        return Policy.WrapAsync(timeoutPolicy, retryPolicy, circuitBreakerPolicy);
    }

    /// <summary>
    /// Creates a resilience policy for long-running operations (e.g., LLM API calls).
    /// Uses extended timeout and more aggressive retry.
    /// </summary>
    /// <param name="clientName">Name of the HTTP client for logging purposes.</param>
    /// <param name="options">Optional resilience configuration. Uses defaults if null.</param>
    /// <param name="logger">Optional logger for policy events.</param>
    /// <returns>Combined async policy for long-running operations.</returns>
    public static IAsyncPolicy<HttpResponseMessage> CreateLongRunningHttpPolicy(
        string clientName,
        ResilienceOptions? options = null,
        ILogger? logger = null)
    {
        options ??= new ResilienceOptions();

        var retryPolicy = CreateRetryPolicy(clientName, options, logger);
        var circuitBreakerPolicy = CreateCircuitBreakerPolicy(clientName, options, logger);
        var timeoutPolicy = CreateTimeoutPolicy(options.LongRunningTimeoutSeconds);

        return Policy.WrapAsync(timeoutPolicy, retryPolicy, circuitBreakerPolicy);
    }

    /// <summary>
    /// Creates a simple retry policy for non-HTTP operations.
    /// Replaces custom retry implementations like StoryGenerator's RetryPolicyService.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="operationName">Name of the operation for logging.</param>
    /// <param name="options">Optional resilience configuration.</param>
    /// <param name="logger">Optional logger for retry events.</param>
    /// <returns>Async retry policy.</returns>
    public static IAsyncPolicy<T> CreateRetryPolicy<T>(
        string operationName,
        ResilienceOptions? options = null,
        ILogger? logger = null)
    {
        options ??= new ResilienceOptions();

        return Policy<T>
            .Handle<Exception>()
            .WaitAndRetryAsync(
                options.MaxRetries,
                retryAttempt => CalculateDelay(retryAttempt, options.BaseDelaySeconds),
                onRetry: (outcome, timespan, retryAttempt, _) =>
                {
                    if (options.EnableDetailedLogging && logger != null)
                    {
                        logger.LogWarning(
                            "[{OperationName}:Retry] Attempt {RetryAttempt} after {Delay}s - {Error}",
                            operationName,
                            retryAttempt,
                            timespan.TotalSeconds,
                            outcome.Exception?.Message ?? "Unknown error");
                    }
                });
    }

    /// <summary>
    /// Creates a simple retry policy for void operations.
    /// </summary>
    /// <param name="operationName">Name of the operation for logging.</param>
    /// <param name="options">Optional resilience configuration.</param>
    /// <param name="logger">Optional logger for retry events.</param>
    /// <returns>Async retry policy.</returns>
    public static IAsyncPolicy CreateVoidRetryPolicy(
        string operationName,
        ResilienceOptions? options = null,
        ILogger? logger = null)
    {
        options ??= new ResilienceOptions();

        return Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                options.MaxRetries,
                retryAttempt => CalculateDelay(retryAttempt, options.BaseDelaySeconds),
                onRetry: (exception, timespan, retryAttempt, _) =>
                {
                    if (options.EnableDetailedLogging && logger != null)
                    {
                        logger.LogWarning(
                            "[{OperationName}:Retry] Attempt {RetryAttempt} after {Delay}s - {Error}",
                            operationName,
                            retryAttempt,
                            timespan.TotalSeconds,
                            exception.Message);
                    }
                });
    }

    /// <summary>
    /// Calculates the delay for a retry attempt using exponential backoff with jitter.
    /// Formula: baseDelay * 2^(attempt-1) + random jitter
    /// </summary>
    private static TimeSpan CalculateDelay(int retryAttempt, int baseDelaySeconds)
    {
        // Exponential backoff: baseDelay * 2^(attempt-1)
        // Attempt 1: baseDelay * 1 = baseDelay
        // Attempt 2: baseDelay * 2
        // Attempt 3: baseDelay * 4
        var exponentialDelay = baseDelaySeconds * Math.Pow(2, retryAttempt - 1);

        // Add jitter (±20%) to prevent thundering herd
        var jitter = exponentialDelay * 0.2 * (Random.Shared.NextDouble() * 2 - 1);

        // Cap at 60 seconds
        var totalDelay = Math.Min(exponentialDelay + jitter, 60);

        return TimeSpan.FromSeconds(totalDelay);
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(
        string clientName,
        ResilienceOptions options,
        ILogger? logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                options.MaxRetries,
                retryAttempt => CalculateDelay(retryAttempt, options.BaseDelaySeconds),
                onRetry: (outcome, timespan, retryAttempt, _) =>
                {
                    if (options.EnableDetailedLogging && logger != null)
                    {
                        logger.LogWarning(
                            "[{ClientName}:Retry] Attempt {RetryAttempt} after {Delay}s - {Error}",
                            clientName,
                            retryAttempt,
                            timespan.TotalSeconds,
                            outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                    }
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateCircuitBreakerPolicy(
        string clientName,
        ResilienceOptions options,
        ILogger? logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                options.CircuitBreakerThreshold,
                TimeSpan.FromSeconds(options.CircuitBreakerDurationSeconds),
                onBreak: (outcome, breakDelay) =>
                {
                    if (options.EnableDetailedLogging && logger != null)
                    {
                        logger.LogWarning(
                            "[{ClientName}:CircuitBreaker] Opened for {Duration}s - {Error}",
                            clientName,
                            breakDelay.TotalSeconds,
                            outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                    }
                },
                onReset: () =>
                {
                    if (options.EnableDetailedLogging && logger != null)
                    {
                        logger.LogInformation("[{ClientName}:CircuitBreaker] Reset", clientName);
                    }
                },
                onHalfOpen: () =>
                {
                    if (options.EnableDetailedLogging && logger != null)
                    {
                        logger.LogInformation("[{ClientName}:CircuitBreaker] Half-open, testing...", clientName);
                    }
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateTimeoutPolicy(int timeoutSeconds)
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(timeoutSeconds));
    }
}
