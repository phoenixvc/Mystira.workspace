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
    void TrackSessionStarted(string scenarioId, string? accountId = null);
    void TrackSessionCompleted(string sessionId, int durationSeconds, string? outcome = null);
    void TrackSessionAbandoned(string sessionId, int durationSeconds);

    // Cache metrics
    void TrackCacheHit(string cacheType, string key);
    void TrackCacheMiss(string cacheType, string key);
    void TrackCacheEviction(string cacheType, string key, string reason);
    void TrackCacheLatency(string cacheType, string operation, double milliseconds);

    // Event processing metrics
    void TrackEventPublished(string eventType);
    void TrackEventProcessed(string eventType, double processingTimeMs, bool success);
    void TrackEventFailed(string eventType, string error);

    // API metrics
    void TrackApiCall(string endpoint, string method, int statusCode, double durationMs);

    // Custom business metrics
    void TrackMetric(string name, double value, IDictionary<string, string>? properties = null);
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

    public void TrackSessionStarted(string scenarioId, string? accountId = null)
    {
        var properties = CreateBaseProperties();
        properties["ScenarioId"] = scenarioId;
        if (accountId != null) properties["AccountId"] = accountId;

        InternalTrackEvent("Session.Started", properties);
        InternalTrackMetric("Session.StartCount", 1, properties);

        _logger.LogInformation("Session started for scenario {ScenarioId}", scenarioId);
    }

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

    public void TrackCacheHit(string cacheType, string key)
    {
        var properties = CreateBaseProperties();
        properties["CacheType"] = cacheType;
        properties["KeyPattern"] = TruncateKey(key);

        InternalTrackMetric("Cache.HitCount", 1, properties);
    }

    public void TrackCacheMiss(string cacheType, string key)
    {
        var properties = CreateBaseProperties();
        properties["CacheType"] = cacheType;
        properties["KeyPattern"] = TruncateKey(key);

        InternalTrackMetric("Cache.MissCount", 1, properties);
    }

    public void TrackCacheEviction(string cacheType, string key, string reason)
    {
        var properties = CreateBaseProperties();
        properties["CacheType"] = cacheType;
        properties["KeyPattern"] = TruncateKey(key);
        properties["Reason"] = reason;

        InternalTrackEvent("Cache.Eviction", properties);
        InternalTrackMetric("Cache.EvictionCount", 1, properties);
    }

    public void TrackCacheLatency(string cacheType, string operation, double milliseconds)
    {
        var properties = CreateBaseProperties();
        properties["CacheType"] = cacheType;
        properties["Operation"] = operation;

        InternalTrackMetric("Cache.LatencyMs", milliseconds, properties);
    }

    #endregion

    #region Event Processing Metrics

    public void TrackEventPublished(string eventType)
    {
        var properties = CreateBaseProperties();
        properties["EventType"] = eventType;

        InternalTrackMetric("Event.PublishedCount", 1, properties);
    }

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
