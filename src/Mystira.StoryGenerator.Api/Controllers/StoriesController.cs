using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Api.Services;
using Mystira.StoryGenerator.Api.Services.LLM;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Domain.Commands.Stories;
using System.Text.Json;

namespace Mystira.StoryGenerator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoriesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IStoryValidationService _validationService;
    private readonly ILLMServiceFactory _llmFactory;
    private readonly AiSettings _aiSettings;
    private readonly IStorySchemaProvider _schemaProvider;
    private readonly ILogger<StoriesController> _logger;

    public StoriesController(
        IMediator mediator,
        IStoryValidationService validationService,
        ILLMServiceFactory llmFactory,
        IStorySchemaProvider schemaProvider,
        IOptions<AiSettings> aiOptions,
        ILogger<StoriesController> logger)
    {
        _mediator = mediator;
        _validationService = validationService;
        _llmFactory = llmFactory;
        _schemaProvider = schemaProvider;
        _aiSettings = aiOptions.Value;
        _logger = logger;
    }

    [HttpPost("validate")]
    public async Task<ActionResult<ValidationResponse>> ValidateStory([FromBody] ValidateStoryRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.StoryContent))
            {
                return BadRequest(new ValidationResponse
                {
                    IsValid = false,
                    Errors = new List<ValidationIssue>
                    {
                        new ValidationIssue
                        {
                            Path = "storyContent",
                            Message = "Story content is required"
                        }
                    }
                });
            }

            var command = new ValidateStoryCommand(request);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating story");
            return StatusCode(500, new ValidationResponse
            {
                IsValid = false,
                Errors = new List<ValidationIssue>
                {
                    new ValidationIssue
                    {
                        Path = "internal",
                        Message = "An internal error occurred during validation"
                    }
                }
            });
        }
    }

    [HttpPost("setup")]
    public async Task<ActionResult<ChatCompletionResponse>> Setup([FromBody] ChatCompletionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (request == null || request.Messages == null || request.Messages.Count == 0)
            {
                return BadRequest(new ChatCompletionResponse
                {
                    Success = false,
                    Error = "At least one message is required"
                });
            }

            var service = !string.IsNullOrWhiteSpace(request.Provider)
                ? _llmFactory.GetService(request.Provider!)
                : _llmFactory.GetDefaultService();

            if (service == null)
            {
                return StatusCode(503, new ChatCompletionResponse
                {
                    Success = false,
                    Error = "No LLM services are currently available"
                });
            }

            var setupPrompt = @"You are the Mystira Story Generator Orchestrator. Your job is to help the user create a JSON-formatted branching adventure story for young players.

    Conversation flow requirements:
    1. When the user signals they want to start a new Mystira story, immediately ask for a short description or theme. Ask this in a single concise question and wait for their reply.
    2. After the user provides a theme or description (and any optional details), create a complete proposal covering every required parameter. Present the parameters in a friendly list for review and clearly label each field.
    3. After showing the list, ask the user if they would like to change anything or if they are ready for you to generate the story. Do NOT output PARAMS_JSON yet.
    4. When the user requests a change to a specific named parameter, modify only that parameter. Keep all other values exactly the same unless the user explicitly mentions them. Restate the full parameter list with the updated value and immediately ask if they want you to generate or regenerate the story. Do not ask about any other parameters.
    5. Only when the user clearly confirms (phrases like ""generate"", ""regenerate"", ""looks good"", ""yes"", ""ready"") that you should proceed may you produce the final payload.
    6. When you produce the final payload you may acknowledge their confirmation, then output exactly one line in this format with no extra commentary before or after it:
    PARAMS_JSON: {""title"": ""..."", ""difficulty"": ""..."", ""session_length"": ""..."", ""age_group"": ""..."", ""minimum_age"": 10, ""core_axes"": [""..."", ""...""], ""archetypes"": [""..."", ""...""], ""character_count"": 4, ""min_scenes"": 6, ""max_scenes"": 12, ""tags"": [""..."", ""...""], ""tone"": ""..."", ""description"": ""...""}
    7. After you output PARAMS_JSON, stop and wait silently for the system to generate JSON.

    Required parameters:
    - Title (or infer from the theme; keep it concise)
    - Difficulty: Easy, Medium, or Hard
    - Session Length: Short, Medium, or Long
    - Age Group: a numeric range such as ""6-9""
    - Minimum Age: an integer (use the lower bound of Age Group if unspecified)
    - Core Axes: at least two moral or character axes
    - Archetypes / Character count: at least two archetypes suitable for young players; ensure character_count matches the archetype count when not otherwise specified
    - Min and Max scenes: sensible integers where max_scenes >= min_scenes (default to 6-12 if the user does not care)
    - Tags (optional but helpful), Tone (optional but helpful), and a one-to-two sentence description that fits the theme

    General rules:
    - Stay positive, concise, and age-appropriate.
    - Reuse the previously confirmed parameter values unless the user changes them.
    - Treat ""generate"" and ""regenerate"" the same—both mean produce the final payload.
    - Keep follow-up questions minimal; only ask for clarification when absolutely necessary.";

            // Try to load the JSON Schema and configure schema-constrained output for providers that support it (Azure OpenAI)
            JsonSchemaResponseFormat? schemaFormat = null;
            try
            {
                var jsonSchema = await _schemaProvider.GetSchemaJsonAsync(cancellationToken);
                if (!string.IsNullOrWhiteSpace(jsonSchema))
                {
                    schemaFormat = new JsonSchemaResponseFormat
                    {
                        FormatName = "mystira-story-setup",
                        SchemaJson = jsonSchema!,
                        IsStrict = _schemaProvider.IsStrict
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load story schema for schema-constrained output; continuing without schema.");
            }

            var chatRequest = new ChatCompletionRequest
            {
                Provider = service.ProviderName,
                Model = string.IsNullOrWhiteSpace(request.Model) ? null : request.Model,
                Temperature = _aiSettings.DefaultTemperature,
                MaxTokens = Math.Max(1000, _aiSettings.DefaultMaxTokens),
                Messages = request.Messages,
                SystemPrompt = setupPrompt,
                JsonSchemaFormat = schemaFormat
            };

            var response = await service.CompleteAsync(chatRequest, cancellationToken);
            if (!response.Success)
            {
                return StatusCode(502, response);
            }

            return Ok(response);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, new ChatCompletionResponse
            {
                Success = false,
                Error = "Request was cancelled"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during story setup conversation");
            return StatusCode(500, new ChatCompletionResponse
            {
                Success = false,
                Error = "An unexpected error occurred"
            });
        }
    }

    [HttpPost("randomize")]
    public async Task<ActionResult<RandomStoryParametersResponse>> Randomize([FromBody] RandomStoryParametersRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var service = !string.IsNullOrWhiteSpace(request.Provider)
                ? _llmFactory.GetService(request.Provider!)
                : _llmFactory.GetDefaultService();

            if (service == null)
            {
                return StatusCode(503, new RandomStoryParametersResponse
                {
                    Success = false,
                    Error = "No LLM services are currently available"
                });
            }

            var systemPrompt = @"You are the Mystira Story Generator Randomizer. Create a complete, age-appropriate parameter set for a branching adventure story suitable for young players.

Return exactly ONE line in this format with no extra commentary or markdown:
RANDOM_PARAMS_JSON: {""title"":""..."",""difficulty"":""Easy|Medium|Hard"",""session_length"":""Short"",""age_group"":""6-9"",""minimum_age:6,""core_axes"":[""Honesty"",""Bravery""],""archetypes"":[""Explorer"",""Mystic""],""character_count"":3,""min_scenes"":6,""max_scenes"":12,""tags"":[""magic"",""friendship""],""tone"":""whimsical"",""description"":""A 1-2 sentence hook for the story.""}

Ensure content is culturally sensitive and age-appropriate.";


            var themeText = string.IsNullOrWhiteSpace(request.Theme) ? "Generate a random new Mystira story concept and parameters." : $"Generate a random new Mystira story concept and parameters based on the theme: '{request.Theme}'.";
            if (request.MinimumAge.HasValue)
            {
                themeText += $" Minimum age: {request.MinimumAge.Value}.";
            }
            if (!string.IsNullOrWhiteSpace(request.AgeGroup))
            {
                themeText += $" Target age group: {request.AgeGroup}.";
            }
            if (!string.IsNullOrWhiteSpace(request.Difficulty))
            {
                themeText += $" Difficulty: {request.Difficulty}.";
            }
            if (!string.IsNullOrWhiteSpace(request.SessionLength))
            {
                themeText += $" Session length: {request.SessionLength}.";
            }
            if (request.MinScenes.HasValue && request.MaxScenes.HasValue)
            {
                themeText += $" Scene range: {request.MinScenes.Value}-{request.MaxScenes.Value}.";
            }

            var chatRequest = new ChatCompletionRequest
            {
                Provider = service.ProviderName,
                Model = string.IsNullOrWhiteSpace(request.Model) ? null : request.Model,
                Temperature = Math.Max(0.8, _aiSettings.DefaultTemperature),
                MaxTokens = Math.Max(800, _aiSettings.DefaultMaxTokens),
                Messages = new List<MystiraChatMessage>
                {
                    new MystiraChatMessage
                    {
                        MessageType = ChatMessageType.User,
                        Content = themeText,
                        Timestamp = DateTime.UtcNow
                    }
                },
                SystemPrompt = systemPrompt
            };

            var response = await service.CompleteAsync(chatRequest, cancellationToken);
            if (!response.Success || string.IsNullOrWhiteSpace(response.Content))
            {
                return StatusCode(502, new RandomStoryParametersResponse
                {
                    Success = false,
                    Error = response.Error ?? "LLM returned empty content"
                });
            }

            var content = response.Content;
            var json = ExtractJson(content, "RANDOM_PARAMS_JSON:");
            if (string.IsNullOrWhiteSpace(json))
            {
                json = content.Trim();
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            RandomParamsPayload? payload = null;
            try
            {
                payload = JsonSerializer.Deserialize<RandomParamsPayload>(json, options);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse RANDOM_PARAMS_JSON: {Content}", content);
            }

            if (payload == null)
            {
                return StatusCode(502, new RandomStoryParametersResponse
                {
                    Success = false,
                    Error = "Failed to parse randomized parameters"
                });
            }

            var normalized = new RandomStoryParametersResponse
            {
                Success = true,
                Provider = response.Provider ?? service.ProviderName,
                Model = response.Model ?? (request.Model ?? string.Empty),
                Title = payload.title ?? "Untitled Adventure",
                Difficulty = NormalizeDifficulty(payload.difficulty),
                SessionLength = NormalizeSessionLength(payload.session_length),
                AgeGroup = payload.age_group ?? string.Empty,
                MinimumAge = payload.minimum_age > 0 ? payload.minimum_age : InferMinimumAgeFromAgeGroup(payload.age_group),
                CoreAxes = payload.core_axes ?? new List<string>(),
                Archetypes = payload.archetypes ?? new List<string>(),
                CharacterCount = payload.character_count > 0 ? payload.character_count : (payload.archetypes?.Count ?? 0),
                MinScenes = payload.min_scenes > 0 ? payload.min_scenes : 6,
                MaxScenes = (payload.max_scenes >= (payload.min_scenes > 0 ? payload.min_scenes : 6)) ? payload.max_scenes : Math.Max(12, (payload.min_scenes > 0 ? payload.min_scenes : 6) + 1),
                Tags = payload.tags,
                Tone = payload.tone,
                Description = payload.description ?? string.Empty
            };

            return Ok(normalized);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, new RandomStoryParametersResponse
            {
                Success = false,
                Error = "Request was cancelled"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error randomizing story parameters");
            return StatusCode(500, new RandomStoryParametersResponse
            {
                Success = false,
                Error = "An unexpected error occurred"
            });
        }
    }

    [HttpPost("generate")]
    public async Task<ActionResult<GenerateJsonStoryResponse>> GenerateJsonStory([FromBody] GenerateJsonStoryRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(new GenerateJsonStoryResponse
                {
                    Success = false,
                    Error = "Title is required"
                });
            }

            if (request.MinScenes <= 0 || request.MaxScenes < request.MinScenes)
            {
                return BadRequest(new GenerateJsonStoryResponse
                {
                    Success = false,
                    Error = "Invalid scene count range"
                });
            }

            var command = new GenerateStoryCommand(request);
            var result = await _mediator.Send(command, cancellationToken);
            if (!result.Success)
            {
                return StatusCode(502, result);
            }

            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, new GenerateJsonStoryResponse
            {
                Success = false,
                Error = "Request was cancelled"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating JSON story");
            return StatusCode(500, new GenerateJsonStoryResponse
            {
                Success = false,
                Error = "An unexpected error occurred"
            });
        }
    }

    private static string NormalizeDifficulty(string difficulty)
    {
        var d = (difficulty ?? string.Empty).Trim().ToLowerInvariant();
        return d switch
        {
            "easy" => "Easy",
            "medium" or "moderate" => "Medium",
            "hard" => "Hard",
            _ => "Medium"
        };
    }

    private static string NormalizeSessionLength(string length)
    {
        var l = (length ?? string.Empty).Trim().ToLowerInvariant();
        return l switch
        {
            "short" => "Short",
            "medium" or "moderate" => "Medium",
            "long" => "Long",
            _ => "Medium"
        };
    }

    private static int InferMinimumAgeFromAgeGroup(string? ageGroup)
    {
        if (string.IsNullOrWhiteSpace(ageGroup)) return 0;
        var parts = ageGroup.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 2 && int.TryParse(parts[0], out var min))
        {
            return min;
        }
        return 0;
    }

    private static string ExtractJson(string content, string marker)
    {
        if (string.IsNullOrWhiteSpace(content)) return string.Empty;
        var idx = content.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return string.Empty;
        var json = content.Substring(idx + marker.Length).Trim();
        if (json.StartsWith("```") )
        {
            json = json.Trim('`');
            var newline = json.IndexOf('\n');
            if (newline > -1)
            {
                json = json[(newline + 1)..];
            }
            var endIdx = json.LastIndexOf("```", StringComparison.Ordinal);
            if (endIdx > -1)
            {
                json = json.Substring(0, endIdx);
            }
        }
        return json.Trim();
    }

    private class RandomParamsPayload
    {
        public string? title { get; set; }
        public string difficulty { get; set; } = string.Empty;
        public string session_length { get; set; } = string.Empty;
        public string? age_group { get; set; }
        public int minimum_age { get; set; }
        public List<string>? core_axes { get; set; }
        public List<string>? archetypes { get; set; }
        public int character_count { get; set; }
        public int min_scenes { get; set; } = 6;
        public int max_scenes { get; set; } = 12;
        public List<string>? tags { get; set; }
        public string? tone { get; set; }
        public string? description { get; set; }
    }
}
