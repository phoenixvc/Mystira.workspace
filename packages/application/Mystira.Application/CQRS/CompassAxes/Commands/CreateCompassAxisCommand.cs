using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.CompassAxes.Commands;

/// <summary>
/// Command to create a new compass axis.
/// </summary>
/// <param name="Name">The name of the compass axis.</param>
/// <param name="Description">The description of the compass axis.</param>
public record CreateCompassAxisCommand(string Name, string Description) : ICommand<CompassAxis>;
