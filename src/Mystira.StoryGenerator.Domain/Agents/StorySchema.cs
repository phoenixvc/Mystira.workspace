using System.Text.Json.Serialization;

namespace Mystira.StoryGenerator.Domain.Agents;

public class StorySchema
{
    [JsonPropertyName("metadata")]
    public StoryMetadata Metadata { get; set; } = new();

    [JsonPropertyName("characters")]
    public List<StoryCharacter> Characters { get; set; } = new();

    [JsonPropertyName("scenes")]
    public List<StoryScene> Scenes { get; set; } = new();

    [JsonPropertyName("narrative_summary")]
    public string NarrativeSummary { get; set; } = string.Empty;

    [JsonPropertyName("safety_annotations")]
    public StorySafetyAnnotations SafetyAnnotations { get; set; } = new();
}

public class StoryMetadata
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("age_group")]
    public string AgeGroup { get; set; } = string.Empty;

    [JsonPropertyName("themes")]
    public List<string> Themes { get; set; } = new();

    [JsonPropertyName("target_axes")]
    public List<string> TargetAxes { get; set; } = new();

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("version")]
    public int Version { get; set; }
}

public class StoryCharacter
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("age")]
    public int Age { get; set; }

    [JsonPropertyName("key_traits")]
    public List<string> KeyTraits { get; set; } = new();

    [JsonPropertyName("initial_axes_state")]
    public Dictionary<string, float> InitialAxesState { get; set; } = new();
}

public class StoryScene
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("sequence")]
    public int Sequence { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("setting")]
    public string Setting { get; set; } = string.Empty;

    [JsonPropertyName("characters_involved")]
    public List<string> CharactersInvolved { get; set; } = new();

    [JsonPropertyName("choices")]
    public List<StoryChoice> Choices { get; set; } = new();

    [JsonPropertyName("narrative")]
    public string Narrative { get; set; } = string.Empty;
}

public class StoryChoice
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("consequence_axes_delta")]
    public Dictionary<string, float> ConsequenceAxesDelta { get; set; } = new();

    [JsonPropertyName("narrative_impact")]
    public string NarrativeImpact { get; set; } = string.Empty;
}

public class StorySafetyAnnotations
{
    [JsonPropertyName("content_warnings")]
    public List<string> ContentWarnings { get; set; } = new();

    [JsonPropertyName("age_appropriateness_notes")]
    public string AgeAppropriatenessNotes { get; set; } = string.Empty;

    [JsonPropertyName("themes_explored")]
    public List<string> ThemesExplored { get; set; } = new();
}
