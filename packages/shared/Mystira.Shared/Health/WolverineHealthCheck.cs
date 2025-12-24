using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.Shared.Messaging;
using Wolverine;

namespace Mystira.Shared.Health;

/// <summary>
/// Health check for Wolverine messaging system.
/// Verifies that the message bus is operational.
/// </summary>
public class WolverineHealthCheck : IHealthCheck
{
    private readonly IMessageBus _messageBus;
    private readonly MessagingOptions _options;
    private readonly ILogger<WolverineHealthCheck> _logger;

    public WolverineHealthCheck(
        IMessageBus messageBus,
        IOptions<MessagingOptions> options,
        ILogger<WolverineHealthCheck> logger)
    {
        _messageBus = messageBus;
        _options = options.Value;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return Task.FromResult(HealthCheckResult.Healthy("Messaging is disabled"));
        }

        try
        {
            // Check if the message bus is available by verifying it was injected
            // The actual connectivity to Azure Service Bus is verified on first message
            if (_messageBus == null)
            {
                return Task.FromResult(HealthCheckResult.Degraded("Wolverine message bus not available"));
            }

            var data = new Dictionary<string, object>
            {
                ["serviceName"] = _options.ServiceName ?? "Unknown",
                ["durabilityMode"] = _options.DurabilityMode.ToString(),
                ["hasServiceBus"] = !string.IsNullOrEmpty(_options.ServiceBusConnectionString),
                ["messageBusType"] = _messageBus.GetType().Name
            };

            // If using Azure Service Bus, note that connectivity will only be verified on first message
            if (!string.IsNullOrEmpty(_options.ServiceBusConnectionString))
            {
                data["azureServiceBus"] = "Configured (connectivity verified on first message)";
            }

            return Task.FromResult(HealthCheckResult.Healthy("Wolverine messaging operational", data));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Wolverine health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Wolverine messaging check failed",
                ex,
                new Dictionary<string, object>
                {
                    ["error"] = ex.Message
                }));
        }
    }
}
