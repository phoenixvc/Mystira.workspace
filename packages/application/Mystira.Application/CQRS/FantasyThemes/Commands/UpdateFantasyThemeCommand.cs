using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.FantasyThemes.Commands;

/// <summary>
/// Command to update an existing fantasy theme.
/// </summary>
public record UpdateFantasyThemeCommand(string Id, string Name, string Description) : ICommand<FantasyThemeDefinition?>;
