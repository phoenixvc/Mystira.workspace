using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;

namespace Mystira.Shared.Resilience;

/// <summary>
/// Extension methods for registering resilience policies in DI.
/// </summary>
public static class ResilienceExtensions
{
    /// <summary>
    /// Adds resilience options from configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMystiraResilience(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ResilienceOptions>(
            configuration.GetSection(ResilienceOptions.SectionName));

        return services;
    }

    /// <summary>
    /// Adds a typed HTTP client with standard resilience policies.
    /// Each client gets its own circuit breaker instance to prevent cascade failures.
    /// </summary>
    /// <typeparam name="TClient">The typed client interface.</typeparam>
    /// <typeparam name="TImplementation">The typed client implementation.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="clientName">Name of the client for logging.</param>
    /// <param name="configureClient">Optional action to configure the HttpClient.</param>
    /// <param name="options">Optional resilience options. Uses defaults if null.</param>
    /// <returns>The IHttpClientBuilder for further configuration.</returns>
    public static IHttpClientBuilder AddResilientHttpClient<TClient, TImplementation>(
        this IServiceCollection services,
        string clientName,
        Action<HttpClient>? configureClient = null,
        ResilienceOptions? options = null)
        where TClient : class
        where TImplementation : class, TClient
    {
        var builder = services.AddHttpClient<TClient, TImplementation>(client =>
        {
            configureClient?.Invoke(client);
        });

        // Add resilience policy - each client gets its own circuit breaker
        builder.AddPolicyHandler((provider, _) =>
        {
            var logger = provider.GetService<ILoggerFactory>()?.CreateLogger(clientName);
            return PolicyFactory.CreateStandardHttpPolicy(clientName, options, logger);
        });

        return builder;
    }

    /// <summary>
    /// Adds a typed HTTP client with long-running resilience policies.
    /// Suitable for LLM API calls and other operations that may take minutes.
    /// </summary>
    /// <typeparam name="TClient">The typed client interface.</typeparam>
    /// <typeparam name="TImplementation">The typed client implementation.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="clientName">Name of the client for logging.</param>
    /// <param name="configureClient">Optional action to configure the HttpClient.</param>
    /// <param name="options">Optional resilience options. Uses defaults if null.</param>
    /// <returns>The IHttpClientBuilder for further configuration.</returns>
    public static IHttpClientBuilder AddLongRunningHttpClient<TClient, TImplementation>(
        this IServiceCollection services,
        string clientName,
        Action<HttpClient>? configureClient = null,
        ResilienceOptions? options = null)
        where TClient : class
        where TImplementation : class, TClient
    {
        var builder = services.AddHttpClient<TClient, TImplementation>(client =>
        {
            configureClient?.Invoke(client);
        });

        builder.AddPolicyHandler((provider, _) =>
        {
            var logger = provider.GetService<ILoggerFactory>()?.CreateLogger(clientName);
            return PolicyFactory.CreateLongRunningHttpPolicy(clientName, options, logger);
        });

        return builder;
    }

    /// <summary>
    /// Adds standard Mystira resilience policy to an existing HTTP client builder.
    /// Use this to add resilience to a client that was already configured with AddHttpClient.
    /// </summary>
    /// <param name="builder">The HTTP client builder.</param>
    /// <param name="clientName">Name of the client for logging.</param>
    /// <param name="options">Optional resilience options. Uses defaults if null.</param>
    /// <returns>The IHttpClientBuilder for further configuration.</returns>
    /// <example>
    /// <code>
    /// services.AddHttpClient&lt;IMyClient, MyClient&gt;()
    ///     .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://api.example.com"))
    ///     .AddMystiraResiliencePolicy("MyClient");
    /// </code>
    /// </example>
    public static IHttpClientBuilder AddMystiraResiliencePolicy(
        this IHttpClientBuilder builder,
        string clientName,
        ResilienceOptions? options = null)
    {
        builder.AddPolicyHandler((provider, _) =>
        {
            var logger = provider.GetService<ILoggerFactory>()?.CreateLogger(clientName);
            return PolicyFactory.CreateStandardHttpPolicy(clientName, options, logger);
        });

        return builder;
    }

    /// <summary>
    /// Adds long-running Mystira resilience policy to an existing HTTP client builder.
    /// Suitable for LLM API calls and other operations that may take minutes.
    /// </summary>
    /// <param name="builder">The HTTP client builder.</param>
    /// <param name="clientName">Name of the client for logging.</param>
    /// <param name="options">Optional resilience options. Uses defaults if null.</param>
    /// <returns>The IHttpClientBuilder for further configuration.</returns>
    /// <example>
    /// <code>
    /// services.AddHttpClient&lt;ILlmClient, LlmClient&gt;()
    ///     .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromMinutes(5))
    ///     .AddMystiraLongRunningResiliencePolicy("LlmClient");
    /// </code>
    /// </example>
    public static IHttpClientBuilder AddMystiraLongRunningResiliencePolicy(
        this IHttpClientBuilder builder,
        string clientName,
        ResilienceOptions? options = null)
    {
        builder.AddPolicyHandler((provider, _) =>
        {
            var logger = provider.GetService<ILoggerFactory>()?.CreateLogger(clientName);
            return PolicyFactory.CreateLongRunningHttpPolicy(clientName, options, logger);
        });

        return builder;
    }
}
