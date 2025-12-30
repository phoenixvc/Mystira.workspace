// Re-export domain enums for backward compatibility
global using DomainContributorRole = Mystira.Domain.Enums.ContributorRole;
global using DomainContributorVerificationStatus = Mystira.Domain.Enums.ContributorVerificationStatus;

namespace Mystira.Contracts.App.Enums;

/// <summary>
/// Represents the role of a contributor in content creation.
/// </summary>
/// <remarks>
/// This type is re-exported from Mystira.Domain.Enums for backward compatibility.
/// New code should use Mystira.Domain.Enums.ContributorRole directly.
/// </remarks>
public enum ContributorRole
{
    /// <inheritdoc cref="DomainContributorRole.Author"/>
    Author = 0,

    /// <inheritdoc cref="DomainContributorRole.Artist"/>
    Artist = 1,

    /// <inheritdoc cref="DomainContributorRole.Editor"/>
    Editor = 2,

    /// <inheritdoc cref="DomainContributorRole.Writer"/>
    Writer = 3,

    /// <inheritdoc cref="DomainContributorRole.Designer"/>
    Designer = 4,

    /// <inheritdoc cref="DomainContributorRole.Composer"/>
    Composer = 5,

    /// <inheritdoc cref="DomainContributorRole.VoiceActor"/>
    VoiceActor = 6,

    /// <inheritdoc cref="DomainContributorRole.Translator"/>
    Translator = 7,

    /// <inheritdoc cref="DomainContributorRole.Other"/>
    Other = 99
}

/// <summary>
/// Represents the verification status of a contributor.
/// </summary>
/// <remarks>
/// This type is re-exported from Mystira.Domain.Enums for backward compatibility.
/// New code should use Mystira.Domain.Enums.ContributorVerificationStatus directly.
/// </remarks>
public enum ContributorVerificationStatus
{
    /// <inheritdoc cref="DomainContributorVerificationStatus.Pending"/>
    Pending = 0,

    /// <inheritdoc cref="DomainContributorVerificationStatus.Verified"/>
    Verified = 1,

    /// <inheritdoc cref="DomainContributorVerificationStatus.Rejected"/>
    Rejected = 2,

    /// <inheritdoc cref="DomainContributorVerificationStatus.Expired"/>
    Expired = 3
}

/// <summary>
/// Extension methods for contributor enum conversions between Contracts and Domain.
/// </summary>
public static class ContributorEnumExtensions
{
    /// <summary>
    /// Converts Contracts ContributorRole to Domain ContributorRole.
    /// </summary>
    public static DomainContributorRole ToDomain(this ContributorRole value)
        => (DomainContributorRole)(int)value;

    /// <summary>
    /// Converts Domain ContributorRole to Contracts ContributorRole.
    /// </summary>
    public static ContributorRole ToContracts(this DomainContributorRole value)
        => (ContributorRole)(int)value;

    /// <summary>
    /// Converts Contracts ContributorVerificationStatus to Domain ContributorVerificationStatus.
    /// </summary>
    public static DomainContributorVerificationStatus ToDomain(this ContributorVerificationStatus value)
        => (DomainContributorVerificationStatus)(int)value;

    /// <summary>
    /// Converts Domain ContributorVerificationStatus to Contracts ContributorVerificationStatus.
    /// </summary>
    public static ContributorVerificationStatus ToContracts(this DomainContributorVerificationStatus value)
        => (ContributorVerificationStatus)(int)value;
}
