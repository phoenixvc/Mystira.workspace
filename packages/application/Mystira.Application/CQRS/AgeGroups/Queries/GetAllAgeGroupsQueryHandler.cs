using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.AgeGroups.Queries;

/// <summary>
/// Wolverine handler for retrieving all age groups.
/// </summary>
public static class GetAllAgeGroupsQueryHandler
{
    /// <summary>
    /// Handles the GetAllAgeGroupsQuery.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="repository">The age group repository.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A list of all age group definitions.</returns>
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
