using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Archetypes.Commands;

/// <summary>
/// Command to update an existing archetype.
/// </summary>
/// <param name="Id">The unique identifier of the archetype to update.</param>
/// <param name="Name">The new name of the archetype.</param>
/// <param name="Description">The new description of the archetype.</param>
public record UpdateArchetypeCommand(string Id, string Name, string Description) : ICommand<ArchetypeDefinition?>;
