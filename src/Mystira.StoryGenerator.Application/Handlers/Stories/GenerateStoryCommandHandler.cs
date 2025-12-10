using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Domain.Commands;
using Mystira.StoryGenerator.Domain.Commands.Stories;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Application.Handlers.Stories;

public class GenerateStoryCommandHandler : ICommandHandler<GenerateStoryCommand, GenerateJsonStoryResponse>
{
    private readonly ILlmServiceFactory _llmFactory;
    private readonly AiSettings _settings;
    private readonly IStorySchemaProvider _schemaProvider;
    private readonly IInstructionBlockService _instructionBlockService;
    private readonly ILlmIntentLlmClassificationService _llmIntentLlmClassificationService;
    private readonly ILogger<GenerateStoryCommandHandler> _logger;

    public GenerateStoryCommandHandler(
        ILlmServiceFactory llmFactory,
        IOptions<AiSettings> aiOptions,
        IStorySchemaProvider schemaProvider,
        IInstructionBlockService instructionBlockService,
        ILlmIntentLlmClassificationService llmIntentLlmClassificationService,
        ILogger<GenerateStoryCommandHandler> logger)
    {
        _llmFactory = llmFactory;
        _settings = aiOptions.Value;
        _schemaProvider = schemaProvider;
        _instructionBlockService = instructionBlockService;
        _llmIntentLlmClassificationService = llmIntentLlmClassificationService;
        _logger = logger;
    }

    public async Task<GenerateJsonStoryResponse> Handle(GenerateStoryCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var request = command.Request;
            var userQuery = command.UserQuery;
            var service = !string.IsNullOrWhiteSpace(request.Provider)
                ? _llmFactory.GetService(request.Provider!, request.Model)
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
            var instructionBlock = await ResolveInstructionBlockAsync(command, cancellationToken);
            if (instructionBlock is null)
            {
                _logger.LogWarning("Instruction search is disabled because search or embedding clients return null");
            }

            var messages = BuildMessages(request, userQuery, instructionBlock);

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
You are the Mystira interactive storytelling engine for a kids’ online story app (with audio, media, and video). Generate branching adventure stories for young players.
Use the provided context parameters (e.g. title, difficulty, session_length, age_group, minimum_age, core_axes, archetypes, character_count, minScenes, maxScenes. Treat them as authoritative.
Your task:
Create a complete tabletop-style RPG adventure with {minScenes}–{maxScenes} scenes, mixing exploration, dialogue, obstacles, and meaningful choices.
JSON OUTPUT (single object only)
Output only valid JSON, no markdown or commentary.
Top-level keys (no extras allowed):
    •   title, description, tags, difficulty, session_length, age_group, minimum_age, core_axes, archetypes
    •   characters: array
    •   scenes: array
STRING FORMATTING (Critical)
    •   All string fields must be single-line.
    •   Do not insert newline or carriage return characters in any string value.
    •   This is especially strict for: title, description, backstory, and any branch description.
    •   If a sentence would normally be on a new line, replace the line break with a single space.
    •   Allowed whitespace inside strings: regular spaces only.
    •   The JSON itself may be pretty-printed, but string values must not contain line breaks.
    •   The JSON must be self-contained, parseable, and obey all rules below.
CHARACTERS
    •   characters must contain exactly character_count entries.
    •   Each character has:
        o   id: lowercase snake_case, unique (e.g. ""brave_fox"")
        o   name
        o   optional image and audio URLs
        o   metadata object with:
    •   role: one or more narrative roles (e.g. ""protagonist"", ""guide"", ""ally"", ""antagonist"")
    •   archetype: aligned with provided archetypes / core_axes
    •   species
    •   age
    •   traits: array of personality traits (e.g. [""curious"", ""kind"", ""cautious""])
    •   backstory: short, age-appropriate background and motivation
SCENES
Each story is a set of modular scenes of type: ""narrative"", ""choice"", ""roll"", or ""special"".
All scenes must follow:
    •   id: string, lowercase snake_case, unique.
    •   title: short, descriptive, age-appropriate.
    •   type: ""narrative"" | ""choice"" | ""roll"" | ""special"".
    •   description: clear, engaging, age-appropriate player-facing text.
    •   media (optional): image/audio/video URLs for some scenes.
    •   developmental metadata (optional, but highly important, and recommended for key moments):
        o   compass_change: how core_axes change.
Scene variety
        •   Distribute scene types across narrative, choice, and roll; avoid clustering all choices or all rolls.
        •   ""special"" scenes are mainly for endings or big reveals and should be used sparingly.

STRUCTURE AND BRANCHING
Scene count
    •   Treat minScenes / maxScenes as an important soft constraint.
    •   Generate {minScenes}–{maxScenes} scenes.
    •   Small deviations (~±25%) are acceptable for coherence, but never output drastically fewer scenes (e.g. around half or less than requested).
Overall graph
    •   The story must form a coherent scene graph with multiple possible endings.
    •   All paths must lead to one of the terminal endings.
    •   Scene flow must be logical and chronological for the story context: no unexplained time jumps or teleporting between locations without clear narrative justification.
    •   NPCs (non-player characters) may be introduced in any scene, but they must not be referenced, spoken to, or relied on in earlier scenes before they have appeared and been clearly introduced.
Endings
    •   All final/ending scenes must be of type ""special"".
    •   Ending ""special"" scenes:
        o   Have no outgoing transitions: next_scene omitted or null.
        o   Must not have branches that continue the story.
Scene-type-specific rules
Narrative (""narrative""):
    •   Used to move the story forward without a choice.
    •   Must have a next_scene pointing to a valid scene.
    •   Must not be used for final endings.
Choice (""choice""):
    •   Must have a branches array with at least two options.
    •   Each branch includes:
        o   A clear player-facing choice description.
        o   A next_scene id.
Roll (""roll""):
    •   Must have roll_requirements describing the mechanic (thresholds, difficulty) for a D20 dice.
    •   Must have a branches array with exactly two outcome branches, each with a next_scene id.
Special (""special""):
    •   Used for endings, major reveals, or meta moments.
    •   Ending specials: no further transitions (next_scene omitted or null).
    •   Non-ending specials may use next_scene but must keep story flow coherent.
Branch uniqueness (critical)
For every ""choice"" or ""roll"" scene:
    •   branches is required, with at least two entries.
    •   Within a single scene, each branch must have a unique next_scene id.
    •   No two branches from the same scene may point to the same next_scene.
General consistency
    •   All next_scene and branch next_scene targets must reference existing scenes.
    •   Avoid dead ends / orphan scenes.
    •   Maintain continuity of characters, locations, and goals; avoid contradictions without explanation.
SAFETY AND CHILD DEVELOPMENT (Critical)
    •   age_group must be one of: ""1-3"", ""4-5"", ""6-9"", ""10-12"", ""13-18"".
    •   Language, content, and themes must be age-appropriate for age_group and minimum_age.
    •   Forbidden: profanity, slurs, sexual content, self-harm, graphic violence, humiliation, cruelty-based humor, or ""punching down"".
    •   Mild peril is allowed but must resolve in emotionally safe ways.
FINAL OUTPUT RULES
    •   Output only:
        o   Metadata fields: title, description, tags, difficulty, session_length, age_group, minimum_age, core_axes, archetypes.
        o   A characters array with exactly character_count entries.
        o   A scenes array following all rules above.
    •   No extra top-level keys.
    •   No markdown, comments, or code fences.
    •   Return a single valid JSON object that fully respects all constraints.
    •   Character restrictions:
        o Never output control characters in the Unicode ranges U+0000–U+001F or U+007F–U+009F, except for standard whitespace characters: newline (\n), carriage return (\r), and tab (\t).
        o Use normal printable characters only. If you need quotes, use "" and ' instead of any special control codes.
";
    }

    private static List<MystiraChatMessage> BuildMessages(GenerateJsonStoryRequest request, string? userQuery,
        string? instructionBlock)
    {
        var messages = new List<MystiraChatMessage>();

        var exampleJson = GetShortExampleJson();
        messages.Add(new MystiraChatMessage
        {
            MessageType = ChatMessageType.AI,
            Content = "Example output (for structure only; do not copy content, follow structure and keys):\n\n" + exampleJson
        });

        if (!string.IsNullOrWhiteSpace(instructionBlock))
        {
            messages.Add(new MystiraChatMessage
            {
                MessageType = ChatMessageType.System,
                Content = instructionBlock
            });
        }

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
            Content = "Context parameters:\n" + jsonPayload
        });

        messages.Add(new MystiraChatMessage
        {
            MessageType = ChatMessageType.User,
            Content = "Where parameters are missing, infer them from the following user query, or the rest of the chat " +
                "context:" + userQuery ?? string.Empty
        });

        return messages;
    }

    private async Task<string?> ResolveInstructionBlockAsync(GenerateStoryCommand request, CancellationToken cancellationToken)
    {
        var queryText = "Generate a story ensuring that you include relevant requirements and guidelines, taking " +
                        "into account that the user provided the following prompt:\n" + request.UserQuery;
        if (string.IsNullOrWhiteSpace(queryText)) return null;

        var categories = new[] { "story_generation" };
        var instructionTypes = new[] { "story_generate_initial" };
        var ageGroup = request.Request?.AgeGroup;

        var intentClassification = await _llmIntentLlmClassificationService.ClassifyAsync(queryText, cancellationToken);
        if (intentClassification != null)
        {
            _logger.LogInformation(
                "Intent classified for story generation: category={Category}, instructionType={InstructionType}",
                intentClassification.Categories,
                intentClassification.InstructionTypes);

            categories = intentClassification.Categories;
            instructionTypes = intentClassification.InstructionTypes;
        }
        else
        {
            _logger.LogDebug("Using default categories and instruction types for story generation RAG query");
        }

        var context = new InstructionSearchContext
        {
            QueryText = queryText,
            Categories = categories,
            InstructionTypes = instructionTypes,
            TopK = 8,
            AgeGroup = ageGroup
        };

        return await _instructionBlockService.BuildInstructionBlockAsync(context, cancellationToken);
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
            var json = _schemaProvider.GetSchemaJsonAsync().Result;
            if (!string.IsNullOrWhiteSpace(json))
            {
                return new JsonSchemaResponseFormat
                {
                    FormatName = "mystira-story-generated",
                    SchemaJson = json,
                    IsStrict = _schemaProvider.IsStrict
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
