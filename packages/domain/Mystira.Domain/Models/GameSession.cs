using Mystira.Domain.Entities;
using Mystira.Domain.Enums;

namespace Mystira.Domain.Models;

/// <summary>
/// Represents an active or completed game session.
/// </summary>
public class GameSession : SoftDeletableEntity
{
    /// <summary>
    /// Gets or sets the scenario ID being played.
    /// </summary>
    public string ScenarioId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account ID that owns this session.
    /// </summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user profile ID for this session.
    /// </summary>
    public string ProfileId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the host player's profile ID.
    /// </summary>
    public string HostPlayerId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the session status.
    /// </summary>
    public SessionStatus Status { get; set; } = SessionStatus.Creating;

    /// <summary>
    /// Gets or sets the current scene ID.
    /// </summary>
    public string? CurrentSceneId { get; set; }

    /// <summary>
    /// Gets or sets the session code for joining.
    /// </summary>
    public string? JoinCode { get; set; }

    /// <summary>
    /// Gets or sets when the session started.
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Gets or sets when the session ended.
    /// </summary>
    public DateTime? EndedAt { get; set; }

    /// <summary>
    /// Gets or sets the start time (alias for StartedAt for DTO compatibility).
    /// </summary>
    public DateTime? StartTime
    {
        get => StartedAt;
        set => StartedAt = value;
    }

    /// <summary>
    /// Gets or sets the end time (alias for EndedAt for DTO compatibility).
    /// </summary>
    public DateTime? EndTime
    {
        get => EndedAt;
        set => EndedAt = value;
    }

    /// <summary>
    /// Gets or sets the elapsed time in the session.
    /// </summary>
    public TimeSpan? ElapsedTime { get; set; }

    /// <summary>
    /// Gets or sets the player names in this session.
    /// </summary>
    public List<string> PlayerNames { get; set; } = new();

    /// <summary>
    /// Gets or sets the target age group name.
    /// </summary>
    public string? TargetAgeGroupName { get; set; }

    /// <summary>
    /// Gets or sets the scene count.
    /// </summary>
    public int SceneCount { get; set; }

    /// <summary>
    /// Gets or sets the compass tracking values for this session.
    /// </summary>
    public List<CompassTracking> CompassValues { get; set; } = new();

    /// <summary>
    /// Gets or sets the choice history for this session.
    /// </summary>
    public List<SessionChoice> ChoiceHistory { get; set; } = new();

    /// <summary>
    /// Gets or sets the echo history for this session.
    /// </summary>
    public List<EchoLog> EchoHistory { get; set; } = new();

    /// <summary>
    /// Gets or sets the player compass progress totals.
    /// </summary>
    public Dictionary<string, int> PlayerCompassProgressTotals { get; set; } = new();

    /// <summary>
    /// Gets or sets whether the session is paused.
    /// </summary>
    public bool IsPaused { get; set; }

    /// <summary>
    /// Gets or sets when the session was paused.
    /// </summary>
    public DateTime? PausedAt { get; set; }

    /// <summary>
    /// Gets or sets the target age group for this session.
    /// </summary>
    public string? TargetAgeGroup { get; set; }

    /// <summary>
    /// Gets or sets the selected character ID.
    /// </summary>
    public string? SelectedCharacterId { get; set; }

    /// <summary>
    /// Gets or sets the end reason.
    /// </summary>
    public SessionEndReason? EndReason { get; set; }

    /// <summary>
    /// Gets or sets the total duration in seconds.
    /// </summary>
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// Gets or sets the current scene index.
    /// </summary>
    public int CurrentSceneIndex { get; set; }

    /// <summary>
    /// Gets or sets the total scenes visited.
    /// </summary>
    public int ScenesVisited { get; set; }

    /// <summary>
    /// Gets or sets the last activity time.
    /// </summary>
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets whether the session is public.
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed players.
    /// </summary>
    public int MaxPlayers { get; set; } = 1;

    /// <summary>
    /// Gets or sets the game state as JSON.
    /// </summary>
    public string? GameStateJson { get; set; }

    /// <summary>
    /// Gets or sets the final score.
    /// </summary>
    public int? FinalScore { get; set; }

    /// <summary>
    /// Gets or sets the ending achieved.
    /// </summary>
    public string? EndingAchieved { get; set; }

    /// <summary>
    /// Gets or sets the player assignments.
    /// </summary>
    public virtual ICollection<SessionPlayerAssignment> PlayerAssignments { get; set; } = new List<SessionPlayerAssignment>();

    /// <summary>
    /// Gets or sets the character assignments.
    /// </summary>
    public virtual ICollection<SessionCharacterAssignment> CharacterAssignments { get; set; } = new List<SessionCharacterAssignment>();

    /// <summary>
    /// Gets or sets the choices made.
    /// </summary>
    public virtual ICollection<SessionChoice> Choices { get; set; } = new List<SessionChoice>();

    /// <summary>
    /// Gets or sets the achievements earned.
    /// </summary>
    public virtual ICollection<SessionAchievement> Achievements { get; set; } = new List<SessionAchievement>();

    /// <summary>
    /// Navigation to the scenario.
    /// </summary>
    public virtual Scenario? Scenario { get; set; }

    /// <summary>
    /// Gets whether the session is active.
    /// </summary>
    public bool IsActive => Status == SessionStatus.Active || Status == SessionStatus.Paused;

    /// <summary>
    /// Gets the duration as a TimeSpan.
    /// </summary>
    public TimeSpan? Duration => DurationSeconds.HasValue
        ? TimeSpan.FromSeconds(DurationSeconds.Value)
        : null;

    /// <summary>
    /// Starts the session.
    /// </summary>
    public void Start()
    {
        if (Status == SessionStatus.Creating || Status == SessionStatus.Pending)
        {
            Status = SessionStatus.Active;
            StartedAt = DateTime.UtcNow;
            LastActivityAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Pauses the session.
    /// </summary>
    public void Pause()
    {
        if (Status == SessionStatus.Active)
        {
            Status = SessionStatus.Paused;
            LastActivityAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Resumes the session.
    /// </summary>
    public void Resume()
    {
        if (Status == SessionStatus.Paused)
        {
            Status = SessionStatus.Active;
            LastActivityAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Completes the session.
    /// </summary>
    /// <param name="endingId">The ending achieved.</param>
    /// <param name="score">The final score.</param>
    public void Complete(string? endingId = null, int? score = null)
    {
        if (IsActive)
        {
            Status = SessionStatus.Completed;
            EndedAt = DateTime.UtcNow;
            EndReason = SessionEndReason.Completed;
            EndingAchieved = endingId;
            FinalScore = score;

            if (StartedAt.HasValue)
            {
                DurationSeconds = (int)(EndedAt.Value - StartedAt.Value).TotalSeconds;
            }
        }
    }

    /// <summary>
    /// Abandons the session.
    /// </summary>
    /// <param name="reason">The abandonment reason.</param>
    public void Abandon(SessionEndReason reason = SessionEndReason.PlayerQuit)
    {
        if (IsActive)
        {
            Status = SessionStatus.Abandoned;
            EndedAt = DateTime.UtcNow;
            EndReason = reason;

            if (StartedAt.HasValue)
            {
                DurationSeconds = (int)(EndedAt.Value - StartedAt.Value).TotalSeconds;
            }
        }
    }

    /// <summary>
    /// Records activity to prevent timeout.
    /// </summary>
    public void RecordActivity()
    {
        LastActivityAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Progresses to a new scene.
    /// </summary>
    /// <param name="sceneId">The new scene ID.</param>
    public void ProgressToScene(string sceneId)
    {
        CurrentSceneId = sceneId;
        CurrentSceneIndex++;
        ScenesVisited++;
        LastActivityAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the total elapsed time for this session.
    /// </summary>
    /// <returns>The total elapsed time.</returns>
    public TimeSpan GetTotalElapsedTime()
    {
        if (ElapsedTime.HasValue)
            return ElapsedTime.Value;

        if (StartedAt.HasValue)
        {
            var endTime = EndedAt ?? DateTime.UtcNow;
            return endTime - StartedAt.Value;
        }

        return TimeSpan.Zero;
    }

    /// <summary>
    /// Recalculates compass progress from the choice history.
    /// </summary>
    public void RecalculateCompassProgressFromHistory()
    {
        PlayerCompassProgressTotals.Clear();

        foreach (var choice in ChoiceHistory)
        {
            if (choice.CompassChange != null)
            {
                var axis = choice.CompassChange.Axis;
                var delta = choice.CompassChange.Delta;

                if (!PlayerCompassProgressTotals.ContainsKey(axis))
                    PlayerCompassProgressTotals[axis] = 0;

                PlayerCompassProgressTotals[axis] += delta;
            }
        }
    }
}

/// <summary>
/// Represents a player's assignment to a session.
/// </summary>
public class SessionPlayerAssignment : Entity
{
    /// <summary>
    /// Gets or sets the session ID.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the player's profile ID.
    /// </summary>
    public string PlayerId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the player type (Profile or Guest).
    /// </summary>
    public PlayerType Type { get; set; } = PlayerType.Profile;

    /// <summary>
    /// Gets or sets the profile ID.
    /// </summary>
    public string? ProfileId { get; set; }

    /// <summary>
    /// Gets or sets the profile name.
    /// </summary>
    public string? ProfileName { get; set; }

    /// <summary>
    /// Gets or sets the profile image.
    /// </summary>
    public string? ProfileImage { get; set; }

    /// <summary>
    /// Gets or sets the selected avatar media ID.
    /// </summary>
    public string? SelectedAvatarMediaId { get; set; }

    /// <summary>
    /// Gets or sets the guest name (for guest players).
    /// </summary>
    public string? GuestName { get; set; }

    /// <summary>
    /// Gets or sets the guest age range.
    /// </summary>
    public string? GuestAgeRange { get; set; }

    /// <summary>
    /// Gets or sets the guest avatar.
    /// </summary>
    public string? GuestAvatar { get; set; }

    /// <summary>
    /// Gets or sets whether to save this guest as a profile.
    /// </summary>
    public bool SaveAsProfile { get; set; }

    /// <summary>
    /// Gets or sets the player's display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this player is the host.
    /// </summary>
    public bool IsHost { get; set; }

    /// <summary>
    /// Gets or sets whether the player is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets when the player joined.
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the player left (if applicable).
    /// </summary>
    public DateTime? LeftAt { get; set; }

    /// <summary>
    /// Gets or sets the player's last activity time.
    /// </summary>
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation to the user profile.
    /// </summary>
    public virtual UserProfile? Player { get; set; }
}

/// <summary>
/// Represents a character assignment in a session.
/// </summary>
public class SessionCharacterAssignment : Entity
{
    /// <summary>
    /// Gets or sets the session ID.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scenario character ID.
    /// </summary>
    public string CharacterId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character name.
    /// </summary>
    public string CharacterName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character image.
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// Gets or sets the character audio.
    /// </summary>
    public string? Audio { get; set; }

    /// <summary>
    /// Gets or sets the character role.
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Gets or sets the character archetype.
    /// </summary>
    public string? Archetype { get; set; }

    /// <summary>
    /// Gets or sets whether this character is unused.
    /// </summary>
    public bool IsUnused { get; set; }

    /// <summary>
    /// Gets or sets the player assignment for this character.
    /// </summary>
    public SessionPlayerAssignment? PlayerAssignment { get; set; }

    /// <summary>
    /// Gets or sets the assigned player ID (null for AI).
    /// </summary>
    public string? PlayerId { get; set; }

    /// <summary>
    /// Gets or sets the character's display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this is AI-controlled.
    /// </summary>
    public bool IsAiControlled { get; set; }

    /// <summary>
    /// Gets or sets the compass tracking state as JSON.
    /// </summary>
    public string? CompassTrackingJson { get; set; }

    /// <summary>
    /// Gets or sets the current scene ID for this character.
    /// </summary>
    public string? CurrentSceneId { get; set; }

    /// <summary>
    /// Navigation to the scenario character.
    /// </summary>
    public virtual ScenarioCharacter? Character { get; set; }

    /// <summary>
    /// Navigation to the player profile.
    /// </summary>
    public virtual UserProfile? Player { get; set; }
}

/// <summary>
/// Represents a choice made during a session.
/// </summary>
public class SessionChoice : Entity
{
    /// <summary>
    /// Gets or sets the session ID.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scene ID where the choice was made.
    /// </summary>
    public string SceneId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scene title.
    /// </summary>
    public string? SceneTitle { get; set; }

    /// <summary>
    /// Gets or sets the branch ID that was chosen.
    /// </summary>
    public string BranchId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the choice text.
    /// </summary>
    public string? ChoiceText { get; set; }

    /// <summary>
    /// Gets or sets the next scene ID.
    /// </summary>
    public string? NextScene { get; set; }

    /// <summary>
    /// Gets or sets the compass axis affected.
    /// </summary>
    public string? CompassAxis { get; set; }

    /// <summary>
    /// Gets or sets the compass direction.
    /// </summary>
    public string? CompassDirection { get; set; }

    /// <summary>
    /// Gets or sets the compass delta value.
    /// </summary>
    public double CompassDelta { get; set; }

    /// <summary>
    /// Gets or sets the compass change for this choice.
    /// </summary>
    public CompassChange? CompassChange { get; set; }

    /// <summary>
    /// Gets or sets whether an echo was generated.
    /// </summary>
    public bool EchoGenerated { get; set; }

    /// <summary>
    /// Gets or sets the player who made the choice.
    /// </summary>
    public string? PlayerId { get; set; }

    /// <summary>
    /// Gets or sets when the choice was made.
    /// </summary>
    public DateTime ChosenAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the time spent on this scene in seconds.
    /// </summary>
    public int? TimeSpentSeconds { get; set; }

    /// <summary>
    /// Gets or sets compass changes from this choice as JSON.
    /// </summary>
    public string? CompassChangesJson { get; set; }

    /// <summary>
    /// Gets or sets any echoes revealed by this choice.
    /// </summary>
    public string? RevealedEchosJson { get; set; }
}

/// <summary>
/// Represents an achievement earned during a session.
/// </summary>
public class SessionAchievement : Entity
{
    /// <summary>
    /// Gets or sets the session ID.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the achievement type.
    /// </summary>
    public AchievementType Type { get; set; }

    /// <summary>
    /// Gets or sets the achievement title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the achievement description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the player who earned the achievement.
    /// </summary>
    public string? PlayerId { get; set; }

    /// <summary>
    /// Gets or sets when the achievement was earned.
    /// </summary>
    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the scene where it was earned.
    /// </summary>
    public string? SceneId { get; set; }

    /// <summary>
    /// Gets or sets the points awarded.
    /// </summary>
    public int PointsAwarded { get; set; }

    /// <summary>
    /// Gets or sets an optional badge ID associated with this achievement.
    /// </summary>
    public string? BadgeId { get; set; }

    /// <summary>
    /// Gets or sets the icon name for this achievement.
    /// </summary>
    public string? IconName { get; set; }

    /// <summary>
    /// Gets or sets the compass axis related to this achievement.
    /// </summary>
    public string? CompassAxis { get; set; }

    /// <summary>
    /// Gets or sets the threshold value for threshold-based achievements.
    /// </summary>
    public int? ThresholdValue { get; set; }

    /// <summary>
    /// Gets or sets additional metadata as JSON.
    /// </summary>
    public string? MetadataJson { get; set; }
}

