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
    /// Non-character animals or monsters appearing without personhood
    /// </summary>
    [JsonPropertyName("creature")]
    Creature,

    /// <summary>
    /// physical objects, tools, weapons, artifacts, etc.
    /// </summary>
    [JsonPropertyName("item")]
    Item,

    /// <summary>
    /// Abstract ideas (fear, honesty, courage, chaos, magic, time)
    /// </summary>
    [JsonPropertyName("concept")]
    Concept,

    /// <summary>
    /// Actions or happenings treated as objects (e.g., “the Great Festival”)
    /// </summary>
    [JsonPropertyName("event")]
    Event,

    /// <summary>
    /// Groups, guilds, councils, teams
    /// </summary>
    [JsonPropertyName("Organization")]
    Organization,

    /// <summary>
    /// Powers or learned skills (“shadow magic”, “lockpicking”)
    /// </summary>
    [JsonPropertyName("Ability")]
    Ability,


    /// <summary>
    /// Emotional or physical states (“fatigue”, “excitement”)
    /// </summary>
    [JsonPropertyName("Condition")]
    Condition
}
