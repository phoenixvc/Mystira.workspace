using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.CharacterMaps.Queries;

/// <summary>
/// Wolverine handler for retrieving a specific character map by ID.
/// </summary>
public static class GetCharacterMapQueryHandler
{
    public static async Task<CharacterMap?> Handle(
        GetCharacterMapQuery query,
        ICharacterMapRepository repository,
        ILogger<GetCharacterMapQuery> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Retrieving character map {CharacterMapId}", query.Id);

        var characterMap = await repository.GetByIdAsync(query.Id, ct);

        if (characterMap == null)
        {
            logger.LogWarning("Character map not found: {CharacterMapId}", query.Id);
        }

        return characterMap;
    }
}
