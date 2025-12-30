namespace Mystira.Domain.Enums;

/// <summary>
/// Represents the role of a contributor.
/// </summary>
public enum ContributorRole
{
    /// <summary>
    /// Primary author/creator.
    /// </summary>
    Author = 0,

    /// <summary>
    /// Visual artist or illustrator.
    /// </summary>
    Artist = 1,

    /// <summary>
    /// Content editor.
    /// </summary>
    Editor = 2,

    /// <summary>
    /// Narrative writer.
    /// </summary>
    Writer = 3,

    /// <summary>
    /// Game or experience designer.
    /// </summary>
    Designer = 4,

    /// <summary>
    /// Music composer.
    /// </summary>
    Composer = 5,

    /// <summary>
    /// Voice actor.
    /// </summary>
    VoiceActor = 6,

    /// <summary>
    /// Language translator.
    /// </summary>
    Translator = 7,

    /// <summary>
    /// Other role not specified above.
    /// </summary>
    Other = 99
}

/// <summary>
/// Represents the verification status of a contributor.
/// </summary>
public enum ContributorVerificationStatus
{
    /// <summary>
    /// Verification pending review.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Contributor is verified.
    /// </summary>
    Verified = 1,

    /// <summary>
    /// Verification was rejected.
    /// </summary>
    Rejected = 2,

    /// <summary>
    /// Verification has expired.
    /// </summary>
    Expired = 3
}
