namespace Mystira.Contracts.App.Requests.UserProfiles;

public record CreateUserProfileRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public List<string> PreferredFantasyThemes { get; set; } = new();
    public string AgeGroup { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public bool IsGuest { get; set; } = false;
    public bool IsNpc { get; set; } = false;
    public string? AccountId { get; set; }
    public bool HasCompletedOnboarding { get; set; }
    public string? SelectedAvatarMediaId { get; set; }
}

public record UpdateUserProfileRequest
{
    public List<string>? PreferredFantasyThemes { get; set; }
    public string? AgeGroup { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public bool? HasCompletedOnboarding { get; set; }
    public bool? IsGuest { get; set; }
    public bool? IsNpc { get; set; }
    public string? AccountId { get; set; }
    public string? Pronouns { get; set; }
    public string? Bio { get; set; }
    public string? SelectedAvatarMediaId { get; set; }
}

public record CreateMultipleProfilesRequest
{
    public List<CreateUserProfileRequest> Profiles { get; set; } = new();
}
