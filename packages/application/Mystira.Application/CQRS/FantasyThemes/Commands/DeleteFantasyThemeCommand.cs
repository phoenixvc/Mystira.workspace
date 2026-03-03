namespace Mystira.Application.CQRS.FantasyThemes.Commands;

/// <summary>
/// Command to delete a fantasy theme.
/// </summary>
/// <param name="Id">The unique identifier of the fantasy theme to delete.</param>
public record DeleteFantasyThemeCommand(string Id) : ICommand<bool>;
