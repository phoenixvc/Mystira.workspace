namespace Mystira.Application.CQRS.FantasyThemes.Queries;

/// <summary>
/// Query to validate if a fantasy theme name exists.
/// </summary>
/// <param name="Name">The name of the fantasy theme to validate.</param>
public record ValidateFantasyThemeQuery(string Name) : IQuery<bool>;
