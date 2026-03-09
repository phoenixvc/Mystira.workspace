using System.Text.Json.Serialization;
using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;

namespace Mystira.StoryGenerator.Contracts.Entities;

// Enable Newtonsoft.Json to (de)serialize enum values as lowercase strings
// matching the schema: "character" | "location" | "item" | "concept"
[Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
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
    /// physical objects, tools, weapons, artifacts, etc.
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
