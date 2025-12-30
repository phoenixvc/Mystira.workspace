using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.AgeGroups.Queries;

/// <summary>
/// Wolverine handler for retrieving an age group by ID.
/// </summary>
public static class GetAgeGroupByIdQueryHandler
{
    public static async Task<AgeGroupDefinition?> Handle(
        GetAgeGroupByIdQuery query,
        IAgeGroupRepository repository,
        ILogger logger,
        CancellationToken ct)
    {
        logger.LogInformation("Retrieving age group with id: {Id}", query.Id);
        var ageGroup = await repository.GetByIdAsync(query.Id);

        if (ageGroup == null)
        {
            logger.LogWarning("Age group with id {Id} not found", query.Id);
        }

        return ageGroup;
    }
}
