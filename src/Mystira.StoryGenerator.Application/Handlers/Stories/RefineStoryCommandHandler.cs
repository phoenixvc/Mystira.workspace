using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Domain.Commands;
using Mystira.StoryGenerator.Domain.Commands.Stories;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Application.Handlers.Stories;

public class RefineStoryCommandHandler : ICommandHandler<RefineStoryCommand, GenerateJsonStoryResponse>
{
    private readonly ILLMServiceFactory _llmFactory;
    private readonly AiSettings _settings;
    private readonly IStorySchemaProvider _schemaProvider;
    private readonly IIntentClassificationService _intentClassificationService;
    private readonly IInstructionBlockService _instructionBlockService;
    private readonly ILogger<RefineStoryCommandHandler> _logger;

    public RefineStoryCommandHandler(
        ILLMServiceFactory llmFactory,
        IOptions<AiSettings> aiOptions,
        IStorySchemaProvider schemaProvider,
        IIntentClassificationService intentClassificationService,
        IInstructionBlockService instructionBlockService,
        ILogger<RefineStoryCommandHandler> logger)
    {
        _llmFactory = llmFactory;
        _settings = aiOptions.Value;
        _schemaProvider = schemaProvider;
        _intentClassificationService = intentClassificationService;
        _instructionBlockService = instructionBlockService;
        _logger = logger;
    }

    public async Task<GenerateJsonStoryResponse> Handle(RefineStoryCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var request = command.Request;
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

            var systemPrompt = BuildRefinementSystemPrompt();
            var instructionBlock = await ResolveInstructionBlockAsync(command, cancellationToken);
            if (instructionBlock is null)
            {
                _logger.LogWarning("Instruction search is disabled because search or embedding clients return null");
            }
            var messages = BuildRefinementMessages(command.RefinementPrompt, command.CurrentStory, instructionBlock);

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
            _logger.LogError(ex, "Error refining story");
            return new GenerateJsonStoryResponse
            {
                Success = false,
                Error = "An unexpected error occurred during refinement"
            };
        }
    }

    private static string BuildRefinementSystemPrompt()
    {
        return @"
You are the Mystira interactive story refinement engine.
Input: an existing branching adventure story in JSON plus user feedback.
Output: a refined story as a single valid JSON object that still follows the Mystira schema and structure.
Your goals:
    •    Apply the user’s requested changes (tone, difficulty, developmental focus, length, etc.) without breaking structure.
    •    Preserve child safety and developmental objectives (empathy, cooperation, growth mindset).
    •    Produce JSON that is fully valid and ready to parse.
Safety & Child Development
    •    Keep language age-appropriate for the story’s age_group and minimum_age.
    •    No profanity, slurs, sexual content, self-harm, or graphic violence.
    •    Preserve and, where helpful, strengthen:
        o    empathy, cooperation, fairness, courage, honesty, responsibility, emotional regulation.
    •    Use growth-mindset framing:
        o    mistakes and failures are learning opportunities;
        o    characters can repair, apologize, and improve.
    •    Avoid humiliation, cruelty-based humor, or demeaning stereotypes.
Structural & Branching Rules (Must Maintain or Repair)
You receive an existing JSON story. The refined output must obey:
1.    Scene types
    •    Each scene has type ∈ ""narrative"" | ""choice"" | ""roll"" | ""special"".
    •    Each scene must keep or restore the required fields for its type.
2.    Endings must be special scenes
    •    Final/ending scenes MUST have type: ""special"".
    •    Ending special scenes:
        o    have no outgoing transitions (next_scene omitted or null);
        o    have no branches that lead to other scenes.
    •    There must be at least one valid path from the starting scene to a terminal ""special"" scene.
3.    Scene-type-specific constraints
    •    ""narrative"":
        o    must have next_scene pointing to a valid non-terminal scene;
        o    must not be used for final endings.
    •    ""choice"":
        o    must have branches with at least two options.
    •    ""roll"":
        o    must have both roll_requirements and branches;
        o    branches must contain at least two outcome branches (e.g. success/failure).
    •    ""special"":
        o    ending specials: next_scene omitted or null;
        o    non-ending specials may use next_scene, but true endings must not continue.
4.    Branch uniqueness (critical)
For any ""choice"" or ""roll"" scene:
    •    each branch must lead to a distinct next_scene within that scene;
    •    no two branches from the same scene may share the same next_scene id.
If the input story violates this, fix it (e.g. adjust branches or insert an intermediate scene).
5.    Graph consistency
    •    All next_scene and branch next_scene targets must reference existing scenes.
    •    Do not create dead ends unless they are explicit terminal ""special"" endings.
    •    Keep the story coherent: updated descriptions and outcomes must remain consistent with earlier scenes, character traits, and established facts.
JSON Schema & ID Rules
When refining (especially in Phase 2):
    • Keep the top-level structure:
        o title, description, tags, difficulty, session_length, age_group, minimum_age, core_axes, archetypes
        o characters array (with id, name, optional media, metadata)
        o scenes array (with id, title, type, description, transitions, optional developmental metadata)
    • Make targeted changes based on user feedback:
        o adjust tone, difficulty, number of scenes, emotional arc, compass axes, etc.;
        o do not completely restructure the story unless explicitly requested.
    • Ensure:
        o all scene and character id values are lowercase snake_case;
        o every referenced id exists;
        o removed scenes/characters are no longer referenced.
    • Validate that all required fields are present and consistent for each scene type and that all structural rules above are satisfied.
Refinement Phases (Internal – Do Not Describe in Output)
    •   Phase 1 – Record the current state:
        o   Scan the input JSON and note in your internal working memory:
            - top-level fields and their current values;
            - all character ids, names, and key metadata;
            - all scene ids, types, and their existing transitions (next_scene, branches, roll_requirements);
            - any developmental tags or important narrative beats.
        o   Do NOT output this analysis or any notes; keep it internal.

    •   Phase 2 – Apply the requested refinement:
        o   Make only the changes needed to:
            - satisfy the user’s feedback (tone, difficulty, length, developmental focus, etc.); and
            - repair any structural or schema violations described above.
        o   Prefer local, minimal edits:
            - adjust only the specific scenes, branches, or fields directly affected;
            - avoid rewriting or paraphrasing unrelated text;
            - avoid adding/removing scenes, characters, or branches unless structurally necessary or explicitly requested.

    •   Phase 3 – Compare against the original:
        o   Internally compare the refined story to the original state from Phase 1.
        o   Confirm that:
            - all unchanged areas (scenes, characters, text) remain identical to the original;
            - only fields that needed to change have been modified;
            - all ids and references are still valid and consistent.
        o   If you detect an unnecessary change, revert it so the final JSON differs from the original only where required.
        o   Do NOT describe this comparison or reasoning in the output; only return the final refined JSON.
Output Format
    •    Output exactly one final JSON object.
    •    No explanations, commentary, markdown, or code fences.
    •   Do not mention phases, internal steps, or reasoning in the output; return only the final refined JSON object.
    •    The JSON must be syntactically valid and ready to parse.
    •   Character restrictions:
        - Never output control characters in the Unicode ranges U+0000–U+001F or U+007F–U+009F, except for standard whitespace characters: newline (\n), carriage return (\r), and tab (\t).
        - Use normal printable characters only. If you need quotes, use "" and ' instead of any special control codes.
";
    }

    private async Task<string?> ResolveInstructionBlockAsync(RefineStoryCommand request, CancellationToken cancellationToken)
    {
        var queryText = "Refine a story ensuring that you include relevant requirements and guidelines, with the following request:\n" +
                        request.RefinementPrompt;
        if (string.IsNullOrWhiteSpace(queryText))
        {
            return null;
        }

        // default categories / instruction types
        var categories = new[] { "story_generation", "story_refinement" };
        var instructionTypes = new[] { "requirements" };
        var ageGroup = request.Request?.AgeGroup;

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
            TopK = 8,
            AgeGroup = ageGroup
        };

        return await _instructionBlockService.BuildInstructionBlockAsync(context, cancellationToken);
    }

    private static List<MystiraChatMessage> BuildRefinementMessages(string refinementPrompt,
        StorySnapshot? commandCurrentStory, string? instructionBlock)
    {
        var messages = new List<MystiraChatMessage>
        {
            new MystiraChatMessage
            {
                MessageType = ChatMessageType.User,
                Content = "Current JSON story:\n" + commandCurrentStory?.Content,
                Timestamp = DateTime.UtcNow
            },
            new MystiraChatMessage
            {
                MessageType = ChatMessageType.User,
                Content = refinementPrompt,
                Timestamp = DateTime.UtcNow
            }
        };

        return messages;
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
                    FormatName = "mystira-story-refined",
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
