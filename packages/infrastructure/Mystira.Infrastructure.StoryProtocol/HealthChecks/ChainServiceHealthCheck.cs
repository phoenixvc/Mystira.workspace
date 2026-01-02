using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.Application.Configuration.StoryProtocol;
using Mystira.Infrastructure.StoryProtocol.Services.Grpc;

namespace Mystira.Infrastructure.StoryProtocol.HealthChecks;

/// <summary>
/// Health check for the Mystira.Chain gRPC service.
/// </summary>
public class ChainServiceHealthCheck : IHealthCheck
{
    private readonly ILogger<ChainServiceHealthCheck> _logger;
    private readonly ChainServiceOptions _options;
    private readonly GrpcStoryProtocolService? _grpcService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChainServiceHealthCheck"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The chain service options.</param>
    /// <param name="grpcService">The gRPC service (optional, may be null if mock is used).</param>
    public ChainServiceHealthCheck(
        ILogger<ChainServiceHealthCheck> logger,
        IOptions<ChainServiceOptions> options,
        GrpcStoryProtocolService? grpcService = null)
    {
        _logger = logger;
        _options = options.Value;
        _grpcService = grpcService;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // If we're using mock implementation, always return healthy
            if (!_options.UseGrpc || _grpcService == null)
            {
                return HealthCheckResult.Healthy(
                    "Chain service is using mock implementation",
                    new Dictionary<string, object>
                    {
                        ["useGrpc"] = false,
                        ["implementation"] = "mock"
                    });
            }

            var isHealthy = await _grpcService.IsHealthyAsync();

            if (isHealthy)
            {
                return HealthCheckResult.Healthy(
                    "Chain service is healthy",
                    new Dictionary<string, object>
                    {
                        ["endpoint"] = _options.GrpcEndpoint,
                        ["useGrpc"] = true,
                        ["implementation"] = "grpc"
                    });
            }

            return HealthCheckResult.Degraded(
                "Chain service is not responding",
                data: new Dictionary<string, object>
                {
                    ["endpoint"] = _options.GrpcEndpoint,
                    ["useGrpc"] = true
                });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Chain service health check failed");

            return HealthCheckResult.Unhealthy(
                "Chain service health check failed",
                ex,
                new Dictionary<string, object>
                {
                    ["endpoint"] = _options.GrpcEndpoint,
                    ["error"] = ex.Message
                });
        }
    }
}
