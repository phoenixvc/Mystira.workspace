using System.Collections.Concurrent;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mystira.Shared.Logging;

namespace Mystira.Shared.Telemetry;

/// <summary>
/// Service for tracking security-related metrics and events in Application Insights.
/// Use this service to monitor authentication failures, rate limiting, and suspicious activity.
/// </summary>
public interface ISecurityMetrics
{
    /// <summary>
    /// Tracks a failed authentication attempt.
    /// </summary>
    void TrackAuthenticationFailed(string method, string? clientIp, string? reason = null);

    /// <summary>
    /// Tracks a successful authentication.
    /// </summary>
    void TrackAuthenticationSuccess(string method, string? userId = null);

    /// <summary>
    /// Tracks a JWT token validation failure.
    /// </summary>
    void TrackTokenValidationFailed(string? clientIp, string reason);

    /// <summary>
    /// Tracks a rate limit hit.
    /// </summary>
    void TrackRateLimitHit(string? clientIp, string endpoint);

    /// <summary>
    /// Tracks sustained rate limiting (many hits in short time).
    /// </summary>
    void TrackRateLimitSustained(string? clientIp, int hitCount, TimeSpan window);

    /// <summary>
    /// Tracks a suspicious request pattern.
    /// </summary>
    void TrackSuspiciousRequest(string? clientIp, string pattern, string? details = null);

    /// <summary>
    /// Tracks an authorization failure (user authenticated but not authorized).
    /// </summary>
    void TrackAuthorizationFailed(string? userId, string resource, string? reason = null);

    /// <summary>
    /// Tracks a brute force detection event.
    /// </summary>
    void TrackBruteForceDetected(string? clientIp, int attemptCount, TimeSpan window);

    /// <summary>
    /// Tracks an invalid input that could indicate injection attempt.
    /// </summary>
    void TrackInvalidInput(string inputType, string? clientIp, string? details = null);

    /// <summary>
    /// Tracks a CORS violation.
    /// </summary>
    void TrackCorsViolation(string? origin, string? clientIp);
}

/// <summary>
/// Implementation of ISecurityMetrics that sends telemetry to Application Insights.
/// Includes real-time brute force and sustained rate limit detection.
/// </summary>
public class SecurityMetrics : ISecurityMetrics
{
    private readonly TelemetryClient? _telemetryClient;
    private readonly ILogger<SecurityMetrics> _logger;
    private readonly string _environment;

    // Alert thresholds
    private const int BruteForceThreshold = 10;
    private const int RateLimitSustainedThreshold = 100;
    private static readonly TimeSpan BruteForceWindow = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(1);

    // Memory safety: Maximum number of IPs to track
    private const int MaxTrackedIps = 10000;

    // In-memory tracking for real-time detection (per IP)
    private readonly ConcurrentDictionary<string, List<DateTime>> _authFailuresByIp = new();
    private readonly ConcurrentDictionary<string, List<DateTime>> _rateLimitsByIp = new();
    private readonly object _cleanupLock = new();
    private DateTime _lastCleanup = DateTime.UtcNow;
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityMetrics"/> class.
    /// </summary>
    /// <param name="telemetryClient">Optional Application Insights telemetry client.</param>
    /// <param name="logger">Logger for diagnostic information.</param>
    /// <param name="environment">The environment name.</param>
    public SecurityMetrics(TelemetryClient? telemetryClient, ILogger<SecurityMetrics> logger, string environment)
    {
        _telemetryClient = telemetryClient;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _environment = environment ?? "Unknown";
    }

    private void CleanupOldEntries()
    {
        if (DateTime.UtcNow - _lastCleanup < CleanupInterval)
            return;

        lock (_cleanupLock)
        {
            if (DateTime.UtcNow - _lastCleanup < CleanupInterval)
                return;

            var cutoff = DateTime.UtcNow - (BruteForceWindow + BruteForceWindow);
            CleanupDictionary(_authFailuresByIp, cutoff, "auth failures");
            CleanupDictionary(_rateLimitsByIp, cutoff, "rate limits");
            _lastCleanup = DateTime.UtcNow;
        }
    }

    private void CleanupDictionary(ConcurrentDictionary<string, List<DateTime>> dictionary, DateTime cutoff, string context)
    {
        var keysWithTimestamps = dictionary.Keys.ToList()
            .Select(ip => new { Ip = ip, Timestamps = dictionary.TryGetValue(ip, out var ts) ? ts : null })
            .Where(x => x.Timestamps != null);

        foreach (var item in keysWithTimestamps)
        {
            lock (item.Timestamps!)
            {
                item.Timestamps.RemoveAll(t => t < cutoff);
                if (item.Timestamps.Count == 0)
                {
                    dictionary.TryRemove(item.Ip, out _);
                }
            }
        }

        if (dictionary.Count > MaxTrackedIps)
        {
            var toRemove = dictionary
                .Select(kvp => new { Ip = kvp.Key, LastActivity = kvp.Value.Count > 0 ? kvp.Value.Max() : DateTime.MinValue })
                .OrderBy(x => x.LastActivity)
                .Take(dictionary.Count - MaxTrackedIps)
                .Select(x => x.Ip)
                .ToList();

            foreach (var ip in toRemove)
            {
                dictionary.TryRemove(ip, out _);
            }

            _logger.LogWarning("SecurityMetrics {Context} dictionary exceeded {MaxSize} entries, evicted {EvictedCount} oldest entries",
                context, MaxTrackedIps, toRemove.Count);
        }
    }

    private void CheckAndTrackBruteForce(string? clientIp)
    {
        if (string.IsNullOrEmpty(clientIp)) return;

        CleanupOldEntries();
        var timestamps = _authFailuresByIp.GetOrAdd(clientIp, _ => new List<DateTime>());
        var now = DateTime.UtcNow;
        var windowStart = now - BruteForceWindow;

        int recentCount;
        lock (timestamps)
        {
            timestamps.Add(now);
            timestamps.RemoveAll(t => t < windowStart);
            recentCount = timestamps.Count;
        }

        if (recentCount >= BruteForceThreshold)
        {
            TrackBruteForceDetected(clientIp, recentCount, BruteForceWindow);
        }
    }

    private void CheckAndTrackSustainedRateLimit(string? clientIp)
    {
        if (string.IsNullOrEmpty(clientIp)) return;

        CleanupOldEntries();
        var timestamps = _rateLimitsByIp.GetOrAdd(clientIp, _ => new List<DateTime>());
        var now = DateTime.UtcNow;
        var windowStart = now - RateLimitWindow;

        int recentCount;
        lock (timestamps)
        {
            timestamps.Add(now);
            timestamps.RemoveAll(t => t < windowStart);
            recentCount = timestamps.Count;
        }

        if (recentCount >= RateLimitSustainedThreshold)
        {
            TrackRateLimitSustained(clientIp, recentCount, RateLimitWindow);
        }
    }

    /// <inheritdoc />
    public void TrackAuthenticationFailed(string method, string? clientIp, string? reason = null)
    {
        var properties = new Dictionary<string, string>
        {
            ["Method"] = method,
            ["Environment"] = _environment,
            ["EventType"] = "SecurityEvent"
        };

        if (!string.IsNullOrEmpty(clientIp))
            properties["ClientIP"] = PiiRedactor.MaskIp(clientIp);

        if (!string.IsNullOrEmpty(reason))
            properties["Reason"] = reason;

        TrackEvent("Security.AuthenticationFailed", properties);
        TrackMetric("Security.AuthFailures", 1, properties);

        _logger.LogWarning("Authentication failed via {Method} from {ClientIp}: {Reason}",
            method, PiiRedactor.MaskIp(clientIp), reason ?? "Unknown");

        CheckAndTrackBruteForce(clientIp);
    }

    /// <inheritdoc />
    public void TrackAuthenticationSuccess(string method, string? userId = null)
    {
        var properties = new Dictionary<string, string>
        {
            ["Method"] = method,
            ["Environment"] = _environment,
            ["EventType"] = "SecurityEvent"
        };

        if (!string.IsNullOrEmpty(userId))
            properties["UserId"] = userId;

        TrackEvent("Security.AuthenticationSuccess", properties);
        TrackMetric("Security.AuthSuccesses", 1, properties);

        _logger.LogInformation("Authentication succeeded via {Method} for user {UserId}", method, userId ?? "Unknown");
    }

    /// <inheritdoc />
    public void TrackTokenValidationFailed(string? clientIp, string reason)
    {
        var properties = new Dictionary<string, string>
        {
            ["Reason"] = reason,
            ["Environment"] = _environment,
            ["EventType"] = "SecurityEvent"
        };

        if (!string.IsNullOrEmpty(clientIp))
            properties["ClientIP"] = PiiRedactor.MaskIp(clientIp);

        TrackEvent("Security.TokenValidationFailed", properties);
        TrackMetric("Security.TokenValidationFailures", 1, properties);

        _logger.LogWarning("Token validation failed from {ClientIp}: {Reason}", PiiRedactor.MaskIp(clientIp), reason);
        CheckAndTrackBruteForce(clientIp);
    }

    /// <inheritdoc />
    public void TrackRateLimitHit(string? clientIp, string endpoint)
    {
        var properties = new Dictionary<string, string>
        {
            ["Endpoint"] = endpoint,
            ["Environment"] = _environment,
            ["EventType"] = "SecurityEvent"
        };

        if (!string.IsNullOrEmpty(clientIp))
            properties["ClientIP"] = PiiRedactor.MaskIp(clientIp);

        TrackEvent("Security.RateLimitHit", properties);
        TrackMetric("Security.RateLimitHits", 1, properties);

        _logger.LogWarning("Rate limit hit from {ClientIp} on {Endpoint}", PiiRedactor.MaskIp(clientIp), endpoint);
        CheckAndTrackSustainedRateLimit(clientIp);
    }

    /// <inheritdoc />
    public void TrackRateLimitSustained(string? clientIp, int hitCount, TimeSpan window)
    {
        var properties = new Dictionary<string, string>
        {
            ["HitCount"] = hitCount.ToString(),
            ["WindowSeconds"] = window.TotalSeconds.ToString(),
            ["Environment"] = _environment,
            ["EventType"] = "SecurityAlert"
        };

        if (!string.IsNullOrEmpty(clientIp))
            properties["ClientIP"] = PiiRedactor.MaskIp(clientIp);

        TrackEvent("Security.RateLimitSustained", properties);
        TrackMetric("Security.SustainedRateLimits", 1, properties);

        _logger.LogError("SECURITY ALERT: Sustained rate limiting detected from {ClientIp} - {HitCount} hits in {Window}",
            PiiRedactor.MaskIp(clientIp), hitCount, window);
    }

    /// <inheritdoc />
    public void TrackSuspiciousRequest(string? clientIp, string pattern, string? details = null)
    {
        var properties = new Dictionary<string, string>
        {
            ["Pattern"] = pattern,
            ["Environment"] = _environment,
            ["EventType"] = "SecurityAlert"
        };

        if (!string.IsNullOrEmpty(clientIp))
            properties["ClientIP"] = PiiRedactor.MaskIp(clientIp);

        if (!string.IsNullOrEmpty(details))
            properties["Details"] = details;

        TrackEvent("Security.SuspiciousRequest", properties);
        TrackMetric("Security.SuspiciousRequests", 1, properties);

        _logger.LogWarning("Suspicious request detected from {ClientIp}: {Pattern} - {Details}",
            PiiRedactor.MaskIp(clientIp), pattern, details ?? "No details");
    }

    /// <inheritdoc />
    public void TrackAuthorizationFailed(string? userId, string resource, string? reason = null)
    {
        var properties = new Dictionary<string, string>
        {
            ["Resource"] = resource,
            ["Environment"] = _environment,
            ["EventType"] = "SecurityEvent"
        };

        if (!string.IsNullOrEmpty(userId))
            properties["UserId"] = userId;

        if (!string.IsNullOrEmpty(reason))
            properties["Reason"] = reason;

        TrackEvent("Security.AuthorizationFailed", properties);
        TrackMetric("Security.AuthorizationFailures", 1, properties);

        _logger.LogWarning("Authorization failed for user {UserId} accessing {Resource}: {Reason}",
            userId ?? "Unknown", resource, reason ?? "Unknown");
    }

    /// <inheritdoc />
    public void TrackBruteForceDetected(string? clientIp, int attemptCount, TimeSpan window)
    {
        var properties = new Dictionary<string, string>
        {
            ["AttemptCount"] = attemptCount.ToString(),
            ["WindowSeconds"] = window.TotalSeconds.ToString(),
            ["Environment"] = _environment,
            ["EventType"] = "SecurityAlert"
        };

        if (!string.IsNullOrEmpty(clientIp))
            properties["ClientIP"] = PiiRedactor.MaskIp(clientIp);

        TrackEvent("Security.BruteForceDetected", properties);
        TrackMetric("Security.BruteForceAttempts", 1, properties);

        _logger.LogError("SECURITY ALERT: Brute force attack detected from {ClientIp} - {AttemptCount} attempts in {Window}",
            PiiRedactor.MaskIp(clientIp), attemptCount, window);
    }

    /// <inheritdoc />
    public void TrackInvalidInput(string inputType, string? clientIp, string? details = null)
    {
        var properties = new Dictionary<string, string>
        {
            ["InputType"] = inputType,
            ["Environment"] = _environment,
            ["EventType"] = "SecurityEvent"
        };

        if (!string.IsNullOrEmpty(clientIp))
            properties["ClientIP"] = PiiRedactor.MaskIp(clientIp);

        if (!string.IsNullOrEmpty(details))
            properties["Details"] = PiiRedactor.SanitizeLogInput(details);

        TrackEvent("Security.InvalidInput", properties);
        TrackMetric("Security.InvalidInputs", 1, properties);

        _logger.LogWarning("Invalid input detected ({InputType}) from {ClientIp}", inputType, PiiRedactor.MaskIp(clientIp));
    }

    /// <inheritdoc />
    public void TrackCorsViolation(string? origin, string? clientIp)
    {
        var properties = new Dictionary<string, string>
        {
            ["Environment"] = _environment,
            ["EventType"] = "SecurityEvent"
        };

        if (!string.IsNullOrEmpty(origin))
            properties["Origin"] = origin;

        if (!string.IsNullOrEmpty(clientIp))
            properties["ClientIP"] = PiiRedactor.MaskIp(clientIp);

        TrackEvent("Security.CorsViolation", properties);
        TrackMetric("Security.CorsViolations", 1, properties);

        _logger.LogWarning("CORS violation from origin {Origin} (IP: {ClientIp})", origin, PiiRedactor.MaskIp(clientIp));
    }

    private void TrackEvent(string name, IDictionary<string, string>? properties = null)
    {
        if (_telemetryClient == null)
        {
            _logger.LogDebug("Security event tracked (no App Insights): {Name}", name);
            return;
        }

        var eventTelemetry = new EventTelemetry(name);
        if (properties != null)
        {
            foreach (var prop in properties)
            {
                eventTelemetry.Properties[prop.Key] = prop.Value;
            }
        }

        _telemetryClient.TrackEvent(eventTelemetry);
    }

    private void TrackMetric(string name, double value, IDictionary<string, string>? properties = null)
    {
        if (_telemetryClient == null)
        {
            _logger.LogDebug("Security metric tracked (no App Insights): {Name} = {Value}", name, value);
            return;
        }

        var metric = new MetricTelemetry(name, value);
        if (properties != null)
        {
            foreach (var prop in properties)
            {
                metric.Properties[prop.Key] = prop.Value;
            }
        }

        _telemetryClient.TrackMetric(metric);
    }
}

/// <summary>
/// Extension methods for registering SecurityMetrics in DI.
/// </summary>
public static class SecurityMetricsExtensions
{
    /// <summary>
    /// Adds ISecurityMetrics service to the DI container.
    /// </summary>
    public static IServiceCollection AddSecurityMetrics(this IServiceCollection services, string environment)
    {
        services.AddSingleton<ISecurityMetrics>(sp =>
        {
            var telemetryClient = sp.GetService<TelemetryClient>();
            var logger = sp.GetRequiredService<ILogger<SecurityMetrics>>();
            return new SecurityMetrics(telemetryClient, logger, environment);
        });

        return services;
    }
}
