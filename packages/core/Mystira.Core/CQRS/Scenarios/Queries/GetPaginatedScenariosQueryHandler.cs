using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Core.Specifications;
using Mystira.Domain.Models;

namespace Mystira.Core.CQRS.Scenarios.Queries;

/// <summary>
/// Wolverine handler for GetPaginatedScenariosQuery.
/// Demonstrates how to use Specification Pattern with pagination.
/// </summary>
public static class GetPaginatedScenariosQueryHandler
{
    /// <summary>
    /// Handles the GetPaginatedScenariosQuery by retrieving paginated scenarios from the repository.
    /// Wolverine injects dependencies as method parameters.
    /// </summary>
    public static async Task<IEnumerable<Scenario>> Handle(
        GetPaginatedScenariosQuery query,
        IScenarioRepository repository,
        ILogger logger,
        CancellationToken ct)
    {
        // Create specification for paginated scenarios
        var spec = new ScenariosPaginatedSpec(
            skip: (query.PageNumber - 1) * query.PageSize,
            take: query.PageSize);

        // Use specification to query repository
        var scenarios = await repository.ListAsync(spec);

        logger.LogDebug("Retrieved page {PageNumber} with {Count} scenarios (page size: {PageSize})",
            query.PageNumber, scenarios.Count(), query.PageSize);

        return scenarios;
    }
}
