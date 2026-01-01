using Microsoft.Extensions.Diagnostics.HealthChecks;
using Mystira.Admin.Api.Services;

namespace Mystira.Admin.Api.Adapters;

public class HealthCheckServiceAdapter : IHealthCheckService
{
    private readonly HealthCheckService _healthCheckService;

    public HealthCheckServiceAdapter(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    public Task<HealthReport> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        return _healthCheckService.CheckHealthAsync(cancellationToken);
    }
}
