namespace Mystira.Application.CQRS.Archetypes.Commands;

/// <summary>
/// Command to delete an archetype.
/// </summary>
/// <param name="Id">The unique identifier of the archetype to delete.</param>
public record DeleteArchetypeCommand(string Id) : ICommand<bool>;
