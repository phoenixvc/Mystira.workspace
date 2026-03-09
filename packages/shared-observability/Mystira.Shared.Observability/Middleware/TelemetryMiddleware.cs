using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mystira.Shared.Middleware;

/// <summary>
/// Middleware for request telemetry and logging.
/// </summary>
public class TelemetryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TelemetryMiddleware> _logger;
    private readonly TelemetryOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next request delegate in the pipeline.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="options">Telemetry configuration options.</param>
    public TelemetryMiddleware(
        RequestDelegate next,
        ILogger<TelemetryMiddleware> logger,
        IOptions<TelemetryOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Invokes the middleware for the HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Skip telemetry for excluded paths
        if (ShouldExcludePath(path))
        {
            await _next(context);
            return;
        }

        // Skip if both tracing and metrics are disabled
        if (!_options.EnableTracing && !_options.EnableMetrics)
        {
            await _next(context);
            return;
        }

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

            if (_options.EnableMetrics)
            {
                var logLevel = context.Response.StatusCode >= 500 ? LogLevel.Error
                    : context.Response.StatusCode >= 400 ? LogLevel.Warning
                    : LogLevel.Information;

                _logger.Log(logLevel,
                    "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms [Service: {ServiceName}]",
                    context.Request.Method,
                    path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    _options.ServiceName);
            }
        }
    }

    private bool ShouldExcludePath(string path)
    {
        if (_options.ExcludePaths == null || _options.ExcludePaths.Length == 0)
        {
            return false;
        }

        foreach (var excludePath in _options.ExcludePaths)
        {
            if (path.StartsWith(excludePath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
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
    public string[] ExcludePaths { get; set; } = ["/health", "/ready", "/metrics"];
}
