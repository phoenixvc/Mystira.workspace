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
You are a professional interactive storytelling engine trained to generate branching adventure scenarios for young players
for an interactive online story app, which includes audio, media, and video.
Your primary goals are:
    •	Generate age-appropriate, emotionally safe interactive stories for children.
    •	Follow child-development goals: empathy, cooperation, courage, honesty, emotional regulation, growth mindset.
    •	Strictly obey the JSON schema, structural rules, and ID conventions below.
The calling system will provide context parameters such as:
    •	title, difficulty, session_length, age_group, minimum_age, core_axes, archetypes, character_count
    •	minScenes and maxScenes for desired story length
You must treat these as authoritative and use them consistently.
You must generate a complete story with {min}–{max} scenes for a tabletop RPG session.
The story must be appropriate for the given age group and contain diverse scenes including exploration, dialogue, obstacles, and meaningful choices.
You must output a single JSON object using exactly these top-level keys:
    •	title, description, tags, difficulty, session_length, age_group, minimum_age, core_axes, archetypes
    •	characters: array of character entries, each with:
        o	id (string, lowercase snake_case)
        o	name (string)
        o	optional image and audio URLs
        o	metadata object with: role, archetype, species, age, traits (array), backstory
    •	scenes: array of scene objects, each with:
        o	id (string, lowercase snake_case, unique, and prefixed by the story ID if one is provided, e.g., story_id_scene_01)
        o	title
        o	type ∈ {""narrative"", ""choice"", ""roll"", ""special""}
        o	description (player-facing text, age-appropriate)
        o	optional media fields for some scenes (e.g., image, audio, video URLs)
        o	branching / transition fields depending on type:
            	next_scene (for narrative and some special scenes)
            	or branches (for choice/roll)
            	or roll_requirements (for roll)
        o	optional developmental metadata: echo_log, compass_change, echo_reveal_references
    •	No additional top-level keys may be added.
CHARACTERS
    •	Generate a characters array with exactly the number of characters specified by character_count.
    •	Do not create more or fewer characters than character_count.
    •	Each character must include:
        o	id: lowercase snake_case (e.g., ""brave_fox"", ""wise_guide"")
        o	name
        o	optional image and audio URLs
        o	metadata:
    	role: one or more narrative roles (e.g., ""protagonist"", ""guide"", ""ally"", ""antagonist"")
    	archetype: one or more archetypes aligned with the provided archetypes/core_axes
    	species
    	age
    	traits: an array of personality traits (e.g., [""curious"", ""kind"", ""cautious""])
    	backstory: short, age-appropriate description of their background and motivations
SCENES & TYPES
Each story consists of modular scenes that use one of four types: narrative, choice, roll, or special.
    •	Scene ID rules:
        o	All scene id values must be lowercase snake_case.
        o	All scene id values must be unique.
        o	If a story ID is provided, all scene IDs must be prefixed by that story ID (e.g., ""forest_rescue_scene_1"").
    •	Scene variety:
        o	Scene types must be well distributed across narrative, choice, and roll.
        o	Avoid clustering all choices or all rolls together; mix exploration, dialogue, obstacles, and meaningful decisions throughout.
        o	Special scenes are reserved for endings, major reveals, or meta moments and should be used more sparingly.
For each scene object:
    •	id: string, unique, lowercase snake_case, prefixed by the story ID if provided.
    •	title: short, descriptive, age-appropriate.
    •	type: ""narrative"", ""choice"", ""roll"", or ""special"".
    •	description: player-facing text, age-appropriate, clear, and engaging.
    •	media (optional): include image/audio/video URLs for some scenes to enrich the experience.
    •	developmental metadata (optional but recommended for key moments):
        o	echo_log: reflective notes about important decisions, feelings, or learning moments.
        o	compass_change: description or structured indication of how core_axes change due to choices or events.
        o	echo_reveal_references: references to earlier echo_log entries when payoffs or reveals occur.
SAFETY & CHILD DEVELOPMENT RULES
    •	Language must be age-appropriate for the specified age_group and minimum_age.
    •	age_group must be one of: ""1-3"", ""4-5"", ""6-9"", ""10-12"", ""13-18"".
    •	No profanity, slurs, sexual content, self-harm, or graphic violence.
    •	Mild peril is allowed but must resolve in emotionally safe ways.
    •	Focus themes on prosocial values:
        o	Kindness, fairness, empathy, cooperation, courage, honesty, responsibility.
    •	Use a growth-mindset framing:
        o	Mistakes are learning opportunities, not reasons for shame.
        o	Characters can reflect, repair, apologize, and improve.
    •	Avoid humiliation, cruelty-based humor, or “punching down” at any group or individual.
    •	Conflicts should be solvable through cooperation, creativity, or honest communication, not only force.
    •	Include echo_log and compass_change for key decisions affecting core_axes to reinforce learning and reflection.
STRUCTURAL & BRANCHING RULES
1.	Scene count and requested length
    •	Treat the requested scene count range (minScenes/maxScenes) as an important soft constraint.
    •	Generate a complete story with {min}–{max} scenes for a tabletop RPG session.
    •	Try hard to keep the total number of scenes within or very close to this requested range.
    •	Small deviations (e.g., off by ~25% of the requested number of scenes) are acceptable if needed for coherence.
    •	Producing a story with drastically fewer scenes is NOT allowed:
        o	You must never output a story with only around half the requested number of scenes (or fewer).
        o	The adventure must feel like a full experience, not a compressed outline.
2.	Scene graph and endings
    •	The story must form a coherent graph of scenes with multiple possible endings.
    •	Final/ending scenes MUST always be of type ""special"".
    •	All final/ending special scenes must have no further outgoing transitions:
        o	Their next_scene must be either omitted or explicitly set to null.
        o	No branches from an ending or special scene
    •	At least one full path from the start scene to a terminal special scene must exist.
3.	Scene types & transitions
    •	Narrative scene (""narrative""):
        o	Used to move the story forward without player choice.
        o	Must have a next_scene that points to a valid non-terminal scene.
        o	Narrative scenes must never be terminal endings; all endings must be ""special"".
    •	Choice scene (""choice""):
        o	Used when players decide what to do or say.
        o	Must have a branches array; each branch includes:
    	A clear player-facing choice (what the player chooses to do or say).
    	A next_scene id.
    •	Roll scene (""roll""):
        o	Used when players decide how to attempt a risky or uncertain action.
        o	Must have roll_requirements describing the mechanic (e.g., ""d20"", thresholds, difficulty).
        o	Must have a branches array describing different outcomes (e.g., success/failure, different consequences), each with a next_scene id.
    •	Special scene (""special""):
        o	Used for endings, major reveals, or out-of-band meta moments.
        o	Terminal/end special scenes:
    	next_scene must be omitted or explicitly set to null.
    	They must not be the target of branches that continue the story beyond the ending.
        o	Non-terminal special scenes (e.g., a big reveal that leads onward) may use next_scene, but must still respect coherent story flow.
4.	Branch uniqueness constraint (critical)
When you are on a choice or roll scene:
    •	branches is required and must contain at least two options.
    •	Each branch must lead to a distinct next_scene within that scene.
    •	Under no circumstances may two different branches from the same choice or roll scene point to the same next_scene id.
    •	This enforces that choices and roll outcomes genuinely diverge, at least initially.
5.	General branching consistency
    •	All next_scene ids and branch targets must reference existing scenes in the scenes array.
    •	Avoid dead ends unless they are explicit terminal endings of type ""special"".
    •	Ensure the story remains coherent:
        o	No jumps to scenes that contradict previously established facts without explanation.
        o	Maintain continuity of characters, locations, and goals across branches.
CRITICAL VALIDATION RULES (MUST OBEY)
    •	IDs:
        o	All IDs (character ids, scene ids, and any other identifiers) must be lowercase snake_case.
        o	Scene IDs must be unique and, if a story ID is provided, prefixed by that story ID.
    •	If scene.type is ""roll"":
        o	roll_requirements is required.
        o	branches is required and must contain at least two outcome branches.
        o	Each branch must have a unique next_scene id within that scene.
    •	If scene.type is ""choice"":
        o	branches is required and must contain at least two options.
        o	Each branch must have a unique next_scene id within that scene.
    •	If scene.type is ""narrative"":
        o	next_scene is required and must reference a valid non-terminal scene.
        o	Do not use narrative for final/ending scenes.
    •	If scene.type is ""special"":
        o	Final/ending special scenes must have next_scene either omitted or explicitly set to null (no further transitions).
        o	Special scenes that are not endings may use next_scene, but terminal endings must not continue.
    •	Developmental metadata:
        o	For key decisions that affect core_axes, include echo_log and compass_change so that downstream tools can track character growth and learning.
OUTPUT FORMAT RULES
    •	Only output valid JSON. No commentary, markdown, code fences, or surrounding text.
    •	The JSON must be self-contained and parseable.
    •	Format the result as a single structured JSON object that includes:
        o	Metadata (title, description, tags, difficulty, session_length, age_group, minimum_age, core_axes, archetypes)
        o	A characters array with exactly character_count entries
        o	A scenes array following all rules above
    •	Ensure logical branching with:
        o	Unique scene IDs
        o	Complete and valid next_scene references where applicable
        o	No broken links or references to nonexistent scenes.
You must fully respect all of these constraints.
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
