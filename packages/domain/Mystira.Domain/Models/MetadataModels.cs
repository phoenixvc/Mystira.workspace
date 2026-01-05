using System.Text.Json.Serialization;

namespace Mystira.Domain.Models;

/// <summary>
/// Individual media metadata entry.
/// </summary>
public class MediaMetadataEntry
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the display title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the file name.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Gets or sets the media type (image, audio, video).</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the age rating.</summary>
    public int AgeRating { get; set; }

    /// <summary>Gets or sets the subject reference ID.</summary>
    public string SubjectReferenceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the classification tags.</summary>
    public List<ClassificationTag> ClassificationTags { get; set; } = new();

    /// <summary>Gets or sets the metadata modifiers.</summary>
    public List<MetadataModifier> Modifiers { get; set; } = new();

    /// <summary>Gets or sets whether this media is loopable.</summary>
    public bool Loopable { get; set; } = false;
}

/// <summary>
/// Represents a classifier for metadata tags within media metadata entries.
/// </summary>
public class ClassificationTag
{
    /// <summary>Gets or sets the tag key.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Gets or sets the tag value.</summary>
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Represents a modifier for media item classifications.
/// </summary>
public class MetadataModifier
{
    /// <summary>Gets or sets the modifier key.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Gets or sets the modifier value.</summary>
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Represents a generic modifier with key-value pair.
/// </summary>
public class Modifier
{
    /// <summary>Gets or sets the modifier key.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Gets or sets the modifier value.</summary>
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Single media metadata file containing all media metadata entries.
/// </summary>
public class MediaMetadataFile
{
    /// <summary>Gets or sets the file identifier.</summary>
    public string Id { get; set; } = "media-metadata";

    /// <summary>Gets or sets the metadata entries.</summary>
    public List<MediaMetadataEntry> Entries { get; set; } = new();

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the last update timestamp.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Gets or sets the file version.</summary>
    public string Version { get; set; } = "1.0";
}

/// <summary>
/// Individual character media metadata entry.
/// </summary>
public class CharacterMediaMetadataEntry
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the display title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the file name.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Gets or sets the media type (image, audio, video).</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the age rating.</summary>
    public int AgeRating { get; set; }

    /// <summary>Gets or sets the tags.</summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>Gets or sets whether this media is loopable.</summary>
    public bool Loopable { get; set; } = false;
}

/// <summary>
/// Single character media metadata file containing all character media metadata entries.
/// </summary>
public class CharacterMediaMetadataFile
{
    /// <summary>Gets or sets the file identifier.</summary>
    public string Id { get; set; } = "character-media-metadata";

    /// <summary>Gets or sets the metadata entries.</summary>
    public List<CharacterMediaMetadataEntry> Entries { get; set; } = new();

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the last update timestamp.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Gets or sets the file version.</summary>
    public string Version { get; set; } = "1.0";
}

/// <summary>
/// Individual character entry for API responses.
/// </summary>
public class Character
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the character name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the image media ID reference.</summary>
    public string Image { get; set; } = string.Empty;

    /// <summary>Gets or sets the character metadata.</summary>
    public CharacterMetadata Metadata { get; set; } = new();
}

/// <summary>
/// Character metadata for character definitions.
/// </summary>
public class CharacterMetadata
{
    /// <summary>Gets or sets the character roles (e.g., mentor, trickster).</summary>
    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; } = new();

    /// <summary>Gets or sets the character role (alias for Roles for compatibility).</summary>
    [JsonPropertyName("role")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<string> Role
    {
        get => Roles;
        set => Roles = value;
    }

    /// <summary>Gets or sets the character archetypes (e.g., guardian, the listener).</summary>
    [JsonPropertyName("archetypes")]
    public List<string> Archetypes { get; set; } = new();

    /// <summary>Gets or sets the character archetype (alias for Archetypes for compatibility).</summary>
    [JsonPropertyName("archetype")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<string> Archetype
    {
        get => Archetypes;
        set => Archetypes = value;
    }

    /// <summary>Gets or sets the species (e.g., elf, goblin).</summary>
    public string Species { get; set; } = string.Empty;

    /// <summary>Gets or sets the age.</summary>
    public int Age { get; set; } = 0;

    /// <summary>Gets or sets the traits (e.g., wise, calm, mysterious).</summary>
    public List<string> Traits { get; set; } = new();

    /// <summary>Gets or sets the backstory.</summary>
    public string Backstory { get; set; } = string.Empty;
}

/// <summary>
/// Character entry used in CharacterMapFile.
/// </summary>
public class CharacterMapFileCharacter
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the character name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the image media ID reference.</summary>
    public string Image { get; set; } = string.Empty;

    /// <summary>Gets or sets the character metadata.</summary>
    public CharacterMetadata Metadata { get; set; } = new();
}

/// <summary>
/// Single character map file containing all characters.
/// </summary>
public class CharacterMapFile
{
    /// <summary>Gets or sets the file identifier.</summary>
    public string Id { get; set; } = "character-map";

    /// <summary>Gets or sets the characters.</summary>
    public List<CharacterMapFileCharacter> Characters { get; set; } = new();

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the last update timestamp.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Gets or sets the file version.</summary>
    public string Version { get; set; } = "1.0";
}

/// <summary>
/// Represents the avatar mapping file stored in storage.
/// </summary>
public class AvatarConfigurationFile
{
    /// <summary>Gets or sets the file identifier.</summary>
    public string Id { get; set; } = "avatar-configuration";

    /// <summary>Gets or sets the age group to avatar mappings.</summary>
    public Dictionary<string, List<string>> AgeGroupAvatars { get; set; } = new();

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the last update timestamp.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Gets or sets the file version.</summary>
    public string Version { get; set; } = "1.0";
}

/// <summary>
/// Media references for scenes and other content.
/// </summary>
public class MediaReferences
{
    /// <summary>Gets or sets the image media ID.</summary>
    public string? Image { get; set; }

    /// <summary>Gets or sets the audio media ID.</summary>
    public string? Audio { get; set; }

    /// <summary>Gets or sets the video media ID.</summary>
    public string? Video { get; set; }
}
