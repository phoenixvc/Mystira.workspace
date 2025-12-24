using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Timeout;

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
                retryAttempt => TimeSpan.FromSeconds(
                    Math.Pow(options.BaseDelaySeconds, retryAttempt)),
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
                retryAttempt => TimeSpan.FromSeconds(
                    Math.Pow(options.BaseDelaySeconds, retryAttempt)),
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

    private static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(
        string clientName,
        ResilienceOptions options,
        ILogger? logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                options.MaxRetries,
                retryAttempt => TimeSpan.FromSeconds(
                    Math.Pow(options.BaseDelaySeconds, retryAttempt)),
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
