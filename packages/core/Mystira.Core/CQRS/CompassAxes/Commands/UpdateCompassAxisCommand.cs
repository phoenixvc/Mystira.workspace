using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.CompassAxes.Commands;

/// <summary>
/// Command to update an existing compass axis.
/// </summary>
public record UpdateCompassAxisCommand(string Id, string Name, string Description) : ICommand<CompassAxis?>;
