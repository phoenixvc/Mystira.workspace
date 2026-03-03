using Microsoft.Extensions.Logging;

namespace Mystira.Application.CQRS.Health.Queries;

/// <summary>
/// Wolverine message handler for readiness probe.
/// Simple check indicating application is ready to receive traffic.
/// </summary>
public static class GetReadinessQueryHandler
{
    /// <summary>
    /// Handles the GetReadinessQuery.
    /// </summary>
    /// <param name="request">The query to handle.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The probe result indicating readiness status.</returns>
    public static Task<ProbeResult> Handle(
        GetReadinessQuery request,
        ILogger<GetReadinessQuery> logger,
        CancellationToken ct)
    {
        logger.LogDebug("Readiness probe checked");

        return Task.FromResult(new ProbeResult(
            Status: "ready",
            Timestamp: DateTime.UtcNow
        ));
    }
}
