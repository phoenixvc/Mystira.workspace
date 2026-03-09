using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Polly;

namespace Mystira.Shared.Extensions;

/// <summary>
/// Extension methods for registering Polly v8 resilience pipelines.
/// This is the modern replacement for ResilienceExtensions using the new pipeline API.
/// </summary>
/// <remarks>
/// Migration from legacy PolicyFactory:
/// - AddResilientHttpClient → AddResilientHttpClientV8
/// - AddPolicyHandler → AddResilienceHandler
/// - IAsyncPolicy → ResiliencePipeline
/// </remarks>
public static class ResiliencePipelineExtensions
{
    /// <summary>
    /// Adds a typed HTTP client with Polly v8 standard resilience pipeline.
    /// Each client gets its own circuit breaker instance to prevent cascade failures.
    /// </summary>
    /// <typeparam name="TClient">The typed client interface.</typeparam>
    /// <typeparam name="TImplementation">The typed client implementation.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="clientName">Name of the client for logging.</param>
    /// <param name="configureClient">Optional action to configure the HttpClient.</param>
    /// <param name="options">Optional resilience options. Uses defaults if null.</param>
    /// <returns>The IHttpClientBuilder for further configuration.</returns>
    public static IHttpClientBuilder AddResilientHttpClientV8<TClient, TImplementation>(
        this IServiceCollection services,
        string clientName,
        Action<HttpClient>? configureClient = null,
        Resilience.ResilienceOptions? options = null)
        where TClient : class
        where TImplementation : class, TClient
    {
        options ??= new Resilience.ResilienceOptions();

        var builder = services.AddHttpClient<TClient, TImplementation>(client =>
        {
            configureClient?.Invoke(client);
        });

        // Use Microsoft.Extensions.Http.Resilience for standard resilience
        builder.AddStandardResilienceHandler(configure =>
        {
            // Configure retry
            configure.Retry.MaxRetryAttempts = options.MaxRetries;
            configure.Retry.Delay = TimeSpan.FromSeconds(options.BaseDelaySeconds);
            configure.Retry.UseJitter = true;
            configure.Retry.BackoffType = DelayBackoffType.Exponential;

            // Configure circuit breaker
            configure.CircuitBreaker.MinimumThroughput = options.CircuitBreakerThreshold;
            configure.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(options.CircuitBreakerDurationSeconds);

            // Configure timeout
            configure.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            configure.AttemptTimeout.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        return builder;
    }

    /// <summary>
    /// Adds a typed HTTP client with Polly v8 pipeline for long-running operations.
    /// Suitable for LLM API calls and other operations that may take minutes.
    /// </summary>
    /// <typeparam name="TClient">The typed client interface.</typeparam>
    /// <typeparam name="TImplementation">The typed client implementation.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="clientName">Name of the client for logging.</param>
    /// <param name="configureClient">Optional action to configure the HttpClient.</param>
    /// <param name="options">Optional resilience options. Uses defaults if null.</param>
    /// <returns>The IHttpClientBuilder for further configuration.</returns>
    public static IHttpClientBuilder AddLongRunningHttpClientV8<TClient, TImplementation>(
        this IServiceCollection services,
        string clientName,
        Action<HttpClient>? configureClient = null,
        Resilience.ResilienceOptions? options = null)
        where TClient : class
        where TImplementation : class, TClient
    {
        options ??= new Resilience.ResilienceOptions();

        var builder = services.AddHttpClient<TClient, TImplementation>(client =>
        {
            // Set longer timeout on HttpClient itself
            client.Timeout = TimeSpan.FromSeconds(options.LongRunningTimeoutSeconds + 60);
            configureClient?.Invoke(client);
        });

        builder.AddStandardResilienceHandler(configure =>
        {
            // Configure retry with longer delays for LLM calls
            configure.Retry.MaxRetryAttempts = options.MaxRetries;
            configure.Retry.Delay = TimeSpan.FromSeconds(options.BaseDelaySeconds * 2);
            configure.Retry.UseJitter = true;
            configure.Retry.BackoffType = DelayBackoffType.Exponential;

            // Circuit breaker with higher threshold for long operations
            configure.CircuitBreaker.MinimumThroughput = options.CircuitBreakerThreshold;
            configure.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(options.CircuitBreakerDurationSeconds * 2);

            // Extended timeouts for long-running operations
            configure.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(options.LongRunningTimeoutSeconds);
            configure.AttemptTimeout.Timeout = TimeSpan.FromSeconds(options.LongRunningTimeoutSeconds);
        });

        return builder;
    }

    /// <summary>
    /// Adds Polly v8 standard resilience handler to an existing HTTP client builder.
    /// Use this to add resilience to a client that was already configured with AddHttpClient.
    /// </summary>
    /// <param name="builder">The HTTP client builder.</param>
    /// <param name="clientName">Name of the client for logging.</param>
    /// <param name="options">Optional resilience options. Uses defaults if null.</param>
    /// <returns>The IHttpClientBuilder for further configuration.</returns>
    public static IHttpClientBuilder AddMystiraResiliencePipelineV8(
        this IHttpClientBuilder builder,
        string clientName,
        Resilience.ResilienceOptions? options = null)
    {
        options ??= new Resilience.ResilienceOptions();

        builder.AddStandardResilienceHandler(configure =>
        {
            configure.Retry.MaxRetryAttempts = options.MaxRetries;
            configure.Retry.Delay = TimeSpan.FromSeconds(options.BaseDelaySeconds);
            configure.Retry.UseJitter = true;
            configure.Retry.BackoffType = DelayBackoffType.Exponential;

            configure.CircuitBreaker.MinimumThroughput = options.CircuitBreakerThreshold;
            configure.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(options.CircuitBreakerDurationSeconds);

            configure.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            configure.AttemptTimeout.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        return builder;
    }

    /// <summary>
    /// Adds a custom resilience handler using ResiliencePipelineFactory.
    /// For advanced scenarios where you need full control over the pipeline.
    /// </summary>
    /// <param name="builder">The HTTP client builder.</param>
    /// <param name="clientName">Name of the client for logging.</param>
    /// <param name="options">Optional resilience options.</param>
    /// <returns>The IHttpClientBuilder for further configuration.</returns>
    public static IHttpClientBuilder AddCustomResiliencePipeline(
        this IHttpClientBuilder builder,
        string clientName,
        Resilience.ResilienceOptions? options = null)
    {
        options ??= new Resilience.ResilienceOptions();

        builder.AddResilienceHandler(clientName, (pipelineBuilder, context) =>
        {
            var logger = context.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger(clientName);

            pipelineBuilder
                .AddTimeout(TimeSpan.FromSeconds(options.TimeoutSeconds))
                .AddRetry(new Polly.Retry.RetryStrategyOptions<HttpResponseMessage>
                {
                    MaxRetryAttempts = options.MaxRetries,
                    BackoffType = DelayBackoffType.Exponential,
                    Delay = TimeSpan.FromSeconds(options.BaseDelaySeconds),
                    UseJitter = true,
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .Handle<Polly.Timeout.TimeoutRejectedException>()
                        .HandleResult(r => (int)r.StatusCode >= 500 || r.StatusCode == System.Net.HttpStatusCode.RequestTimeout),
                    OnRetry = args =>
                    {
                        if (options.EnableDetailedLogging && logger != null)
                        {
                            logger.LogWarning(
                                "[{ClientName}:Retry] Attempt {AttemptNumber} - {Error}",
                                clientName,
                                args.AttemptNumber,
                                args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString());
                        }
                        return default;
                    }
                })
                .AddCircuitBreaker(new Polly.CircuitBreaker.CircuitBreakerStrategyOptions<HttpResponseMessage>
                {
                    MinimumThroughput = options.CircuitBreakerThreshold,
                    BreakDuration = TimeSpan.FromSeconds(options.CircuitBreakerDurationSeconds),
                    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                        .Handle<HttpRequestException>()
                        .Handle<Polly.Timeout.TimeoutRejectedException>()
                        .HandleResult(r => (int)r.StatusCode >= 500)
                });
        });

        return builder;
    }
}
