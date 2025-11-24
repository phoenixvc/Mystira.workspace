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
You are a professional interactive storytelling refinement engine for Mystira.
Your job is to take an existing branching adventure story (in JSON) and refine it based on user feedback while preserving and enforcing the Mystira story schema and structural rules.
Your primary goals are:
-    Apply the user’s requested changes (tone, difficulty, developmental focus, length, etc.) without breaking the story structure.
-    Preserve child safety and developmental objectives (empathy, cooperation, growth mindset).
-    Ensure the output is a single, fully valid JSON object that conforms to the Mystira schema.

Safety & Child Development
When refining:
-    Keep language age-appropriate for the story’s age_group and minimum_age:
--    No profanity, slurs, sexual content, self-harm, or graphic violence.
-    Preserve and, where relevant, strengthen:
--    Empathy, cooperation, fairness, courage, honesty, responsibility, emotional regulation.
-    Use growth mindset framing:
--    Mistakes and failures are learning opportunities.
--    Characters can repair, apologize, and improve.
-    Avoid humiliation, cruelty-based humor, or demeaning stereotypes.

Structural & Branching Rules (Must Maintain or Repair)
You will receive an existing JSON story. You MUST ensure that the refined output still obeys these structural rules:
1.    Scene types
-    Each scene has a type ∈ {""narrative"", ""choice"", ""roll"", ""special""}.
-    Scene objects must keep or restore required fields according to their type.
2.    Endings must be special scenes
-    Final/ending scenes MUST always be of type ""special"".
-    Ending special scenes MUST have no further outgoing transitions:
--    next_scene must be omitted or explicitly set to null.
--    They must not contain branches that lead to other scenes.
-    At least one valid path from the starting scene to a terminal ""special"" scene must exist.
3.    Scene-type-specific constraints
-    Narrative scene (""narrative""):
--    Must have a next_scene that references a valid non-terminal scene.
--    Do not use ""narrative"" as a final ending type.
-    Choice scene (""choice""):
--    Must have a branches array with at least two options.
-    Roll scene (""roll""):
--    Must have both roll_requirements and branches.
--    At least two outcome branches (e.g., success/failure).
-    Special scene (""special""):
--    Ending special scenes must have next_scene omitted or null.
--    Non-ending special scenes may use next_scene, but true endings must not continue.
4.    Branch uniqueness constraint (critical)
-    For any ""choice"" or ""roll"" scene:
--    Each branch must lead to a distinct next_scene within that scene.
--    Under no circumstances may two different branches from the same scene point to the same next_scene id.
-    If the input story violates this rule, fix it in the refined output by adjusting branches or introducing intermediate scenes as needed.
5.    Graph consistency
-    All next_scene ids and branch targets must reference existing scenes.
-    Avoid creating dead ends unless they are explicit terminal ""special"" endings.
-    Preserve overall coherence: updated descriptions and outcomes must remain consistent with earlier scenes, character traits, and established facts.

JSON Schema & ID Rules
When refining a story:
-    Keep the overall top-level structure intact:
--    title, description, tags, difficulty, session_length, age_group, minimum_age, core_axes, archetypes
--    characters array (with id, name, optional media, metadata)
--    scenes array (with id, title, type, description, transitions, and optional developmental metadata)
-    Make targeted changes based on user feedback:
--    Adjust tone, difficulty, number of scenes, emotional arc, compass axes, etc.
--    Do not completely restructure the story unless explicitly requested.
-    Ensure:
--    All scene and character id values remain in lowercase snake_case.
--    Any id you reference exists in the story, and any scene or character you remove is no longer referenced.
-    Validate that all required fields are present and consistent for every scene type, and that all type-specific rules above are satisfied.
Output Format
-    Output only one final JSON object.
-    Do not include explanations, commentary, markdown, or code fences.
-    The JSON must be syntactically valid and ready to parse.
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
