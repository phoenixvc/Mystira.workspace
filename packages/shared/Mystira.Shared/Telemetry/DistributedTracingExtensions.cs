using System.Diagnostics;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;

namespace Mystira.Shared.Telemetry;

/// <summary>
/// Extension methods for configuring distributed tracing with Application Insights.
/// Enables W3C Trace Context propagation for end-to-end request correlation.
/// </summary>
public static class DistributedTracingExtensions
{
    /// <summary>
    /// Configure distributed tracing for the application.
    /// </summary>
    public static IServiceCollection AddDistributedTracing(
        this IServiceCollection services,
        string serviceName,
        string environment)
    {
        // Add telemetry initializer for custom dimensions
        services.AddSingleton<ITelemetryInitializer>(sp =>
            new DistributedTracingInitializer(serviceName, environment));

        // Register ActivityListener as a hosted service for proper lifecycle management
        services.AddHostedService<ActivityListenerHostedService>();

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

/// <summary>
/// Telemetry initializer that adds distributed tracing context.
/// </summary>
public class DistributedTracingInitializer : ITelemetryInitializer
{
    private readonly string _serviceName;
    private readonly string _environment;
    private readonly string _serviceVersion;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedTracingInitializer"/> class.
    /// </summary>
    /// <param name="serviceName">The name of the service.</param>
    /// <param name="environment">The environment name (e.g., "Production", "Staging").</param>
    public DistributedTracingInitializer(string serviceName, string environment)
    {
        _serviceName = serviceName;
        _environment = environment;
        _serviceVersion = typeof(DistributedTracingInitializer).Assembly
            .GetName().Version?.ToString() ?? "1.0.0";
    }

    /// <inheritdoc />
    public void Initialize(Microsoft.ApplicationInsights.Channel.ITelemetry telemetry)
    {
        // Set cloud role for Application Map
        telemetry.Context.Cloud.RoleName = _serviceName;
        telemetry.Context.Cloud.RoleInstance = Environment.MachineName;

        // Add custom dimensions
        if (telemetry is ISupportProperties propTelemetry)
        {
            propTelemetry.Properties.TryAdd("Environment", _environment);
            propTelemetry.Properties.TryAdd("ServiceVersion", _serviceVersion);

            // Add trace context if available
            var activity = Activity.Current;
            if (activity != null)
            {
                propTelemetry.Properties.TryAdd("TraceId", activity.TraceId.ToString());
                propTelemetry.Properties.TryAdd("SpanId", activity.SpanId.ToString());
                if (activity.ParentSpanId != default)
                {
                    propTelemetry.Properties.TryAdd("ParentSpanId", activity.ParentSpanId.ToString());
                }
            }
        }
    }
}
