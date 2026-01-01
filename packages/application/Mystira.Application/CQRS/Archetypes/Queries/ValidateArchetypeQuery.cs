namespace Mystira.Application.CQRS.Archetypes.Queries;

/// <summary>
/// Query to validate if an archetype name exists.
/// </summary>
/// <param name="Name">The name of the archetype to validate.</param>
public record ValidateArchetypeQuery(string Name) : IQuery<bool>;
