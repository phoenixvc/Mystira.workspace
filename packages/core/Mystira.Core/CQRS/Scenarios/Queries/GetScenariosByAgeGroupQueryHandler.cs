using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.Scenarios.Queries;

/// <summary>
/// Wolverine handler for GetScenariosByAgeGroupQuery.
/// </summary>
public static class GetScenariosByAgeGroupQueryHandler
{
    /// <summary>
    /// Handles the GetScenariosByAgeGroupQuery by retrieving scenarios filtered by age group from the repository.
    /// Wolverine injects dependencies as method parameters.
    /// </summary>
    public static async Task<IEnumerable<Scenario>> Handle(
        GetScenariosByAgeGroupQuery query,
        IScenarioRepository repository,
        ILogger logger,
        CancellationToken ct)
    {
        // Use domain repository method to query scenarios by age group
        var scenarios = await repository.GetByAgeGroupAsync(query.AgeGroup, ct);

        logger.LogDebug("Retrieved {Count} scenarios for age group: {AgeGroup}",
            scenarios.Count(), query.AgeGroup);

        return scenarios;
    }
}
