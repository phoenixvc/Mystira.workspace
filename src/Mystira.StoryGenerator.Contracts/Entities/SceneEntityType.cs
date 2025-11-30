using System.Text.Json.Serialization;

namespace Mystira.StoryGenerator.Contracts.Entities;

public enum SceneEntityType
{
    /// <summary>
    /// Places, rooms, villages, forests, magical realms
    /// </summary>
    [JsonPropertyName("location")]
    Location,

    /// <summary>
    /// Any sentient individual (child, adult, animal with personality, monster, etc.)
    /// </summary>
    [JsonPropertyName("character")]
    Character,

    /// <summary>
    /// physical objects, tools, weapons, artifacts, etc.
    /// </summary>
    [JsonPropertyName("item")]
    Item,

    /// <summary>
    /// Abstract ideas (fear, honesty, courage, chaos, magic, time)
    /// </summary>
    [JsonPropertyName("concept")]
    Concept
}
