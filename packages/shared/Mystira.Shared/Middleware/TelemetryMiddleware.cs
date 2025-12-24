using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Mystira.Shared.Middleware;

/// <summary>
/// Middleware for request telemetry and logging.
/// </summary>
public class TelemetryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TelemetryMiddleware> _logger;

    public TelemetryMiddleware(RequestDelegate next, ILogger<TelemetryMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Activity.Current?.Id ?? context.TraceIdentifier;

        // Add correlation ID to response headers
        context.Response.Headers.TryAdd("X-Request-Id", requestId);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var logLevel = context.Response.StatusCode >= 500 ? LogLevel.Error
                : context.Response.StatusCode >= 400 ? LogLevel.Warning
                : LogLevel.Information;

            _logger.Log(logLevel,
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}

/// <summary>
/// Options for telemetry configuration.
/// </summary>
public class TelemetryOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "Mystira:Telemetry";

    /// <summary>
    /// Service name for telemetry
    /// </summary>
    public string ServiceName { get; set; } = "mystira-service";

    /// <summary>
    /// Whether to enable distributed tracing
    /// </summary>
    public bool EnableTracing { get; set; } = true;

    /// <summary>
    /// Whether to enable metrics collection
    /// </summary>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>
    /// Paths to exclude from telemetry (e.g., health checks)
    /// </summary>
    public string[] ExcludePaths { get; set; } = { "/health", "/ready", "/metrics" };
}
