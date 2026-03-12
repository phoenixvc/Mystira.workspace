namespace Mystira.Core.CQRS.EchoTypes.Queries;

/// <summary>
/// Query to validate if an echo type name exists.
/// </summary>
/// <param name="Name">The name of the echo type to validate.</param>
public record ValidateEchoTypeQuery(string Name) : IQuery<bool>;
