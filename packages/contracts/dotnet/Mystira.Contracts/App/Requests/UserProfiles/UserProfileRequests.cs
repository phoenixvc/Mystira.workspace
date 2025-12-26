namespace Mystira.Contracts.App.Requests.UserProfiles;

/// <summary>
/// Request to create a new user profile.
/// </summary>
public record CreateUserProfileRequest
{
    /// <summary>
    /// The unique identifier for the profile.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The display name for the profile.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// List of preferred fantasy themes for content recommendations.
    /// </summary>
    public List<string> PreferredFantasyThemes { get; set; } = new();

    /// <summary>
    /// The age group classification for content filtering.
    /// </summary>
    public string AgeGroup { get; set; } = string.Empty;

    /// <summary>
    /// Optional date of birth for age verification.
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Indicates if this is a guest profile.
    /// </summary>
    public bool IsGuest { get; set; } = false;

    /// <summary>
    /// Indicates if this profile represents an NPC.
    /// </summary>
    public bool IsNpc { get; set; } = false;

    /// <summary>
    /// Optional account identifier this profile belongs to.
    /// </summary>
    public string? AccountId { get; set; }

    /// <summary>
    /// Indicates if the user has completed the onboarding process.
    /// </summary>
    public bool HasCompletedOnboarding { get; set; }

    /// <summary>
    /// Optional media identifier for the selected avatar.
    /// </summary>
    public string? SelectedAvatarMediaId { get; set; }

    /// <summary>
    /// Optional pronouns for the user.
    /// </summary>
    public string? Pronouns { get; set; }

    /// <summary>
    /// Optional biography text for the user.
    /// </summary>
    public string? Bio { get; set; }
}

/// <summary>
/// Request to update an existing user profile.
/// </summary>
public record UpdateUserProfileRequest
{
    /// <summary>
    /// Optional updated list of preferred fantasy themes.
    /// </summary>
    public List<string>? PreferredFantasyThemes { get; set; }

    /// <summary>
    /// Optional updated age group classification.
    /// </summary>
    public string? AgeGroup { get; set; }

    /// <summary>
    /// Optional updated date of birth.
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Optional flag to update onboarding completion status.
    /// </summary>
    public bool? HasCompletedOnboarding { get; set; }

    /// <summary>
    /// Optional flag to update guest status.
    /// </summary>
    public bool? IsGuest { get; set; }

    /// <summary>
    /// Optional flag to update NPC status.
    /// </summary>
    public bool? IsNpc { get; set; }

    /// <summary>
    /// Optional updated account identifier.
    /// </summary>
    public string? AccountId { get; set; }

    /// <summary>
    /// Optional updated pronouns for the user.
    /// </summary>
    public string? Pronouns { get; set; }

    /// <summary>
    /// Optional updated biography text.
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Optional updated avatar media identifier.
    /// </summary>
    public string? SelectedAvatarMediaId { get; set; }
}

/// <summary>
/// Request to create multiple user profiles at once.
/// </summary>
public record CreateMultipleProfilesRequest
{
    /// <summary>
    /// The list of profiles to create.
    /// </summary>
    public List<CreateUserProfileRequest> Profiles { get; set; } = new();
}

/// <summary>
/// Request to assign a profile to an account.
/// </summary>
public class ProfileAssignmentRequest
{
    /// <summary>
    /// The unique identifier of the profile.
    /// </summary>
    public string ProfileId { get; set; } = string.Empty;

    /// <summary>
    /// The unique identifier of the account.
    /// </summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// The unique identifier of the character to assign.
    /// </summary>
    public string CharacterId { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether this is an NPC assignment.
    /// </summary>
    public bool IsNpcAssignment { get; set; } = false;
}

/// <summary>
/// Request to create a guest profile for quick play without full registration.
/// </summary>
public record CreateGuestProfileRequest
{
    /// <summary>
    /// The unique identifier for the guest profile.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Display name for the guest. Required for PWA flow, optional for API flow with UseAdjectiveNames.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The age group for content filtering. Primary property name.
    /// </summary>
    public string AgeGroup { get; set; } = string.Empty;

    /// <summary>
    /// Alias for AgeGroup. Used by PWA for backward compatibility.
    /// Setting this also sets AgeGroup.
    /// </summary>
    public string? AgeRange
    {
        get => AgeGroup;
        set => AgeGroup = value ?? string.Empty;
    }

    /// <summary>
    /// Whether to generate a name using adjective-based naming (e.g., "Brave Explorer").
    /// </summary>
    public bool UseAdjectiveNames { get; set; } = false;

    /// <summary>
    /// Optional avatar identifier for the guest.
    /// </summary>
    public string? Avatar { get; set; }

    /// <summary>
    /// Account identifier to link the guest profile to.
    /// </summary>
    public string? AccountId { get; set; }

    /// <summary>
    /// Indicates this is a guest profile. Always true for this request type.
    /// </summary>
    public bool IsGuest { get; set; } = true;
}
