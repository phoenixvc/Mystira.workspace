using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Entities;

/// <summary>
/// Type of story entity.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EntityType
{
    /// <summary>
    /// A character entity (person, creature, etc.).
    /// </summary>
    [JsonPropertyName("character")]
    [EnumMember(Value = "character")]
    Character,

    /// <summary>
    /// A location entity (place, room, realm, etc.).
    /// </summary>
    [JsonPropertyName("location")]
    [EnumMember(Value = "location")]
    Location,

    /// <summary>
    /// An item entity (object, tool, artifact, etc.).
    /// </summary>
    [JsonPropertyName("item")]
    [EnumMember(Value = "item")]
    Item,

    /// <summary>
    /// A concept entity (abstract idea, theme, etc.).
    /// </summary>
    [JsonPropertyName("concept")]
    [EnumMember(Value = "concept")]
    Concept,

    /// <summary>
    /// An organization entity (group, faction, etc.).
    /// </summary>
    [JsonPropertyName("organization")]
    [EnumMember(Value = "organization")]
    Organization,

    /// <summary>
    /// An event entity (historical event, occurrence, etc.).
    /// </summary>
    [JsonPropertyName("event")]
    [EnumMember(Value = "event")]
    Event,

    /// <summary>
    /// Other type of entity.
    /// </summary>
    [JsonPropertyName("other")]
    [EnumMember(Value = "other")]
    Other
}
