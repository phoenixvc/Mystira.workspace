using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Archetypes.Commands;

/// <summary>
/// Command to create a new archetype.
/// </summary>
/// <param name="Name">The name of the archetype.</param>
/// <param name="Description">The description of the archetype.</param>
public record CreateArchetypeCommand(string Name, string Description) : ICommand<ArchetypeDefinition>;
