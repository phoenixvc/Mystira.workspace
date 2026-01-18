using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Mystira.Shared.Caching;

/// <summary>
/// Provides cache observability metrics using OpenTelemetry-compatible instrumentation.
/// Tracks cache hits, misses, errors, and latency for monitoring and tuning.
/// </summary>
public class CacheMetrics : IDisposable
{
    private static readonly Meter s_meter = new("Mystira.Shared.Caching", "1.0.0");
    private static readonly ActivitySource s_activitySource = new("Mystira.Shared.Caching");

    private readonly Counter<long> _hits;
    private readonly Counter<long> _misses;
    private readonly Counter<long> _errors;
    private readonly Counter<long> _sets;
    private readonly Counter<long> _removes;
    private readonly Histogram<double> _latency;
    private bool _disposed;

    /// <summary>
    /// Gets the singleton instance of cache metrics.
    /// </summary>
    public static CacheMetrics Instance { get; } = new();

    /// <summary>
    /// Initializes cache metrics counters and histograms.
    /// </summary>
    public CacheMetrics()
    {
        _hits = s_meter.CreateCounter<long>(
            "mystira.cache.hits",
            "operations",
            "Number of cache hits");

        _misses = s_meter.CreateCounter<long>(
            "mystira.cache.misses",
            "operations",
            "Number of cache misses");

        _errors = s_meter.CreateCounter<long>(
            "mystira.cache.errors",
            "operations",
            "Number of cache operation errors");

        _sets = s_meter.CreateCounter<long>(
            "mystira.cache.sets",
            "operations",
            "Number of cache set operations");

        _removes = s_meter.CreateCounter<long>(
            "mystira.cache.removes",
            "operations",
            "Number of cache remove operations");

        _latency = s_meter.CreateHistogram<double>(
            "mystira.cache.latency",
            "ms",
            "Cache operation latency in milliseconds");
    }

    /// <summary>
    /// Records a cache hit.
    /// </summary>
    /// <param name="keyPattern">Key pattern category (e.g., "scenario", "badge", "user")</param>
    public void RecordHit(string? keyPattern = null)
    {
        var tags = GetTags(keyPattern, "hit");
        _hits.Add(1, tags);
    }

    /// <summary>
    /// Records a cache miss.
    /// </summary>
    /// <param name="keyPattern">Key pattern category (e.g., "scenario", "badge", "user")</param>
    public void RecordMiss(string? keyPattern = null)
    {
        var tags = GetTags(keyPattern, "miss");
        _misses.Add(1, tags);
    }

    /// <summary>
    /// Records a cache error.
    /// </summary>
    /// <param name="operation">Operation type (get, set, remove)</param>
    /// <param name="keyPattern">Key pattern category</param>
    public void RecordError(string operation, string? keyPattern = null)
    {
        var tags = new TagList
        {
            { "operation", operation },
            { "key_pattern", keyPattern ?? "unknown" }
        };
        _errors.Add(1, tags);
    }

    /// <summary>
    /// Records a cache set operation.
    /// </summary>
    /// <param name="keyPattern">Key pattern category</param>
    public void RecordSet(string? keyPattern = null)
    {
        var tags = GetTags(keyPattern, "set");
        _sets.Add(1, tags);
    }

    /// <summary>
    /// Records a cache remove operation.
    /// </summary>
    /// <param name="keyPattern">Key pattern category</param>
    public void RecordRemove(string? keyPattern = null)
    {
        var tags = GetTags(keyPattern, "remove");
        _removes.Add(1, tags);
    }

    /// <summary>
    /// Records cache operation latency.
    /// </summary>
    /// <param name="latencyMs">Latency in milliseconds</param>
    /// <param name="operation">Operation type</param>
    /// <param name="keyPattern">Key pattern category</param>
    public void RecordLatency(double latencyMs, string operation, string? keyPattern = null)
    {
        var tags = new TagList
        {
            { "operation", operation },
            { "key_pattern", keyPattern ?? "unknown" }
        };
        _latency.Record(latencyMs, tags);
    }

    /// <summary>
    /// Starts an activity span for cache operation tracing.
    /// </summary>
    /// <param name="operation">Operation name</param>
    /// <param name="key">Cache key</param>
    /// <returns>Activity for distributed tracing</returns>
    public Activity? StartActivity(string operation, string key)
    {
        var activity = s_activitySource.StartActivity($"Cache.{operation}");
        activity?.SetTag("cache.key", key);
        activity?.SetTag("cache.key_pattern", ExtractKeyPattern(key));
        return activity;
    }

    /// <summary>
    /// Extracts a key pattern from a cache key for grouping metrics.
    /// </summary>
    /// <param name="key">Full cache key</param>
    /// <returns>Key pattern category</returns>
    public static string ExtractKeyPattern(string key)
    {
        if (string.IsNullOrEmpty(key))
            return "unknown";

        // Extract pattern from keys like "mystira:scenario:123" -> "scenario"
        // or "mystira-admin:content:scenarios:list" -> "scenarios"
        var parts = key.Split(':', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length >= 2)
        {
            // Skip the prefix (mystira, mystira-admin) and get the type
            var typeIndex = parts[0].Contains("mystira") ? 1 : 0;
            if (typeIndex < parts.Length)
            {
                return parts[typeIndex].ToLowerInvariant();
            }
        }

        return "other";
    }

    private static TagList GetTags(string? keyPattern, string operation)
    {
        return new TagList
        {
            { "key_pattern", keyPattern ?? "unknown" },
            { "operation", operation }
        };
    }

    /// <summary>
    /// Disposes the metrics resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        s_meter.Dispose();
        s_activitySource.Dispose();
    }
}
