namespace Mystira.Application.CQRS.CompassAxes.Queries;

/// <summary>
/// Query to validate if a compass axis name exists.
/// </summary>
/// <param name="Name">The name of the compass axis to validate.</param>
public record ValidateCompassAxisQuery(string Name) : IQuery<bool>;
