
using System.Text;
using System.Text.Json;
using System.IO;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Api.Services.LLM;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Contracts.Stories;

namespace Mystira.StoryGenerator.Api.Services;

public class StoryGenerationService : IStoryGenerationService
{
    private readonly ILLMServiceFactory _llmFactory;
    private readonly AiSettings _settings;
    private readonly ILogger<StoryGenerationService> _logger;

    public StoryGenerationService(
        ILLMServiceFactory llmFactory,
        IOptions<AiSettings> aiOptions,
        ILogger<StoryGenerationService> logger)
    {
        _llmFactory = llmFactory;
        _settings = aiOptions.Value;
        _logger = logger;
    }

    public async Task<GenerateJsonStoryResponse> GenerateJsonStoryAsync(GenerateJsonStoryRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var service = !string.IsNullOrWhiteSpace(request.Provider)
                ? _llmFactory.GetService(request.Provider!)
                : _llmFactory.GetDefaultService();

            if (service is null)
            {
                return new GenerateJsonStoryResponse
                {
                    Success = false,
                    Error = "No LLM services are currently available"
                };
            }

            var resolvedModelId = string.IsNullOrWhiteSpace(request.ModelId) ? null : request.ModelId;
            var resolvedModelName = string.IsNullOrWhiteSpace(request.Model) ? null : request.Model;
            var temperature = _settings.DefaultTemperature;
            var maxTokens = Math.Max(1200, _settings.DefaultMaxTokens);

            var systemPrompt = BuildSystemPrompt();
            var messages = BuildMessages(request);

            var chatRequest = new ChatCompletionRequest
            {
                Provider = service.ProviderName,
                ModelId = resolvedModelId,
                Model = resolvedModelName,
                Temperature = temperature,
                MaxTokens = maxTokens,
                Messages = messages,
                SystemPrompt = systemPrompt,
                JsonSchemaFormat = LoadSchemaFormatSafe()
            };

            var response = await service.CompleteAsync(chatRequest, cancellationToken);
            if (!response.Success)
            {
                return new GenerateJsonStoryResponse
                {
                    Success = false,
                    Error = response.Error ?? "LLM returned an error",
                    Provider = response.Provider ?? service.ProviderName,
                    Model = response.Model ?? resolvedModelName ?? string.Empty,
                    ModelId = response.ModelId ?? resolvedModelId
                };
            }

            return new GenerateJsonStoryResponse
            {
                Success = true,
                Json = response.Content ?? string.Empty,
                Provider = response.Provider ?? service.ProviderName,
                Model = response.Model ?? resolvedModelName ?? string.Empty,
                ModelId = response.ModelId ?? resolvedModelId
            };
        }
        catch (OperationCanceledException)
        {
            return new GenerateJsonStoryResponse
            {
                Success = false,
                Error = "Request was cancelled"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating story JSON");
            return new GenerateJsonStoryResponse
            {
                Success = false,
                Error = "An unexpected error occurred during generation"
            };
        }
    }

    private static string BuildSystemPrompt()
    {
        return @"
You are a professional interactive storytelling engine trained to generate branching adventure scenarios for young players.
Each story consists of modular scenes that use one of three types: narrative, choice, or roll.
Output must be a single JSON object using the following keys:
- title, description, tags, difficulty, session_length, age_group, minimum_age, core_axes, archetypes
- characters: array of character entries with id, name, optional image/audio URLs, and metadata (role, archetype, species, age, traits, backstory)
- scenes: each with id, title, type, description, optional media, choices/branches or roll_requirements, and optional echo_log, compass_change, echo_reveal_references
Only output a single valid JSON object—no commentary, markdown, or code fences.";
    }

    private static List<MystiraChatMessage> BuildMessages(GenerateJsonStoryRequest request)
    {
        var messages = new List<MystiraChatMessage>();

        // Few-shot example in JSON (structure only)
        var exampleJson = GetShortExampleJson();
        messages.Add(new MystiraChatMessage
        {
            MessageType = ChatMessageType.AI,
            Content = "Example output (for structure only; do not copy content, follow structure and keys):\n\n" + exampleJson
        });

        var instruction = BuildInstructionPrompt(request);
        messages.Add(new MystiraChatMessage
        {
            MessageType = ChatMessageType.System,
            Content = instruction
        });

        // Context JSON payload with selected parameters
        var payload = new
        {
            title = request.Title,
            difficulty = request.Difficulty,
            session_length = request.SessionLength,
            age_group = request.AgeGroup,
            minimum_age = request.MinimumAge,
            core_axes = request.CoreAxes,
            archetypes = request.Archetypes,
            character_count = request.CharacterCount,
            tags = request.Tags ?? new List<string>(),
            tone = request.Tone,
            min_scenes = request.MinScenes,
            max_scenes = request.MaxScenes
        };

        var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        messages.Add(new MystiraChatMessage
        {
            MessageType = ChatMessageType.User,
            Content = "Context parameters (use these exactly):\n" + jsonPayload
        });

        return messages;
    }

    private static string BuildInstructionPrompt(GenerateJsonStoryRequest request)
    {
        var min = Math.Max(1, request.MinScenes);
        var max = Math.Max(min + 1, request.MaxScenes);
        var sb = new StringBuilder();
        sb.AppendLine($"Generate a complete story with {min}–{max} scenes for a tabletop RPG session.");
        sb.AppendLine("The story must be appropriate for the given age group and contain diverse scenes including exploration, dialogue, obstacles, and meaningful choices.");
        sb.AppendLine();
        sb.AppendLine("Requirements:");
        sb.AppendLine("- Use the context parameters provided below (title, difficulty, session length, age group, minimum age, core axes, archetypes, character_count).");
        sb.AppendLine("- age group must be one of \"1-3\", \"4-5\", \"6-9\", \"10-12\", \"13-18\".");
        sb.AppendLine("- Format the result as a single structured JSON object that includes metadata, a characters array, and a scenes array.");
        sb.AppendLine("- Generate a `characters` section with exactly the number of characters specified by `character_count`. Each character must include: id (lowercase snake_case), name, optional image/audio URLs, and a metadata block with role(s), archetype(s), species, age, traits[], and backstory.");
        sb.AppendLine("- Scene types must be well distributed across narrative, choice, and roll.");
        sb.AppendLine("- At least 3 scenes must be of type 'choice' and at least 1 must be of type 'roll'.");
        sb.AppendLine("- Include echo_log and compass_change for key decisions affecting compass axes.");
        sb.AppendLine("- Include optional media fields (image/audio/video URLs) for some scenes.");
        sb.AppendLine();
        sb.AppendLine("Important constraints:");
        sb.AppendLine("- Only output valid JSON. No commentary, markdown, or code fences.");
        sb.AppendLine("- All IDs must be lowercase snake_case, with scene IDs prefixed by the story ID.");
        sb.AppendLine("- Ensure logical branching with unique scene IDs and complete next_scene references where applicable.");
        return sb.ToString();
    }

    private static string GetShortExampleJson()
    {
        var example = new
        {
            title = "The Mystery of the Missing Glow-Berries",
            description = "Four friends from Whispering Woods discover the special Glow-Berries are gone.",
            tags = new[] { "mystery", "animals", "friendship", "forest" },
            difficulty = "Easy",
            session_length = "Short",
            age_group = "6-9",
            minimum_age = 6,
            core_axes = new[] { "Empathy", "Accountability" },
            archetypes = new[] { "the_helper", "the_science_investigator" },
            characters = new[]
            {
                new
                {
                    id = "archimedes",
                    name = "Archimedes",
                    metadata = new
                    {
                        role = new[] { "Thinker" },
                        archetype = new[] { "the_science_investigator" },
                        species = "Owl",
                        age = 12,
                        traits = new[] { "wise", "logical" },
                        backstory = "From high branches, Archimedes maps moonlit patterns."
                    }
                }
            },
            scenes = new[]
            {
                new
                {
                    id = "glow_berries_start",
                    type = "narrative",
                    title = "The Empty Bush",
                    description = "The bush where the brightest Glow-Berries grew is empty.",
                    next_scene = "glow_berries_first_choice"
                }
            }
        };

        return JsonSerializer.Serialize(example, new JsonSerializerOptions { WriteIndented = true });
    }

    private JsonSchemaResponseFormat? LoadSchemaFormatSafe()
    {
        try
        {
            var configuredPath = _settings.AzureOpenAI.SchemaValidation.SchemaPath;
            string schemaPath = string.IsNullOrWhiteSpace(configuredPath)
                ? Path.Combine(AppContext.BaseDirectory, "config", "story-schema.json")
                : (Path.IsPathRooted(configuredPath)
                    ? configuredPath
                    : Path.Combine(AppContext.BaseDirectory, configuredPath));
            if (File.Exists(schemaPath))
            {
                var json = File.ReadAllText(schemaPath);
                return new JsonSchemaResponseFormat
                {
                    FormatName = "mystira-story-generated",
                    SchemaJson = json,
                    IsStrict = _settings.AzureOpenAI.SchemaValidation.IsSchemaValidationStrict
                };
            }
        }
        catch
        {
            // ignore and fall back
        }
        return null;
    }
}
