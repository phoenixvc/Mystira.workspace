namespace Mystira.Contracts.App.Enums;

/// <summary>
/// Represents the role of a contributor in content creation.
/// </summary>
public enum ContributorRole
{
    /// <summary>
    /// The primary author of the content.
    /// </summary>
    Author = 0,

    /// <summary>
    /// An artist who created visual assets.
    /// </summary>
    Artist = 1,

    /// <summary>
    /// An editor who reviewed and refined the content.
    /// </summary>
    Editor = 2,

    /// <summary>
    /// A writer who contributed to the narrative.
    /// </summary>
    Writer = 3,

    /// <summary>
    /// A designer who created game mechanics or layouts.
    /// </summary>
    Designer = 4,

    /// <summary>
    /// A composer who created musical content.
    /// </summary>
    Composer = 5,

    /// <summary>
    /// A voice actor who provided voice recordings.
    /// </summary>
    VoiceActor = 6,

    /// <summary>
    /// A translator who localized the content.
    /// </summary>
    Translator = 7,

    /// <summary>
    /// Other contributor role not specified above.
    /// </summary>
    Other = 99
}
