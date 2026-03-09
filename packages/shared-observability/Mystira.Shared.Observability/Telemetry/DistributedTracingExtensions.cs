using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Mystira.Shared.Telemetry;

/// <summary>
/// Extension methods for configuring distributed tracing with OpenTelemetry and Application Insights 3.x.
/// Enables W3C Trace Context propagation for end-to-end request correlation.
/// </summary>
public static class DistributedTracingExtensions
{
    /// <summary>
    /// Configure distributed tracing for the application.
    /// Registers OpenTelemetry resource attributes for service identification and
    /// the ActivityListenerHostedService for bridging Activity API with telemetry.
    /// </summary>
    public static IServiceCollection AddDistributedTracing(
        this IServiceCollection services,
        string serviceName,
        string environment)
    {
        // Register ActivityListener as a hosted service for proper lifecycle management
        services.AddHostedService<ActivityListenerHostedService>();

        // Configure OpenTelemetry with cloud role attributes and Mystira instrumentation
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddCloudRoleAttributes(serviceName, environment)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["service.version"] = typeof(DistributedTracingExtensions).Assembly
                        .GetName().Version?.ToString() ?? "1.0.0"
                }))
            .WithTracing(tracing => tracing
                .AddMystiraInstrumentation());

        return services;
    }

    /// <summary>
    /// Start a new traced operation span.
    /// </summary>
    /// <param name="operationName">Name of the operation</param>
    /// <param name="spanType">Type of span (e.g., "Database", "HTTP", "Cache")</param>
    /// <returns>Activity representing the span, or null if tracing is disabled</returns>
    public static Activity? StartOperation(string operationName, string spanType = "Internal")
    {
        var activity = MystiraActivitySource.Source.StartActivity(operationName, ActivityKind.Internal);
        activity?.SetTag("span.type", spanType);
        return activity;
    }

    /// <summary>
    /// Start a database operation span.
    /// </summary>
    public static Activity? StartDatabaseOperation(string operationName, string? collection = null)
    {
        var activity = MystiraActivitySource.Source.StartActivity($"DB: {operationName}", ActivityKind.Client);
        activity?.SetTag("span.type", "Database");
        activity?.SetTag("db.system", "cosmosdb");
        if (collection != null)
        {
            activity?.SetTag("db.collection", collection);
        }
        return activity;
    }

    /// <summary>
    /// Start an HTTP client operation span.
    /// </summary>
    public static Activity? StartHttpOperation(string method, string url)
    {
        var activity = MystiraActivitySource.Source.StartActivity($"HTTP {method}", ActivityKind.Client);
        activity?.SetTag("span.type", "HTTP");
        activity?.SetTag("http.method", method);
        activity?.SetTag("http.url", url);
        return activity;
    }

    /// <summary>
    /// Start a cache operation span.
    /// </summary>
    public static Activity? StartCacheOperation(string operationName, string cacheKey)
    {
        var activity = MystiraActivitySource.Source.StartActivity($"Cache: {operationName}", ActivityKind.Client);
        activity?.SetTag("span.type", "Cache");
        activity?.SetTag("cache.operation", operationName);
        activity?.SetTag("cache.key", cacheKey);
        return activity;
    }

    /// <summary>
    /// Start a gRPC operation span.
    /// </summary>
    public static Activity? StartGrpcOperation(string serviceName, string methodName)
    {
        var activity = MystiraActivitySource.Source.StartActivity($"gRPC: {serviceName}/{methodName}", ActivityKind.Client);
        activity?.SetTag("span.type", "gRPC");
        activity?.SetTag("rpc.service", serviceName);
        activity?.SetTag("rpc.method", methodName);
        return activity;
    }

    /// <summary>
    /// Mark the current activity as successful.
    /// </summary>
    public static void RecordSuccess(this Activity? activity)
    {
        activity?.SetStatus(ActivityStatusCode.Ok);
    }
}
