using System.Text.Json;
using System.Text.Json.Serialization;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Mystira.StoryGenerator.Application.Scenarios;

/// <summary>
/// Application-layer implementation for creating Scenarios from JSON or YAML.
/// </summary>
public class ScenarioFactory : IScenarioFactory
{
    public Task<Scenario> CreateFromContentAsync(string content, ScenarioContentFormat format, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is null or empty", nameof(content));

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            return Task.FromResult(format switch
            {
                ScenarioContentFormat.Json => FromJson(content),
                ScenarioContentFormat.Yaml => FromYaml(content),
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported format")
            });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create Scenario from {format} content: {ex.Message}", ex);
        }
    }

    private static Scenario FromJson(string json)
    {
        // Deserialize into DTO that matches snake_case keys, then map to Domain model
        var dto = JsonSerializer.Deserialize<ScenarioDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        });

        if (dto == null)
            throw new JsonException("Deserialized scenario DTO is null");

        return Map(dto);
    }

    private static Scenario FromYaml(string yaml)
    {
        // YAML uses snake_case keys (e.g., next_scene)
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        var dto = deserializer.Deserialize<ScenarioDto>(yaml);
        if (dto == null)
            throw new InvalidOperationException("Deserialized YAML scenario DTO is null");

        return Map(dto);
    }

    #region DTOs
    private sealed class ScenarioDto
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
        [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
        [JsonPropertyName("tags")] public List<string>? Tags { get; set; }
        [JsonPropertyName("difficulty")] public string? Difficulty { get; set; }
        [JsonPropertyName("session_length")] public string? SessionLength { get; set; }
        [JsonPropertyName("archetypes")] public List<string>? Archetypes { get; set; }
        [JsonPropertyName("age_group")] public string AgeGroup { get; set; } = string.Empty;
        [JsonPropertyName("minimum_age")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int MinimumAge { get; set; }
        [JsonPropertyName("core_axes")] public List<string>? CoreAxes { get; set; }
        [JsonPropertyName("characters")] public List<CharacterDto>? Characters { get; set; }
        [JsonPropertyName("scenes")] public List<SceneDto>? Scenes { get; set; }
        [JsonPropertyName("created_at")] public DateTime? CreatedAt { get; set; }
    }

    private sealed class CharacterDto
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("image")] public string? Image { get; set; }
        [JsonPropertyName("audio")] public string? Audio { get; set; }
        [JsonPropertyName("metadata")] public CharacterMetadataDto? Metadata { get; set; }
    }

    private sealed class CharacterMetadataDto
    {
        [JsonPropertyName("role")] public List<string>? Role { get; set; }
        [JsonPropertyName("archetype")] public List<string>? Archetype { get; set; }
        [JsonPropertyName("species")] public string? Species { get; set; }
        [JsonPropertyName("age")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int? Age { get; set; }
        [JsonPropertyName("traits")] public List<string>? Traits { get; set; }
        [JsonPropertyName("backstory")] public string? Backstory { get; set; }
    }

    private sealed class SceneDto
    {
        [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
        [JsonPropertyName("type")] public string Type { get; set; } = "narrative";
        [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
        [JsonPropertyName("next_scene")] public string? NextScene { get; set; }
        [JsonPropertyName("media")] public MediaDto? Media { get; set; }
        [JsonPropertyName("branches")] public List<BranchDto>? Branches { get; set; }
        [JsonPropertyName("echo_reveals")] public List<EchoRevealDto>? EchoReveals { get; set; }
        [JsonPropertyName("difficulty")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int? Difficulty { get; set; }
    }

    private sealed class MediaDto
    {
        [JsonPropertyName("image")] public string? Image { get; set; }
        [JsonPropertyName("audio")] public string? Audio { get; set; }
        [JsonPropertyName("video")] public string? Video { get; set; }
    }

    private sealed class BranchDto
    {
        [JsonPropertyName("choice")] public string? Choice { get; set; }
        [JsonPropertyName("next_scene")] public string? NextScene { get; set; }
        [JsonPropertyName("echo_log")] public EchoLogDto? EchoLog { get; set; }
        [JsonPropertyName("compass_change")] public CompassChangeDto? CompassChange { get; set; }
    }

    private sealed class EchoLogDto
    {
        [JsonPropertyName("echo_type")] public string? EchoType { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("strength")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public double? Strength { get; set; }
        [JsonPropertyName("timestamp")] public DateTime? Timestamp { get; set; }
    }

    private sealed class CompassChangeDto
    {
        [JsonPropertyName("axis")] public string? Axis { get; set; }
        [JsonPropertyName("delta")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public double? Delta { get; set; }
        [JsonPropertyName("developmental_link")] public string? DevelopmentalLink { get; set; }
    }

    private sealed class EchoRevealDto
    {
        [JsonPropertyName("echo_type")] public string? EchoType { get; set; }
        [JsonPropertyName("min_strength")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public double? MinStrength { get; set; }
        [JsonPropertyName("trigger_scene_id")] public string? TriggerSceneId { get; set; }
        [JsonPropertyName("max_age_scenes")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int? MaxAgeScenes { get; set; }
        [JsonPropertyName("reveal_mechanic")] public string? RevealMechanic { get; set; }
        [JsonPropertyName("required")] public bool? Required { get; set; }
    }
    #endregion

    private static Scenario Map(ScenarioDto dto)
    {
        var scenario = new Scenario
        {
            Id = dto.Id ?? string.Empty,
            Title = dto.Title ?? string.Empty,
            Description = dto.Description ?? string.Empty,
            Tags = dto.Tags ?? new List<string>(),
            Difficulty = ParseDifficulty(dto.Difficulty),
            SessionLength = ParseSessionLength(dto.SessionLength),
            Archetypes = dto.Archetypes ?? new List<string>(),
            AgeGroup = dto.AgeGroup ?? string.Empty,
            MinimumAge = dto.MinimumAge,
            CoreAxes = dto.CoreAxes ?? new List<string>(),
            Characters = (dto.Characters ?? new()).Select(Map).ToList(),
            Scenes = (dto.Scenes ?? new()).Select(Map).ToList(),
            CreatedAt = dto.CreatedAt ?? DateTime.UtcNow
        };
        return scenario;
    }

    private static ScenarioCharacter Map(CharacterDto c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Image = c.Image,
        Audio = c.Audio,
        Metadata = Map(c.Metadata)
    };

    private static ScenarioCharacterMetadata Map(CharacterMetadataDto? m) => new()
    {
        Role = m?.Role ?? new(),
        Archetype = m?.Archetype ?? new(),
        Species = m?.Species ?? string.Empty,
        Age = m?.Age ?? 0,
        Traits = m?.Traits ?? new(),
        Backstory = m?.Backstory ?? string.Empty
    };

    private static Scene Map(SceneDto s) => new()
    {
        Id = s.Id,
        Title = s.Title,
        Type = ParseSceneType(s.Type),
        Description = s.Description,
        NextSceneId = s.NextScene,
        Media = s.Media == null ? null : new MediaReferences
        {
            Image = s.Media.Image,
            Audio = s.Media.Audio,
            Video = s.Media.Video
        },
        Branches = (s.Branches ?? new()).Select(Map).Where(b => !string.IsNullOrWhiteSpace(b.NextSceneId)).ToList(),
        EchoReveals = (s.EchoReveals ?? new()).Select(Map).ToList(),
        Difficulty = s.Difficulty
    };

    private static Branch Map(BranchDto b) => new()
    {
        Choice = b.Choice ?? string.Empty,
        NextSceneId = b.NextScene ?? string.Empty,
        EchoLog = b.EchoLog == null ? null : new EchoLog
        {
            EchoType = b.EchoLog.EchoType ?? string.Empty,
            Description = b.EchoLog.Description ?? string.Empty,
            Strength = b.EchoLog.Strength ?? 0,
            Timestamp = b.EchoLog.Timestamp ?? DateTime.UtcNow
        },
        CompassChange = b.CompassChange == null ? null : new CompassChange
        {
            Axis = b.CompassChange.Axis ?? string.Empty,
            Delta = b.CompassChange.Delta ?? 0,
            DevelopmentalLink = b.CompassChange.DevelopmentalLink
        }
    };

    private static EchoReveal Map(EchoRevealDto r) => new()
    {
        EchoType = r.EchoType ?? string.Empty,
        MinStrength = r.MinStrength ?? 0,
        TriggerSceneId = r.TriggerSceneId ?? string.Empty,
        MaxAgeScenes = r.MaxAgeScenes,
        RevealMechanic = r.RevealMechanic,
        Required = r.Required
    };

    private static DifficultyLevel ParseDifficulty(string? s)
        => s?.Trim().ToLowerInvariant() switch
        {
            "easy" => DifficultyLevel.Easy,
            "hard" => DifficultyLevel.Hard,
            _ => DifficultyLevel.Medium
        };

    private static SessionLength ParseSessionLength(string? s)
        => s?.Trim().ToLowerInvariant() switch
        {
            "short" => SessionLength.Short,
            "long" => SessionLength.Long,
            _ => SessionLength.Medium
        };

    private static SceneType ParseSceneType(string? s)
        => s?.Trim().ToLowerInvariant() switch
        {
            "narrative" => SceneType.Narrative,
            "choice" => SceneType.Choice,
            "roll" => SceneType.Roll,
            "special" => SceneType.Special,
            _ => SceneType.Narrative
        };
}
