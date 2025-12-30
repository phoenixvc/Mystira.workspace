using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.AgeGroups.Queries;

/// <summary>
/// Wolverine handler for retrieving all age groups.
/// </summary>
public static class GetAllAgeGroupsQueryHandler
{
    public static async Task<List<AgeGroupDefinition>> Handle(
        GetAllAgeGroupsQuery query,
        IAgeGroupRepository repository,
        ILogger logger,
        CancellationToken ct)
    {
        logger.LogInformation("Retrieving all age groups");
        var ageGroups = await repository.GetAllAsync();
        logger.LogInformation("Retrieved {Count} age groups", ageGroups.Count);
        return ageGroups;
    }
}
