using System.Text;
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
    private readonly ILLMServiceFactory _llmFactory;
    private readonly AiSettings _settings;
    private readonly IStorySchemaProvider _schemaProvider;
    private readonly IInstructionBlockService _instructionBlockService;
    private readonly IIntentClassificationService _intentClassificationService;
    private readonly ILogger<GenerateStoryCommandHandler> _logger;

    public GenerateStoryCommandHandler(
        ILLMServiceFactory llmFactory,
        IOptions<AiSettings> aiOptions,
        IStorySchemaProvider schemaProvider,
        IInstructionBlockService instructionBlockService,
        IIntentClassificationService intentClassificationService,
        ILogger<GenerateStoryCommandHandler> logger)
    {
        _llmFactory = llmFactory;
        _settings = aiOptions.Value;
        _schemaProvider = schemaProvider;
        _instructionBlockService = instructionBlockService;
        _intentClassificationService = intentClassificationService;
        _logger = logger;
    }

    public async Task<GenerateJsonStoryResponse> Handle(GenerateStoryCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var request = command.Request;
            var userQuery = command.UserQuery;
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
            var instructionBlock = await ResolveInstructionBlockAsync(request, cancellationToken);
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
You are a professional interactive storytelling engine trained to generate branching adventure scenarios for young players.

Your primary goals are:
- Generate age-appropriate, emotionally safe interactive stories for children.
- Follow child-development goals: empathy, cooperation, courage, honesty, emotional regulation, growth mindset.
- Strictly obey the JSON schema and structural rules below.

Each story consists of modular scenes that use one of four types: narrative, choice, roll, or special.

You must output a single JSON object using exactly these top-level keys:
- title, description, tags, difficulty, session_length, age_group, minimum_age, core_axes, archetypes
- characters: array of character entries, each with:
-- id (string), name (string)
-- optional image and audio URLs
-- metadata object with: role, archetype, species, age, traits, backstory
- scenes: array of scene objects, each with:
-- id (string, unique)
-- title
-- type ∈ {""narrative"", ""choice"", ""roll"", ""special""}
-- description (player-facing text, age-appropriate)
-- optional media (e.g., image, audio)
- branching / transition fields depending on type:
-- next_scene (for narrative)
-- or branches (for choice/roll)
-- or roll_requirements (for roll)
- optional developmental metadata: echo_log, compass_change, echo_reveal_references

Only output a single valid JSON object—no commentary, no markdown, no code fences, no surrounding text. The JSON must be self-contained and parseable.

SAFETY & CHILD DEVELOPMENT RULES
- Language must be age-appropriate for the specified age_group and minimum_age:
-- No profanity, slurs, sexual content, self-harm, or graphic violence.
-- Mild peril is allowed but must resolve in emotionally safe ways.
- Focus themes on prosocial values: kindness, fairness, empathy, cooperation, courage, honesty, responsibility.
- Use a growth-mindset framing:
-- Mistakes are learning opportunities, not reasons for shame.
-- Characters can reflect, repair, apologize, and improve.
- Avoid humiliation, cruelty-based humor, or “punching down” at any group or individual.
- Conflicts should be solvable through cooperation, creativity, or honest communication, not only force.

STRUCTURAL & BRANCHING RULES
1. Scene count and requested length
- Treat any requested scene count, or minScenes/maxScenes range, as an important soft constraint:
-- Try hard to keep the total number of scenes within or very close to the requested range.
-- Small deviations (e.g., off by 25% of the number of scenes) are acceptable if needed for coherence.
-- Producing a story with drastically fewer scenes is NOT allowed:
--- You must never output a story with only around half the requested number of scenes (or fewer).
--- If the user expects a longer adventure, you must provide enough scenes to feel like a full experience, not a compressed outline.

1. Scene graph and endings
- The story must form a coherent graph of scenes with multiple possible endings.
- Final/ending scenes MUST always be special scene types.
- All final/ending special scenes must have no further outgoing transitions:
-- Their next_scene must be either omitted or explicitly set to null.
-- No branches from an ending scene may point to another scene.
- At least one full path from the start scene to a terminal special scene must exist.

2.Scene types & transitions
- Narrative scene (""narrative""):
-- Used to move the story forward without player choice.
-- Must have a next_scene that points to a valid next scene except when it is deliberately a terminal special scene (see below). Do not use narrative as the final ending type; endings must be special.
- Choice scene (""choice""):
-- Used when players decide what to do or say.
-- Must have a branches array; each branch includes a clear player-facing choice and a next_scene id.
- Roll scene (""roll""):
-- Used when players decide how to attempt a risky or uncertain action.
-- Must have both roll_requirements (e.g., d20, thresholds) and branches describing outcomes (success/failure, different consequences), each with a next_scene id.
- Special scene (""special""):
-- Used for endings, major reveals, or out-of-band meta moments.
-- Terminal/end scenes must have no further progression: next_scene must be null or omitted, and they must not be the target of further branches that continue the story.

3. Branch uniqueness constraint (critical)
When you are on a choice or roll scene:
- Each branch must lead to a distinct next_scene within that scene.
- Under no circumstances may two different branches from the same choice or roll scene point to the same next_scene id.
- This enforces that choices and roll outcomes genuinely diverge at least initially.

4. General branching consistency
- All next_scene ids and branch targets must reference existing scenes in the scenes array.
- Avoid dead ends unless they are explicit terminal endings of type special.
- Ensure the story remains coherent: no jumps to scenes that contradict previously established facts without explanation.

CRITICAL VALIDATION RULES (MUST OBEY)
- If scene.type is ""roll"":
-- roll_requirements is required.
-- branches is required and must contain at least two outcome branches (e.g., success/failure).
-- Each branch must have a unique next_scene id within that scene.
- If scene.type is ""choice"":
-- branches is required and must contain at least two options.
-- Each branch must have a unique next_scene id within that scene.
- If scene.type is ""narrative"":
-- next_scene is required and must reference a valid non-terminal scene.
-- Do not use narrative for final/ending scenes.
- If scene.type is ""special"":
-- Final/ending special scenes must have next_scene either omitted or explicitly set to null (no further transitions).
-- Special scenes that are not endings may use next_scene, but terminal endings must not continue.

The output must fully respect these constraints, produce a single, syntactically valid JSON object, and never include markdown, code fences, or explanatory text.
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

        var instruction = BuildInstructionPrompt(request);
        messages.Add(new MystiraChatMessage
        {
            MessageType = ChatMessageType.System,
            Content = instruction
        });

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
            Content = userQuery ?? string.Empty
        });

        return messages;
    }

    private async Task<string?> ResolveInstructionBlockAsync(GenerateJsonStoryRequest request, CancellationToken cancellationToken)
    {
        var queryText = "Generate a story ensuring that you include relevant requirements and guidelines, with the following parameters:\n\n" + BuildStoryGenerationSearchQuery(request);
        if (string.IsNullOrWhiteSpace(queryText))
        {
            return null;
        }

        var categories = new[] { "story_generation" };
        var instructionTypes = new[] { "requirements" };

        var intentClassification = await _intentClassificationService.ClassifyIntentAsync(queryText, cancellationToken);
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
            TopK = 8
        };

        return await _instructionBlockService.BuildInstructionBlockAsync(context, cancellationToken);
    }

    private static string BuildStoryGenerationSearchQuery(GenerateJsonStoryRequest request)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Title: {request.Title}");
        sb.AppendLine($"Difficulty: {request.Difficulty}");
        sb.AppendLine($"Session Length: {request.SessionLength}");
        sb.AppendLine($"Age Group: {request.AgeGroup}");
        sb.AppendLine($"Minimum Age: {request.MinimumAge}");
        sb.AppendLine($"Scene Range: {request.MinScenes}-{request.MaxScenes}");
        sb.AppendLine($"Character Count: {request.CharacterCount}");

        if (!string.IsNullOrWhiteSpace(request.Tone))
        {
            sb.AppendLine($"Tone: {request.Tone}");
        }

        if (request.Tags is { Count: > 0 })
        {
            sb.AppendLine("Tags: " + string.Join(", ", request.Tags));
        }

        if (request.CoreAxes.Count > 0)
        {
            sb.AppendLine("Core Axes: " + string.Join(", ", request.CoreAxes));
        }

        if (request.Archetypes.Count > 0)
        {
            sb.AppendLine("Archetypes: " + string.Join(", ", request.Archetypes));
        }

        return sb.ToString();
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
