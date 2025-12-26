using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mystira.Shared.Middleware;

/// <summary>
/// Rate limiting options.
/// </summary>
public sealed class RateLimitOptions
{
    /// <summary>
    /// Maximum requests per window.
    /// </summary>
    public int RequestsPerWindow { get; set; } = 100;

    /// <summary>
    /// Window duration.
    /// </summary>
    public TimeSpan WindowDuration { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Whether to include rate limit headers in responses.
    /// </summary>
    public bool IncludeHeaders { get; set; } = true;

    /// <summary>
    /// Endpoints to exclude from rate limiting (e.g., "/health").
    /// </summary>
    public HashSet<string> ExcludedPaths { get; set; } = ["/health", "/health/ready", "/health/live"];
}

/// <summary>
/// Simple in-memory rate limiting middleware.
/// For production, consider using Redis-backed rate limiting.
/// </summary>
public sealed class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitOptions _options;
    private readonly ConcurrentDictionary<string, RateLimitEntry> _entries = new();

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        IOptions<RateLimitOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // Skip excluded paths
        if (_options.ExcludedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var clientId = GetClientId(context);
        var now = DateTimeOffset.UtcNow;

        var entry = _entries.AddOrUpdate(
            clientId,
            _ => new RateLimitEntry(1, now),
            (_, existing) =>
            {
                if (now - existing.WindowStart >= _options.WindowDuration)
                {
                    // New window
                    return new RateLimitEntry(1, now);
                }
                // Same window
                return existing with { RequestCount = existing.RequestCount + 1 };
            });

        var remaining = Math.Max(0, _options.RequestsPerWindow - entry.RequestCount);
        var resetTime = entry.WindowStart + _options.WindowDuration;

        if (_options.IncludeHeaders)
        {
            context.Response.Headers["X-RateLimit-Limit"] = _options.RequestsPerWindow.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
            context.Response.Headers["X-RateLimit-Reset"] = resetTime.ToUnixTimeSeconds().ToString();
        }

        if (entry.RequestCount > _options.RequestsPerWindow)
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId}", clientId);

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["Retry-After"] = ((int)(resetTime - now).TotalSeconds).ToString();
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Rate limit exceeded",
                retryAfterSeconds = (int)(resetTime - now).TotalSeconds
            });
            return;
        }

        await _next(context);
    }

    private static string GetClientId(HttpContext context)
    {
        // Prefer authenticated user ID
        var userId = context.User?.FindFirst("sub")?.Value
                  ?? context.User?.FindFirst("oid")?.Value;

        if (!string.IsNullOrEmpty(userId))
            return $"user:{userId}";

        // Fall back to IP address
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Check for forwarded IP (behind proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            ip = forwardedFor.Split(',')[0].Trim();
        }

        return $"ip:{ip}";
    }

    private sealed record RateLimitEntry(int RequestCount, DateTimeOffset WindowStart);
}

/// <summary>
/// Extension methods for rate limiting.
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>
    /// Adds rate limiting middleware.
    /// </summary>
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RateLimitingMiddleware>();
    }
}
