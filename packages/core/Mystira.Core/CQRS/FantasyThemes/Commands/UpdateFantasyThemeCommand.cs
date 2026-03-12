using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.FantasyThemes.Commands;

/// <summary>
/// Command to update an existing fantasy theme.
/// </summary>
public record UpdateFantasyThemeCommand(string Id, string Name, string Description) : ICommand<FantasyThemeDefinition?>;
