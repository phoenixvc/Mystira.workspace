using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.Characters.Queries;

/// <summary>
/// Query to get a specific character by ID from the character map.
/// </summary>
public record GetCharacterQuery(string CharacterId) : IQuery<Character?>;
