using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Mystira.StoryGenerator.Domain.Stories;

public class Scenario
{
    [JsonPropertyName("title")]
    [Required]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    [Required]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; } = string.Empty; // Easy | Medium | Hard

    [JsonPropertyName("session_length")]
    public string SessionLength { get; set; } = string.Empty; // Short | Medium | Long

    [JsonPropertyName("age_group")]
    public string AgeGroup { get; set; } = string.Empty; // e.g., "6-9"

    [JsonPropertyName("minimum_age")]
    public int MinimumAge { get; set; }

    [JsonPropertyName("core_axes")]
    public List<string> CoreAxes { get; set; } = new();

    [JsonPropertyName("archetypes")]
    public List<string> Archetypes { get; set; } = new();

    [JsonPropertyName("characters")]
    public List<Character> Characters { get; set; } = new();

    [JsonPropertyName("scenes")]
    public List<Scene> Scenes { get; set; } = new();
}

public class Character
{
    [JsonPropertyName("id")]
    [Required]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    [Required]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("audio")]
    public string? Audio { get; set; }

    [JsonPropertyName("metadata")]
    public CharacterMetadata Metadata { get; set; } = new();
}

public class CharacterMetadata
{
    [JsonPropertyName("role")]
    public List<string> Role { get; set; } = new();

    [JsonPropertyName("archetype")]
    public List<string> Archetype { get; set; } = new();

    [JsonPropertyName("species")]
    public string Species { get; set; } = string.Empty;

    [JsonPropertyName("age")]
    public int Age { get; set; }

    [JsonPropertyName("traits")]
    public List<string> Traits { get; set; } = new();

    [JsonPropertyName("backstory")]
    public string Backstory { get; set; } = string.Empty;
}

public class Scene
{
    [JsonPropertyName("id")]
    [Required]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    [Required]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    [Required]
    public string Type { get; set; } = string.Empty; // narrative | choice | roll | special

    [JsonPropertyName("description")]
    [Required]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("next_scene")]
    public string? NextScene { get; set; }

    [JsonPropertyName("difficulty")]
    public double? Difficulty { get; set; } // for roll scenes

    [JsonPropertyName("media")]
    public SceneMedia? Media { get; set; }

    [JsonPropertyName("choices")]
    public List<Choice>? Choices { get; set; }

    [JsonPropertyName("roll_requirements")]
    public List<RollRequirement>? RollRequirements { get; set; }
}

public class SceneMedia
{
    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("audio")]
    public string? Audio { get; set; }

    [JsonPropertyName("video")]
    public string? Video { get; set; }
}

public class Choice
{
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("options")]
    public List<ChoiceOption> Options { get; set; } = new();
}

public class ChoiceOption
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("next_scene")]
    public string? NextScene { get; set; }
}

public class RollRequirement
{
    [JsonPropertyName("attribute")]
    public string Attribute { get; set; } = string.Empty;

    [JsonPropertyName("target")]
    public int Target { get; set; }

    [JsonPropertyName("success")]
    public string? SuccessNextScene { get; set; }

    [JsonPropertyName("failure")]
    public string? FailureNextScene { get; set; }
}
