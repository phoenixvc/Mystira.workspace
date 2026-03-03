using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Archetypes.Queries;

/// <summary>
/// Wolverine handler for retrieving all archetypes.
/// </summary>
public static class GetAllArchetypesQueryHandler
{
    /// <summary>
    /// Handles the GetAllArchetypesQuery.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="repository">The archetype repository.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A list of all archetype definitions.</returns>
    public static async Task<List<ArchetypeDefinition>> Handle(
        GetAllArchetypesQuery query,
        IArchetypeRepository repository,
        ILogger logger,
        CancellationToken ct)
    {
        logger.LogInformation("Retrieving all archetypes");
        var archetypes = await repository.GetAllAsync();
        logger.LogInformation("Retrieved {Count} archetypes", archetypes.Count);
        return archetypes;
    }
}
