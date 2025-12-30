// Re-export domain enums for backward compatibility
global using DomainContributorRole = Mystira.Domain.Enums.ContributorRole;
global using DomainContributorVerificationStatus = Mystira.Domain.Enums.ContributorVerificationStatus;

namespace Mystira.Contracts.App.Enums;

/// <summary>
/// Represents the role of a contributor in content creation.
/// </summary>
/// <remarks>
/// This type mirrors Mystira.Domain.Enums.ContributorRole for backward compatibility.
/// </remarks>
public enum ContributorRole
{
    /// <summary>Primary author/creator.</summary>
    Author = 0,

    /// <summary>Visual artist or illustrator.</summary>
    Artist = 1,

    /// <summary>Content editor.</summary>
    Editor = 2,

    /// <summary>Narrative writer.</summary>
    Writer = 3,

    /// <summary>Game or experience designer.</summary>
    Designer = 4,

    /// <summary>Music composer.</summary>
    Composer = 5,

    /// <summary>Voice actor.</summary>
    VoiceActor = 6,

    /// <summary>Language translator.</summary>
    Translator = 7,

    /// <summary>Other role not specified above.</summary>
    Other = 99
}

/// <summary>
/// Represents the verification status of a contributor.
/// </summary>
/// <remarks>
/// This type mirrors Mystira.Domain.Enums.ContributorVerificationStatus for backward compatibility.
/// </remarks>
public enum ContributorVerificationStatus
{
    /// <summary>Verification pending review.</summary>
    Pending = 0,

    /// <summary>Contributor is verified.</summary>
    Verified = 1,

    /// <summary>Verification was rejected.</summary>
    Rejected = 2,

    /// <summary>Verification has expired.</summary>
    Expired = 3
}

/// <summary>
/// Extension methods for contributor enum conversions between Contracts and Domain.
/// </summary>
public static class ContributorEnumExtensions
{
    /// <summary>Converts Contracts ContributorRole to Domain ContributorRole.</summary>
    public static DomainContributorRole ToDomain(this ContributorRole value)
        => (DomainContributorRole)(int)value;

    /// <summary>Converts Domain ContributorRole to Contracts ContributorRole.</summary>
    public static ContributorRole ToContracts(this DomainContributorRole value)
        => (ContributorRole)(int)value;

    /// <summary>Converts Contracts ContributorVerificationStatus to Domain.</summary>
    public static DomainContributorVerificationStatus ToDomain(this ContributorVerificationStatus value)
        => (DomainContributorVerificationStatus)(int)value;

    /// <summary>Converts Domain ContributorVerificationStatus to Contracts.</summary>
    public static ContributorVerificationStatus ToContracts(this DomainContributorVerificationStatus value)
        => (ContributorVerificationStatus)(int)value;
}
