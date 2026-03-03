using System.Text.Json.Serialization;
using System.Runtime.Serialization;

namespace Mystira.Contracts.StoryGenerator.Entities;

/// <summary>
/// Type of entity in a scene: character, location, item, or concept.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SceneEntityType
{
    /// <summary>
    /// Places, rooms, villages, forests, magical realms
    /// </summary>
    [JsonPropertyName("location")]
    [EnumMember(Value = "location")]
    Location,

    /// <summary>
    /// Any sentient individual (child, adult, animal with personality, monster, etc.)
    /// </summary>
    [JsonPropertyName("character")]
    [EnumMember(Value = "character")]
    Character,

    /// <summary>
    /// Physical objects, tools, weapons, artifacts, etc.
    /// </summary>
    [JsonPropertyName("item")]
    [EnumMember(Value = "item")]
    Item,

    /// <summary>
    /// Abstract ideas (fear, honesty, courage, chaos, magic, time)
    /// </summary>
    [JsonPropertyName("concept")]
    [EnumMember(Value = "concept")]
    Concept
}
