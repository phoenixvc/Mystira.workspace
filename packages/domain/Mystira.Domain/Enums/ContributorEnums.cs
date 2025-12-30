namespace Mystira.Domain.Enums;

/// <summary>
/// Represents the role of a contributor.
/// </summary>
public enum ContributorRole
{
    /// <summary>
    /// Primary author/writer.
    /// </summary>
    Author = 0,

    /// <summary>
    /// Illustrator or artist.
    /// </summary>
    Illustrator = 1,

    /// <summary>
    /// Editor.
    /// </summary>
    Editor = 2,

    /// <summary>
    /// Voice actor.
    /// </summary>
    VoiceActor = 3,

    /// <summary>
    /// Music composer.
    /// </summary>
    Composer = 4,

    /// <summary>
    /// Sound designer.
    /// </summary>
    SoundDesigner = 5,

    /// <summary>
    /// Translator.
    /// </summary>
    Translator = 6,

    /// <summary>
    /// Game designer.
    /// </summary>
    GameDesigner = 7,

    /// <summary>
    /// Technical contributor.
    /// </summary>
    Technical = 8,

    /// <summary>
    /// Other role.
    /// </summary>
    Other = 99
}

/// <summary>
/// Represents the verification status of a contributor.
/// </summary>
public enum ContributorVerificationStatus
{
    /// <summary>
    /// Not yet verified.
    /// </summary>
    Unverified = 0,

    /// <summary>
    /// Verification pending review.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Verified contributor.
    /// </summary>
    Verified = 2,

    /// <summary>
    /// Verification rejected.
    /// </summary>
    Rejected = 3
}
