using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.AgeGroups.Queries;

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
