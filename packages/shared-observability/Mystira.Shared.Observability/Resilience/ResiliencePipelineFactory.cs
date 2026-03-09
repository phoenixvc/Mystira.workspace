using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using System.Net;

namespace Mystira.Shared.Resilience;

/// <summary>
/// Factory for creating Polly v8 resilience pipelines.
/// This is the modern replacement for PolicyFactory, using ResiliencePipelineBuilder
/// instead of the legacy Policy.WrapAsync API.
/// </summary>
/// <remarks>
/// Key differences from legacy PolicyFactory:
/// - Uses ResiliencePipeline instead of IAsyncPolicy
/// - Strategies are composable and more testable
/// - Better integration with .NET dependency injection
/// - Built-in telemetry support
/// </remarks>
public static class ResiliencePipelineFactory
{
    /// <summary>
    /// Creates a standard HTTP resilience pipeline with retry, circuit breaker, and timeout.
    /// Uses Polly v8 ResiliencePipelineBuilder for better composability.
    /// </summary>
    /// <param name="clientName">Name of the HTTP client for logging purposes.</param>
    /// <param name="options">Optional resilience configuration. Uses defaults if null.</param>
    /// <param name="logger">Optional logger for pipeline events.</param>
    /// <returns>Resilience pipeline for HTTP response messages.</returns>
    public static ResiliencePipeline<HttpResponseMessage> CreateStandardHttpPipeline(
        string clientName,
        ResilienceOptions? options = null,
        ILogger? logger = null)
    {
        options ??= new ResilienceOptions();

        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddTimeout(CreateTimeoutOptions(options.TimeoutSeconds))
            .AddRetry(CreateHttpRetryOptions(clientName, options, logger))
            .AddCircuitBreaker(CreateHttpCircuitBreakerOptions(clientName, options, logger))
            .Build();
    }

    /// <summary>
    /// Creates a resilience pipeline for long-running operations (e.g., LLM API calls).
    /// Uses extended timeout and configurable retry settings.
    /// </summary>
    /// <param name="clientName">Name of the HTTP client for logging purposes.</param>
    /// <param name="options">Optional resilience configuration. Uses defaults if null.</param>
    /// <param name="logger">Optional logger for pipeline events.</param>
    /// <returns>Resilience pipeline for long-running HTTP operations.</returns>
    public static ResiliencePipeline<HttpResponseMessage> CreateLongRunningHttpPipeline(
        string clientName,
        ResilienceOptions? options = null,
        ILogger? logger = null)
    {
        options ??= new ResilienceOptions();

        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddTimeout(CreateTimeoutOptions(options.LongRunningTimeoutSeconds))
            .AddRetry(CreateHttpRetryOptions(clientName, options, logger))
            .AddCircuitBreaker(CreateHttpCircuitBreakerOptions(clientName, options, logger))
            .Build();
    }

    /// <summary>
    /// Creates a generic resilience pipeline for non-HTTP operations.
    /// Replaces custom retry implementations like StoryGenerator's RetryPolicyService.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="operationName">Name of the operation for logging.</param>
    /// <param name="options">Optional resilience configuration.</param>
    /// <param name="logger">Optional logger for retry events.</param>
    /// <returns>Generic resilience pipeline.</returns>
    public static ResiliencePipeline<T> CreateRetryPipeline<T>(
        string operationName,
        ResilienceOptions? options = null,
        ILogger? logger = null)
    {
        options ??= new ResilienceOptions();

        return new ResiliencePipelineBuilder<T>()
            .AddRetry(new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = options.MaxRetries,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(options.BaseDelaySeconds),
                UseJitter = true,
                ShouldHandle = new PredicateBuilder<T>().Handle<Exception>(),
                OnRetry = args =>
                {
                    if (options.EnableDetailedLogging && logger != null)
                    {
                        logger.LogWarning(
                            "[{OperationName}:Retry] Attempt {AttemptNumber} after {Delay}s - {Error}",
                            operationName,
                            args.AttemptNumber,
                            args.RetryDelay.TotalSeconds,
                            args.Outcome.Exception?.Message ?? "Unknown error");
                    }
                    return default;
                }
            })
            .Build();
    }

    /// <summary>
    /// Creates a void resilience pipeline for operations without return values.
    /// </summary>
    /// <param name="operationName">Name of the operation for logging.</param>
    /// <param name="options">Optional resilience configuration.</param>
    /// <param name="logger">Optional logger for retry events.</param>
    /// <returns>Void resilience pipeline.</returns>
    public static ResiliencePipeline CreateVoidRetryPipeline(
        string operationName,
        ResilienceOptions? options = null,
        ILogger? logger = null)
    {
        options ??= new ResilienceOptions();

        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = options.MaxRetries,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(options.BaseDelaySeconds),
                UseJitter = true,
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                OnRetry = args =>
                {
                    if (options.EnableDetailedLogging && logger != null)
                    {
                        logger.LogWarning(
                            "[{OperationName}:Retry] Attempt {AttemptNumber} after {Delay}s - {Error}",
                            operationName,
                            args.AttemptNumber,
                            args.RetryDelay.TotalSeconds,
                            args.Outcome.Exception?.Message ?? "Unknown error");
                    }
                    return default;
                }
            })
            .Build();
    }

    /// <summary>
    /// Creates a resilience pipeline with circuit breaker only (no retry).
    /// Useful when retry logic is handled elsewhere.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="operationName">Name of the operation for logging.</param>
    /// <param name="options">Optional resilience configuration.</param>
    /// <param name="logger">Optional logger for circuit breaker events.</param>
    /// <returns>Circuit breaker resilience pipeline.</returns>
    public static ResiliencePipeline<T> CreateCircuitBreakerPipeline<T>(
        string operationName,
        ResilienceOptions? options = null,
        ILogger? logger = null)
    {
        options ??= new ResilienceOptions();

        return new ResiliencePipelineBuilder<T>()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<T>
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(options.CircuitBreakerDurationSeconds),
                MinimumThroughput = options.CircuitBreakerThreshold,
                BreakDuration = TimeSpan.FromSeconds(options.CircuitBreakerDurationSeconds),
                ShouldHandle = new PredicateBuilder<T>().Handle<Exception>(),
                OnOpened = args =>
                {
                    if (options.EnableDetailedLogging && logger != null)
                    {
                        logger.LogWarning(
                            "[{OperationName}:CircuitBreaker] Opened for {Duration}s - {Error}",
                            operationName,
                            args.BreakDuration.TotalSeconds,
                            args.Outcome.Exception?.Message ?? "Unknown error");
                    }
                    return default;
                },
                OnClosed = args =>
                {
                    if (options.EnableDetailedLogging && logger != null)
                    {
                        logger.LogInformation("[{OperationName}:CircuitBreaker] Reset", operationName);
                    }
                    return default;
                },
                OnHalfOpened = args =>
                {
                    if (options.EnableDetailedLogging && logger != null)
                    {
                        logger.LogInformation("[{OperationName}:CircuitBreaker] Half-open, testing...", operationName);
                    }
                    return default;
                }
            })
            .Build();
    }

    private static TimeoutStrategyOptions CreateTimeoutOptions(int timeoutSeconds)
    {
        return new TimeoutStrategyOptions
        {
            Timeout = TimeSpan.FromSeconds(timeoutSeconds)
        };
    }

    private static RetryStrategyOptions<HttpResponseMessage> CreateHttpRetryOptions(
        string clientName,
        ResilienceOptions options,
        ILogger? logger)
    {
        return new RetryStrategyOptions<HttpResponseMessage>
        {
            MaxRetryAttempts = options.MaxRetries,
            BackoffType = DelayBackoffType.Exponential,
            Delay = TimeSpan.FromSeconds(options.BaseDelaySeconds),
            UseJitter = true,
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .Handle<TimeoutRejectedException>()
                .HandleResult(response => IsTransientError(response.StatusCode)),
            OnRetry = args =>
            {
                if (options.EnableDetailedLogging && logger != null)
                {
                    var error = args.Outcome.Exception?.Message
                        ?? args.Outcome.Result?.StatusCode.ToString()
                        ?? "Unknown";
                    logger.LogWarning(
                        "[{ClientName}:Retry] Attempt {AttemptNumber} after {Delay}s - {Error}",
                        clientName,
                        args.AttemptNumber,
                        args.RetryDelay.TotalSeconds,
                        error);
                }
                return default;
            }
        };
    }

    private static CircuitBreakerStrategyOptions<HttpResponseMessage> CreateHttpCircuitBreakerOptions(
        string clientName,
        ResilienceOptions options,
        ILogger? logger)
    {
        return new CircuitBreakerStrategyOptions<HttpResponseMessage>
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(30),
            MinimumThroughput = options.CircuitBreakerThreshold,
            BreakDuration = TimeSpan.FromSeconds(options.CircuitBreakerDurationSeconds),
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .Handle<TimeoutRejectedException>()
                .HandleResult(response => IsTransientError(response.StatusCode)),
            OnOpened = args =>
            {
                if (options.EnableDetailedLogging && logger != null)
                {
                    var error = args.Outcome.Exception?.Message
                        ?? args.Outcome.Result?.StatusCode.ToString()
                        ?? "Unknown";
                    logger.LogWarning(
                        "[{ClientName}:CircuitBreaker] Opened for {Duration}s - {Error}",
                        clientName,
                        args.BreakDuration.TotalSeconds,
                        error);
                }
                return default;
            },
            OnClosed = args =>
            {
                if (options.EnableDetailedLogging && logger != null)
                {
                    logger.LogInformation("[{ClientName}:CircuitBreaker] Reset", clientName);
                }
                return default;
            },
            OnHalfOpened = args =>
            {
                if (options.EnableDetailedLogging && logger != null)
                {
                    logger.LogInformation("[{ClientName}:CircuitBreaker] Half-open, testing...", clientName);
                }
                return default;
            }
        };
    }

    private static bool IsTransientError(HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.RequestTimeout
            || statusCode == HttpStatusCode.TooManyRequests
            || (int)statusCode >= 500;
    }
}
