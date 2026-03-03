using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.CharacterMaps.Queries;

/// <summary>
/// Wolverine handler for retrieving a specific character map by ID.
/// </summary>
public static class GetCharacterMapQueryHandler
{
    /// <summary>
    /// Handles the GetCharacterMapQuery.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="repository">The character map repository.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The character map if found; otherwise, null.</returns>
    public static async Task<CharacterMap?> Handle(
        GetCharacterMapQuery query,
        ICharacterMapRepository repository,
        ILogger<GetCharacterMapQuery> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Retrieving character map {CharacterMapId}", query.Id);

        var characterMap = await repository.GetByIdAsync(query.Id);

        if (characterMap == null)
        {
            logger.LogWarning("Character map not found: {CharacterMapId}", query.Id);
        }

        return characterMap;
    }
}
