using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.Archetypes.Commands;

/// <summary>
/// Command to update an existing archetype.
/// </summary>
public record UpdateArchetypeCommand(string Id, string Name, string Description) : ICommand<ArchetypeDefinition?>;
