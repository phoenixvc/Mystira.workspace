using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.FantasyThemes.Commands;

/// <summary>
/// Command to create a new fantasy theme.
/// </summary>
/// <param name="Name">The name of the fantasy theme.</param>
/// <param name="Description">The description of the fantasy theme.</param>
public record CreateFantasyThemeCommand(string Name, string Description) : ICommand<FantasyThemeDefinition>;
