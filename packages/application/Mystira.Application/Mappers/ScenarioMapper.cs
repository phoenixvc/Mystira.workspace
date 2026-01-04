using Mystira.Domain.Models;
using Mystira.Contracts.App.Requests.Scenarios;
using Riok.Mapperly.Abstractions;

namespace Mystira.Application.Mappers;

/// <summary>
/// Mapperly-based mapper for scenario-related types.
/// Uses compile-time source generation for optimal performance.
/// </summary>
[Mapper]
public static partial class ScenarioMapper
{
    /// <summary>
    /// Maps a CharacterRequest to a ScenarioCharacter domain model.
    /// Note: Many properties exist only on the entity side (for persistence) or request side.
    /// </summary>
    // Ignore source properties that don't exist on target
    [MapperIgnoreSource(nameof(CharacterRequest.Description))]
    [MapperIgnoreSource(nameof(CharacterRequest.Role))]
    [MapperIgnoreSource(nameof(CharacterRequest.Archetype))]
    [MapperIgnoreSource(nameof(CharacterRequest.Traits))]
    [MapperIgnoreSource(nameof(CharacterRequest.IsPlayerCharacter))]
    [MapperIgnoreSource(nameof(CharacterRequest.Image))]
    [MapperIgnoreSource(nameof(CharacterRequest.Audio))]
    [MapperIgnoreSource(nameof(CharacterRequest.Metadata))]
    // Ignore target properties that don't exist on source (Entity-specific)
    [MapperIgnoreTarget(nameof(ScenarioCharacter.ScenarioId))]
    [MapperIgnoreTarget(nameof(ScenarioCharacter.Description))]
    [MapperIgnoreTarget(nameof(ScenarioCharacter.Role))]
    [MapperIgnoreTarget(nameof(ScenarioCharacter.ArchetypeId))]
    [MapperIgnoreTarget(nameof(ScenarioCharacter.AvatarUrl))]
    [MapperIgnoreTarget(nameof(ScenarioCharacter.IsPlayable))]
    [MapperIgnoreTarget(nameof(ScenarioCharacter.IsProtagonist))]
    [MapperIgnoreTarget(nameof(ScenarioCharacter.PersonalityTraitsJson))]
    [MapperIgnoreTarget(nameof(ScenarioCharacter.Backstory))]
    [MapperIgnoreTarget(nameof(ScenarioCharacter.DisplayOrder))]
    [MapperIgnoreTarget(nameof(ScenarioCharacter.CreatedAt))]
    [MapperIgnoreTarget(nameof(ScenarioCharacter.UpdatedAt))]
    [MapperIgnoreTarget(nameof(ScenarioCharacter.CreatedBy))]
    [MapperIgnoreTarget(nameof(ScenarioCharacter.UpdatedBy))]
    [MapperIgnoreTarget(nameof(ScenarioCharacter.Image))]
    [MapperIgnoreTarget(nameof(ScenarioCharacter.Audio))]
    [MapperIgnoreTarget(nameof(ScenarioCharacter.Metadata))]
    public static partial ScenarioCharacter ToScenarioCharacter(CharacterRequest request);

    /// <summary>
    /// Maps CharacterMetadataRequest to ScenarioCharacterMetadata.
    /// </summary>
    [MapProperty(nameof(CharacterMetadataRequest.Archetype), nameof(ScenarioCharacterMetadata.Archetypes), Use = nameof(MapArchetypes))]
    [MapProperty(nameof(CharacterMetadataRequest.Role), nameof(ScenarioCharacterMetadata.Roles))]
    private static partial ScenarioCharacterMetadata ToScenarioCharacterMetadata(CharacterMetadataRequest request);

    /// <summary>
    /// Maps a SceneRequest to a Scene domain model.
    /// Note: Many properties exist only on the entity side (for persistence) or request side.
    /// </summary>
    [MapProperty(nameof(SceneRequest.Type), nameof(Scene.Type), Use = nameof(ParseSceneType))]
    [MapProperty(nameof(SceneRequest.Difficulty), nameof(Scene.Difficulty), Use = nameof(ParseDifficulty))]
    [MapProperty(nameof(SceneRequest.Media), nameof(Scene.Media), Use = nameof(MapMedia))]
    [MapProperty(nameof(SceneRequest.Branches), nameof(Scene.Branches), Use = nameof(MapBranches))]
    [MapProperty(nameof(SceneRequest.EchoReveals), nameof(Scene.EchoReveals), Use = nameof(MapEchoReveals))]
    // Ignore source properties that don't exist on target or are handled differently
    [MapperIgnoreSource(nameof(SceneRequest.Content))]
    [MapperIgnoreSource(nameof(SceneRequest.Order))]
    [MapperIgnoreSource(nameof(SceneRequest.BackgroundImage))]
    [MapperIgnoreSource(nameof(SceneRequest.BackgroundMusic))]
    [MapperIgnoreSource(nameof(SceneRequest.Choices))]
    // Ignore target properties that don't exist on source (Entity-specific)
    [MapperIgnoreTarget(nameof(Scene.Music))]
    [MapperIgnoreTarget(nameof(Scene.SoundEffects))]
    [MapperIgnoreTarget(nameof(Scene.ScenarioId))]
    [MapperIgnoreTarget(nameof(Scene.Content))]
    [MapperIgnoreTarget(nameof(Scene.Slug))]
    [MapperIgnoreTarget(nameof(Scene.BackgroundUrl))]
    [MapperIgnoreTarget(nameof(Scene.MusicUrl))]
    [MapperIgnoreTarget(nameof(Scene.AmbientSoundUrl))]
    [MapperIgnoreTarget(nameof(Scene.DisplayOrder))]
    [MapperIgnoreTarget(nameof(Scene.IsEnding))]
    [MapperIgnoreTarget(nameof(Scene.EndingType))]
    [MapperIgnoreTarget(nameof(Scene.CompassChanges))]
    [MapperIgnoreTarget(nameof(Scene.CreatedAt))]
    [MapperIgnoreTarget(nameof(Scene.UpdatedAt))]
    [MapperIgnoreTarget(nameof(Scene.CreatedBy))]
    [MapperIgnoreTarget(nameof(Scene.UpdatedBy))]
    public static partial Scene ToScene(SceneRequest request);

    /// <summary>
    /// Maps a BranchRequest to a Branch domain model.
    /// Note: Many properties exist only on the entity side (for persistence) or request side.
    /// </summary>
    // Ignore source properties that don't exist on target or are handled via manual mapping
    [MapperIgnoreSource(nameof(BranchRequest.Text))]
    [MapperIgnoreSource(nameof(BranchRequest.CompassAxis))]
    [MapperIgnoreSource(nameof(BranchRequest.CompassDirection))]
    [MapperIgnoreSource(nameof(BranchRequest.CompassDelta))]
    [MapperIgnoreSource(nameof(BranchRequest.EchoLog))]
    // Ignore target properties that don't exist on source (Entity-specific)
    [MapperIgnoreTarget(nameof(Branch.Choice))]
    [MapperIgnoreTarget(nameof(Branch.EchoLog))]
    [MapperIgnoreTarget(nameof(Branch.CompassChange))]
    [MapperIgnoreTarget(nameof(Branch.SceneId))]
    [MapperIgnoreTarget(nameof(Branch.TargetSceneId))]
    [MapperIgnoreTarget(nameof(Branch.Text))]
    [MapperIgnoreTarget(nameof(Branch.Description))]
    [MapperIgnoreTarget(nameof(Branch.DisplayOrder))]
    [MapperIgnoreTarget(nameof(Branch.ConditionsJson))]
    [MapperIgnoreTarget(nameof(Branch.IsHidden))]
    [MapperIgnoreTarget(nameof(Branch.CompassChanges))]
    [MapperIgnoreTarget(nameof(Branch.Id))]
    [MapperIgnoreTarget(nameof(Branch.CreatedAt))]
    [MapperIgnoreTarget(nameof(Branch.UpdatedAt))]
    [MapperIgnoreTarget(nameof(Branch.CreatedBy))]
    [MapperIgnoreTarget(nameof(Branch.UpdatedBy))]
    public static partial Branch ToBranch(BranchRequest request);

    /// <summary>
    /// Safely converts a request difficulty int to domain DifficultyLevel.
    /// Validates that the input value corresponds to a defined enum value.
    /// </summary>
    /// <param name="requestDifficulty">The difficulty level from the request</param>
    /// <returns>The corresponding domain DifficultyLevel</returns>
    /// <exception cref="ArgumentException">Thrown when the input value is not a valid DifficultyLevel</exception>
    public static DifficultyLevel MapDifficultyLevel(int requestDifficulty)
    {
        if (!Enum.IsDefined(typeof(DifficultyLevel), requestDifficulty))
        {
            var validValues = string.Join(", ", Enum.GetValues<DifficultyLevel>()
                .Select(e => $"{e} ({(int)e})"));
            throw new ArgumentException(
                $"Invalid difficulty level: {requestDifficulty}. Valid values are: {validValues}",
                nameof(requestDifficulty));
        }

        return (DifficultyLevel)requestDifficulty;
    }

    /// <summary>
    /// Safely converts a request session length int to domain SessionLength.
    /// Validates that the input value corresponds to a defined enum value.
    /// </summary>
    /// <param name="requestSessionLength">The session length from the request</param>
    /// <returns>The corresponding domain SessionLength</returns>
    /// <exception cref="ArgumentException">Thrown when the input value is not a valid SessionLength</exception>
    public static SessionLength MapSessionLength(int requestSessionLength)
    {
        if (!Enum.IsDefined(typeof(SessionLength), requestSessionLength))
        {
            var validValues = string.Join(", ", Enum.GetValues<SessionLength>()
                .Select(e => $"{e} ({(int)e})"));
            throw new ArgumentException(
                $"Invalid session length: {requestSessionLength}. Valid values are: {validValues}",
                nameof(requestSessionLength));
        }

        return (SessionLength)requestSessionLength;
    }

    /// <summary>
    /// Safely parses a list of archetype strings, filtering out any null results.
    /// Returns string values for storage in Scenario.Archetypes (List&lt;string&gt;).
    /// </summary>
    public static List<string> ParseArchetypes(IEnumerable<string>? archetypes)
    {
        if (archetypes == null)
        {
            return new List<string>();
        }

        return archetypes
            .Select(Archetype.Parse)
            .OfType<Archetype>()
            .Select(a => a.Value)
            .ToList();
    }

    /// <summary>
    /// Safely parses a list of core axis strings, filtering out any null results.
    /// Returns string values for storage in Scenario.CoreAxes (List&lt;string&gt;).
    /// </summary>
    public static List<string> ParseCoreAxes(IEnumerable<string>? coreAxes)
    {
        if (coreAxes == null)
        {
            return new List<string>();
        }

        return coreAxes
            .Select(CoreAxis.Parse)
            .OfType<CoreAxis>()
            .Select(a => a.Value)
            .ToList();
    }

    // Custom mapping methods for complex scenarios

    private static List<Archetype> MapArchetypes(List<string>? archetypes)
    {
        if (archetypes == null)
        {
            return new List<Archetype>();
        }

        return archetypes
            .Select(Archetype.Parse)
            .OfType<Archetype>()
            .ToList();
    }

    private static SceneType ParseSceneType(string? type)
        => Enum.TryParse<SceneType>(type, true, out var sceneType)
            ? sceneType
            : SceneType.Standard;

    private static int? ParseDifficulty(string? difficulty)
        => string.IsNullOrEmpty(difficulty) ? null : int.TryParse(difficulty, out var diff) ? diff : null;

    private static MediaReferences? MapMedia(MediaReferencesRequest? media)
        => media != null ? new MediaReferences
        {
            Image = media.Image,
            Audio = media.Audio,
            Video = media.Video
        } : null;

    private static ICollection<Branch> MapBranches(List<BranchRequest>? branches)
        => branches?.Select(b => new Branch
        {
            Choice = b.Text ?? string.Empty,
            NextSceneId = b.NextSceneId ?? string.Empty,
            CompassChange = (b.CompassAxis != null || b.CompassDelta.HasValue)
                ? new CompassChangeDto
                {
                    Axis = b.CompassAxis ?? string.Empty,
                    Delta = b.CompassDelta ?? 0.0
                }
                : null,
            EchoLog = MapEchoLog(b.EchoLog)
        }).ToList() ?? new List<Branch>();

    private static EchoLog? MapEchoLog(EchoLogRequest? echoLog)
        => echoLog != null
            ? new EchoLog
            {
                EchoTypeId = echoLog.EchoType ?? string.Empty,
                Description = echoLog.Description ?? string.Empty,
                Strength = Math.Clamp(echoLog.Strength ?? 1.0, 0.0, 1.0),
                Timestamp = DateTime.UtcNow
            }
            : null;

    private static ICollection<EchoReveal> MapEchoReveals(List<EchoRevealRequest>? echoReveals)
        => echoReveals?.Select(e => new EchoReveal
        {
            EchoType = e.Tone ?? "honesty",
            TriggerSceneId = e.Condition ?? string.Empty,
            RevealMechanic = e.Message
        }).ToList() ?? new List<EchoReveal>();
}
