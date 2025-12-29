using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mystira.Shared.Telemetry;

/// <summary>
/// Service for tracking business-related metrics in Application Insights.
/// Use this service to monitor sessions, cache performance, and event processing.
/// </summary>
public interface IBusinessMetrics
{
    // Session metrics
    /// <summary>Tracks when a new session is started.</summary>
    /// <param name="scenarioId">The scenario identifier.</param>
    /// <param name="accountId">Optional account identifier.</param>
    void TrackSessionStarted(string scenarioId, string? accountId = null);
    
    /// <summary>Tracks when a session is completed successfully.</summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="durationSeconds">Duration in seconds.</param>
    /// <param name="outcome">Optional outcome description.</param>
    void TrackSessionCompleted(string sessionId, int durationSeconds, string? outcome = null);
    
    /// <summary>Tracks when a session is abandoned before completion.</summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="durationSeconds">Duration in seconds before abandonment.</param>
    void TrackSessionAbandoned(string sessionId, int durationSeconds);

    // Cache metrics
    /// <summary>Tracks a cache hit.</summary>
    /// <param name="cacheType">Type of cache.</param>
    /// <param name="key">Cache key.</param>
    void TrackCacheHit(string cacheType, string key);
    
    /// <summary>Tracks a cache miss.</summary>
    /// <param name="cacheType">Type of cache.</param>
    /// <param name="key">Cache key.</param>
    void TrackCacheMiss(string cacheType, string key);
    
    /// <summary>Tracks a cache eviction.</summary>
    /// <param name="cacheType">Type of cache.</param>
    /// <param name="key">Cache key.</param>
    /// <param name="reason">Reason for eviction.</param>
    void TrackCacheEviction(string cacheType, string key, string reason);
    
    /// <summary>Tracks cache operation latency.</summary>
    /// <param name="cacheType">Type of cache.</param>
    /// <param name="operation">Operation performed.</param>
    /// <param name="milliseconds">Duration in milliseconds.</param>
    void TrackCacheLatency(string cacheType, string operation, double milliseconds);

    // Event processing metrics
    /// <summary>Tracks when an event is published.</summary>
    /// <param name="eventType">Type of event.</param>
    void TrackEventPublished(string eventType);
    
    /// <summary>Tracks when an event is processed.</summary>
    /// <param name="eventType">Type of event.</param>
    /// <param name="processingTimeMs">Processing time in milliseconds.</param>
    /// <param name="success">Whether processing was successful.</param>
    void TrackEventProcessed(string eventType, double processingTimeMs, bool success);
    
    /// <summary>Tracks when event processing fails.</summary>
    /// <param name="eventType">Type of event.</param>
    /// <param name="error">Error description.</param>
    void TrackEventFailed(string eventType, string error);

    // API metrics
    /// <summary>Tracks an API call.</summary>
    /// <param name="endpoint">API endpoint.</param>
    /// <param name="method">HTTP method.</param>
    /// <param name="statusCode">HTTP status code.</param>
    /// <param name="durationMs">Duration in milliseconds.</param>
    void TrackApiCall(string endpoint, string method, int statusCode, double durationMs);

    // Custom business metrics
    /// <summary>Tracks a custom metric.</summary>
    /// <param name="name">Metric name.</param>
    /// <param name="value">Metric value.</param>
    /// <param name="properties">Optional properties.</param>
    void TrackMetric(string name, double value, IDictionary<string, string>? properties = null);
    
    /// <summary>Tracks a custom event.</summary>
    /// <param name="name">Event name.</param>
    /// <param name="properties">Optional properties.</param>
    void TrackEvent(string name, IDictionary<string, string>? properties = null);
}

/// <summary>
/// Implementation of IBusinessMetrics that sends telemetry to Application Insights.
/// </summary>
public class BusinessMetrics : IBusinessMetrics
{
    private readonly TelemetryClient? _telemetryClient;
    private readonly ILogger<BusinessMetrics> _logger;
    private readonly string _serviceName;
    private readonly string _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessMetrics"/> class.
    /// </summary>
    /// <param name="telemetryClient">Optional Application Insights telemetry client.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="serviceName">Name of the service.</param>
    /// <param name="environment">Environment name.</param>
    public BusinessMetrics(
        TelemetryClient? telemetryClient,
        ILogger<BusinessMetrics> logger,
        string serviceName,
        string environment)
    {
        _telemetryClient = telemetryClient;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceName = serviceName ?? "Unknown";
        _environment = environment ?? "Unknown";
    }

    #region Session Metrics

    /// <inheritdoc />
    public void TrackSessionStarted(string scenarioId, string? accountId = null)
    {
        var properties = CreateBaseProperties();
        properties["ScenarioId"] = scenarioId;
        if (accountId != null) properties["AccountId"] = accountId;

        InternalTrackEvent("Session.Started", properties);
        InternalTrackMetric("Session.StartCount", 1, properties);

        _logger.LogInformation("Session started for scenario {ScenarioId}", scenarioId);
    }

    /// <inheritdoc />
    public void TrackSessionCompleted(string sessionId, int durationSeconds, string? outcome = null)
    {
        var properties = CreateBaseProperties();
        properties["SessionId"] = sessionId;
        properties["DurationSeconds"] = durationSeconds.ToString();
        if (outcome != null) properties["Outcome"] = outcome;

        InternalTrackEvent("Session.Completed", properties);
        InternalTrackMetric("Session.CompletionCount", 1, properties);
        InternalTrackMetric("Session.DurationSeconds", durationSeconds, properties);

        _logger.LogInformation("Session {SessionId} completed in {Duration}s with outcome {Outcome}",
            sessionId, durationSeconds, outcome ?? "N/A");
    }

    /// <inheritdoc />
    public void TrackSessionAbandoned(string sessionId, int durationSeconds)
    {
        var properties = CreateBaseProperties();
        properties["SessionId"] = sessionId;
        properties["DurationSeconds"] = durationSeconds.ToString();

        InternalTrackEvent("Session.Abandoned", properties);
        InternalTrackMetric("Session.AbandonCount", 1, properties);
        InternalTrackMetric("Session.AbandonDurationSeconds", durationSeconds, properties);

        _logger.LogInformation("Session {SessionId} abandoned after {Duration}s", sessionId, durationSeconds);
    }

    #endregion

    #region Cache Metrics

    /// <inheritdoc />
    public void TrackCacheHit(string cacheType, string key)
    {
        var properties = CreateBaseProperties();
        properties["CacheType"] = cacheType;
        properties["KeyPattern"] = TruncateKey(key);

        InternalTrackMetric("Cache.HitCount", 1, properties);
    }

    /// <inheritdoc />
    public void TrackCacheMiss(string cacheType, string key)
    {
        var properties = CreateBaseProperties();
        properties["CacheType"] = cacheType;
        properties["KeyPattern"] = TruncateKey(key);

        InternalTrackMetric("Cache.MissCount", 1, properties);
    }

    /// <inheritdoc />
    public void TrackCacheEviction(string cacheType, string key, string reason)
    {
        var properties = CreateBaseProperties();
        properties["CacheType"] = cacheType;
        properties["KeyPattern"] = TruncateKey(key);
        properties["Reason"] = reason;

        InternalTrackEvent("Cache.Eviction", properties);
        InternalTrackMetric("Cache.EvictionCount", 1, properties);
    }

    /// <inheritdoc />
    public void TrackCacheLatency(string cacheType, string operation, double milliseconds)
    {
        var properties = CreateBaseProperties();
        properties["CacheType"] = cacheType;
        properties["Operation"] = operation;

        InternalTrackMetric("Cache.LatencyMs", milliseconds, properties);
    }

    #endregion

    #region Event Processing Metrics

    /// <inheritdoc />
    public void TrackEventPublished(string eventType)
    {
        var properties = CreateBaseProperties();
        properties["EventType"] = eventType;

        InternalTrackMetric("Event.PublishedCount", 1, properties);
    }

    /// <inheritdoc />
    public void TrackEventProcessed(string eventType, double processingTimeMs, bool success)
    {
        var properties = CreateBaseProperties();
        properties["EventType"] = eventType;
        properties["Success"] = success.ToString();

        InternalTrackMetric("Event.ProcessedCount", 1, properties);
        InternalTrackMetric("Event.ProcessingTimeMs", processingTimeMs, properties);

        if (!success)
        {
            InternalTrackMetric("Event.FailureCount", 1, properties);
        }
    }

    /// <inheritdoc />
    public void TrackEventFailed(string eventType, string error)
    {
        var properties = CreateBaseProperties();
        properties["EventType"] = eventType;
        properties["Error"] = error;

        InternalTrackEvent("Event.Failed", properties);
        InternalTrackMetric("Event.FailureCount", 1, properties);

        _logger.LogWarning("Event processing failed for {EventType}: {Error}", eventType, error);
    }

    #endregion

    #region API Metrics

    /// <inheritdoc />
    public void TrackApiCall(string endpoint, string method, int statusCode, double durationMs)
    {
        var properties = CreateBaseProperties();
        properties["Endpoint"] = endpoint;
        properties["Method"] = method;
        properties["StatusCode"] = statusCode.ToString();
        properties["Success"] = (statusCode < 400).ToString();

        InternalTrackMetric("Api.RequestCount", 1, properties);
        InternalTrackMetric("Api.DurationMs", durationMs, properties);

        if (statusCode >= 400)
        {
            InternalTrackMetric("Api.ErrorCount", 1, properties);
        }
    }

    #endregion

    #region Custom Metrics

    /// <inheritdoc />
    public void TrackMetric(string name, double value, IDictionary<string, string>? properties = null)
    {
        var mergedProperties = CreateBaseProperties();
        if (properties != null)
        {
            foreach (var prop in properties)
            {
                mergedProperties[prop.Key] = prop.Value;
            }
        }

        InternalTrackMetric(name, value, mergedProperties);
    }

    /// <inheritdoc />
    public void TrackEvent(string name, IDictionary<string, string>? properties = null)
    {
        var mergedProperties = CreateBaseProperties();
        if (properties != null)
        {
            foreach (var prop in properties)
            {
                mergedProperties[prop.Key] = prop.Value;
            }
        }

        InternalTrackEvent(name, mergedProperties);
    }

    #endregion

    #region Private Methods

    private Dictionary<string, string> CreateBaseProperties()
    {
        return new Dictionary<string, string>
        {
            ["Service"] = _serviceName,
            ["Environment"] = _environment
        };
    }

    private static string TruncateKey(string key)
    {
        // Return key pattern without specific IDs for grouping
        if (key.Contains(':'))
        {
            var parts = key.Split(':');
            return parts.Length > 1 ? $"{parts[0]}:*" : key;
        }
        return key.Length > 50 ? key[..50] + "..." : key;
    }

    private void InternalTrackEvent(string name, IDictionary<string, string> properties)
    {
        if (_telemetryClient == null)
        {
            _logger.LogDebug("Business event tracked (no App Insights): {Name}", name);
            return;
        }

        var eventTelemetry = new EventTelemetry(name);
        foreach (var prop in properties)
        {
            eventTelemetry.Properties[prop.Key] = prop.Value;
        }

        _telemetryClient.TrackEvent(eventTelemetry);
    }

    private void InternalTrackMetric(string name, double value, IDictionary<string, string> properties)
    {
        if (_telemetryClient == null)
        {
            _logger.LogDebug("Business metric tracked (no App Insights): {Name} = {Value}", name, value);
            return;
        }

        var metric = new MetricTelemetry(name, value);
        foreach (var prop in properties)
        {
            metric.Properties[prop.Key] = prop.Value;
        }

        _telemetryClient.TrackMetric(metric);
    }

    #endregion
}

/// <summary>
/// Extension methods for registering BusinessMetrics in DI.
/// </summary>
public static class BusinessMetricsExtensions
{
    /// <summary>
    /// Adds IBusinessMetrics service to the DI container.
    /// </summary>
    public static IServiceCollection AddBusinessMetrics(
        this IServiceCollection services,
        string serviceName,
        string environment)
    {
        services.AddSingleton<IBusinessMetrics>(sp =>
        {
            var telemetryClient = sp.GetService<TelemetryClient>();
            var logger = sp.GetRequiredService<ILogger<BusinessMetrics>>();
            return new BusinessMetrics(telemetryClient, logger, serviceName, environment);
        });

        return services;
    }
}
