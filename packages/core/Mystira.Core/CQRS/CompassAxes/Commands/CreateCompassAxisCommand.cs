using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.CompassAxes.Commands;

/// <summary>
/// Command to create a new compass axis.
/// </summary>
public record CreateCompassAxisCommand(string Name, string Description) : ICommand<CompassAxis>;
