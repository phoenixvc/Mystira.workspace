using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Characters.Queries;

/// <summary>
/// Query to get a specific character by ID from the character map.
/// </summary>
/// <param name="CharacterId">The unique identifier of the character.</param>
public record GetCharacterQuery(string CharacterId) : IQuery<Character?>;
