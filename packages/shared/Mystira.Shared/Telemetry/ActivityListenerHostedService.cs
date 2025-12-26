using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Mystira.Shared.Telemetry;

/// <summary>
/// Hosted service that manages the lifecycle of the ActivityListener for distributed tracing.
/// Ensures proper disposal of the listener when the application shuts down.
/// Bridges .NET Activity API with Application Insights telemetry.
/// </summary>
public class ActivityListenerHostedService : IHostedService, IDisposable
{
    private readonly ILogger<ActivityListenerHostedService> _logger;
    private readonly TelemetryClient? _telemetryClient;
    private readonly string _activitySourcePrefix;
    private ActivityListener? _listener;

    /// <summary>
    /// Creates a new ActivityListenerHostedService.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="telemetryClient">Optional Application Insights telemetry client.</param>
    /// <param name="activitySourcePrefix">Prefix for activity sources to listen to. Defaults to "Mystira".</param>
    public ActivityListenerHostedService(
        ILogger<ActivityListenerHostedService> logger,
        TelemetryClient? telemetryClient = null,
        string activitySourcePrefix = "Mystira")
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _telemetryClient = telemetryClient;
        _activitySourcePrefix = activitySourcePrefix;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting ActivityListener for distributed tracing (prefix: {Prefix})", _activitySourcePrefix);

        _listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name.StartsWith(_activitySourcePrefix),
            Sample = (ref ActivityCreationOptions<ActivityContext> options) =>
                ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = activity =>
            {
                _logger.LogDebug("Activity started: {OperationName} ({TraceId})",
                    activity.OperationName, activity.TraceId);
            },
            ActivityStopped = activity =>
            {
                if (_telemetryClient != null && activity.Duration.TotalMilliseconds > 0)
                {
                    // Track as dependency for visibility in Application Map
                    var dependency = new DependencyTelemetry
                    {
                        Name = activity.OperationName,
                        Type = activity.GetTagItem("span.type")?.ToString() ?? "Internal",
                        Duration = activity.Duration,
                        Success = activity.Status != ActivityStatusCode.Error,
                        Timestamp = activity.StartTimeUtc
                    };

                    foreach (var tag in activity.Tags)
                    {
                        dependency.Properties[tag.Key] = tag.Value ?? "";
                    }

                    _telemetryClient.TrackDependency(dependency);
                }
            }
        };

        ActivitySource.AddActivityListener(_listener);
        _logger.LogInformation("ActivityListener started successfully");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping ActivityListener");
        Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_listener != null)
        {
            _listener.Dispose();
            _listener = null;
            _logger.LogInformation("ActivityListener disposed");
        }
    }
}
