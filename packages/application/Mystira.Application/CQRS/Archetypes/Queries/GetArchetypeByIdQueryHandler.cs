using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Archetypes.Queries;

/// <summary>
/// Wolverine handler for retrieving an archetype by ID.
/// </summary>
public static class GetArchetypeByIdQueryHandler
{
    /// <summary>
    /// Handles the GetArchetypeByIdQuery.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="repository">The archetype repository.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The archetype definition if found; otherwise, null.</returns>
    public static async Task<ArchetypeDefinition?> Handle(
        GetArchetypeByIdQuery query,
        IArchetypeRepository repository,
        ILogger logger,
        CancellationToken ct)
    {
        logger.LogInformation("Retrieving archetype with id: {Id}", query.Id);
        var archetype = await repository.GetByIdAsync(query.Id);

        if (archetype == null)
        {
            logger.LogWarning("Archetype with id {Id} not found", query.Id);
        }

        return archetype;
    }
}
