using Microsoft.Extensions.Logging;

namespace Mystira.Application.CQRS.Health.Queries;

/// <summary>
/// Wolverine message handler for liveness probe.
/// Simple check indicating application is alive and running.
/// </summary>
public static class GetLivenessQueryHandler
{
    /// <summary>
    /// Handles the GetLivenessQuery.
    /// </summary>
    /// <param name="request">The query to handle.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The probe result indicating liveness status.</returns>
    public static Task<ProbeResult> Handle(
        GetLivenessQuery request,
        ILogger<GetLivenessQuery> logger,
        CancellationToken ct)
    {
        logger.LogDebug("Liveness probe checked");

        return Task.FromResult(new ProbeResult(
            Status: "alive",
            Timestamp: DateTime.UtcNow
        ));
    }
}
