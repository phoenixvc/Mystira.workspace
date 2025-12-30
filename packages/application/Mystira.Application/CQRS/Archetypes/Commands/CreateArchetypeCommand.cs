using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Archetypes.Commands;

/// <summary>
/// Command to create a new archetype.
/// </summary>
public record CreateArchetypeCommand(string Name, string Description) : ICommand<ArchetypeDefinition>;
