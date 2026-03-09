using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Mystira.Admin.Api.Services;

public interface IHealthCheckService
{
    Task<HealthReport> CheckHealthAsync(CancellationToken cancellationToken = default);
}
