using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.CompassAxes.Commands;

/// <summary>
/// Command to update an existing compass axis.
/// </summary>
/// <param name="Id">The unique identifier of the compass axis to update.</param>
/// <param name="Name">The new name of the compass axis.</param>
/// <param name="Description">The new description of the compass axis.</param>
public record UpdateCompassAxisCommand(string Id, string Name, string Description) : ICommand<CompassAxis?>;
