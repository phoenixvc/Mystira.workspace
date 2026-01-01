using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.FantasyThemes.Commands;

/// <summary>
/// Command to update an existing fantasy theme.
/// </summary>
/// <param name="Id">The unique identifier of the fantasy theme to update.</param>
/// <param name="Name">The new name of the fantasy theme.</param>
/// <param name="Description">The new description of the fantasy theme.</param>
public record UpdateFantasyThemeCommand(string Id, string Name, string Description) : ICommand<FantasyThemeDefinition?>;
