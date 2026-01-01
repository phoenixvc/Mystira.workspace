namespace Mystira.Application.CQRS.CompassAxes.Commands;

/// <summary>
/// Command to delete a compass axis.
/// </summary>
/// <param name="Id">The unique identifier of the compass axis to delete.</param>
public record DeleteCompassAxisCommand(string Id) : ICommand<bool>;
