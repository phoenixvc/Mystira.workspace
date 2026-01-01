using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.GameSessions.Commands;

/// <summary>
/// Command to finalize a completed game session and award badges to profiles.
/// </summary>
/// <param name="SessionId">The unique identifier of the game session to finalize.</param>
public record FinalizeGameSessionCommand(string SessionId) : ICommand<FinalizeGameSessionResult>;

/// <summary>
/// Result of finalizing a game session, containing session ID and badge awards for all profiles.
/// </summary>
public class FinalizeGameSessionResult
{
    /// <summary>
    /// Gets or sets the unique identifier of the finalized game session.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of badge awards for each profile in the session.
    /// </summary>
    public List<ProfileBadgeAwards> Awards { get; set; } = new();
}

/// <summary>
/// Contains badge awards information for a specific profile.
/// </summary>
public class ProfileBadgeAwards
{
    /// <summary>
    /// Gets or sets the unique identifier of the user profile.
    /// </summary>
    public string ProfileId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the user profile.
    /// </summary>
    public string? ProfileName { get; set; }

    /// <summary>
    /// Gets or sets the list of newly awarded badges for this profile.
    /// </summary>
    public List<UserBadge> NewBadges { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether this profile has already played this scenario.
    /// </summary>
    public bool AlreadyPlayed { get; set; } = false;
}
