using Mystira.Domain.Entities;
using Mystira.Domain.ValueObjects;

namespace Mystira.Domain.Models;

/// <summary>
/// Represents a user's profile information.
/// </summary>
public class UserProfile : Entity
{
    /// <summary>
    /// Gets or sets the associated account ID.
    /// </summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the user's last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the user's date of birth.
    /// </summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// Gets or sets the user's age group.
    /// </summary>
    public string? AgeGroupId { get; set; }

    /// <summary>
    /// Gets or sets the avatar image URL.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Gets or sets the selected avatar configuration ID.
    /// </summary>
    public string? AvatarConfigurationId { get; set; }

    /// <summary>
    /// Gets or sets the user's bio.
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Gets or sets the user's preferred language code.
    /// </summary>
    public string PreferredLanguage { get; set; } = "en";

    /// <summary>
    /// Gets or sets the user's timezone.
    /// </summary>
    public string? Timezone { get; set; }

    /// <summary>
    /// Gets or sets whether the profile is public.
    /// </summary>
    public bool IsPublic { get; set; } = true;

    /// <summary>
    /// Gets or sets whether parental controls are enabled.
    /// </summary>
    public bool ParentalControlsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the parental control PIN hash.
    /// </summary>
    public string? ParentalControlPinHash { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed session length in minutes.
    /// </summary>
    public int? MaxSessionLengthMinutes { get; set; }

    /// <summary>
    /// Gets or sets the total play time in minutes.
    /// </summary>
    public int TotalPlayTimeMinutes { get; set; }

    /// <summary>
    /// Gets or sets the number of completed scenarios.
    /// </summary>
    public int CompletedScenarios { get; set; }

    /// <summary>
    /// Gets or sets the user's experience points.
    /// </summary>
    public long ExperiencePoints { get; set; }

    /// <summary>
    /// Gets or sets the user's current level.
    /// </summary>
    public int Level { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether the user has completed onboarding.
    /// </summary>
    public bool OnboardingCompleted { get; set; }

    /// <summary>
    /// Gets or sets when the user last played.
    /// </summary>
    public DateTime? LastPlayedAt { get; set; }

    /// <summary>
    /// Gets or sets the compass progress as JSON.
    /// </summary>
    public string? CompassProgressJson { get; set; }

    /// <summary>
    /// Navigation property to the account.
    /// </summary>
    public virtual Account? Account { get; set; }

    /// <summary>
    /// Gets the user's age group based on AgeGroupId.
    /// </summary>
    public AgeGroup? AgeGroup => AgeGroup.FromId(AgeGroupId);

    /// <summary>
    /// Gets the user's full name.
    /// </summary>
    public string? FullName =>
        string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName)
            ? null
            : $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Calculates the user's age if date of birth is set.
    /// </summary>
    public int? Age
    {
        get
        {
            if (!DateOfBirth.HasValue)
                return null;

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - DateOfBirth.Value.Year;

            if (DateOfBirth.Value > today.AddYears(-age))
                age--;

            return age;
        }
    }

    /// <summary>
    /// Updates the age group based on the current age.
    /// </summary>
    public void UpdateAgeGroup()
    {
        if (Age.HasValue)
        {
            AgeGroupId = ValueObjects.AgeGroup.ForAge(Age.Value).Id;
        }
    }

    /// <summary>
    /// Adds experience points and updates level if needed.
    /// </summary>
    /// <param name="points">The points to add.</param>
    /// <returns>True if leveled up.</returns>
    public bool AddExperience(long points)
    {
        if (points <= 0)
            return false;

        ExperiencePoints += points;

        var newLevel = CalculateLevel(ExperiencePoints);
        if (newLevel > Level)
        {
            Level = newLevel;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Calculates level based on XP (simple formula: level = 1 + sqrt(xp / 100)).
    /// </summary>
    private static int CalculateLevel(long xp)
    {
        return 1 + (int)Math.Sqrt(xp / 100.0);
    }
}
