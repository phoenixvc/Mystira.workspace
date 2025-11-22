using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Contracts.Extensions;
using Mystira.StoryGenerator.Domain.Commands.Stories;
using Mystira.StoryGenerator.Llm.Services.Intent;
using MediatR;
using Mystira.StoryGenerator.Contracts.Stories;

namespace Mystira.StoryGenerator.Api.Controllers;

/// <summary>
/// Controller for chat completion and orchestration
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ILLMServiceFactory _llmServiceFactory;
    private readonly AiSettings _aiSettings;
    private readonly IInstructionBlockService _instructionBlockService;
    private readonly IIntentRouterService _intentRouterService;
    private readonly ILogger<ChatController> _logger;
    private readonly IMediator _mediator;
    private readonly ICommandIntentRouter _commandIntentRouter;

    public ChatController(
        ILLMServiceFactory llmServiceFactory,
        IOptions<AiSettings> aiOptions,
        IInstructionBlockService instructionBlockService,
        IIntentRouterService intentRouterService,
        ILogger<ChatController> logger,
        IMediator mediator,
        ICommandIntentRouter commandIntentRouter)
    {
        _llmServiceFactory = llmServiceFactory;
        _aiSettings = aiOptions.Value;
        _instructionBlockService = instructionBlockService;
        _intentRouterService = intentRouterService;
        _logger = logger;
        _mediator = mediator;
        _commandIntentRouter = commandIntentRouter;
    }

    /// <summary>
    /// Complete a chat interaction: detect intent and either execute a handler or ask for clarification/parameters.
    /// </summary>
    [HttpPost("complete")]
    public async Task<ActionResult<ChatOrchestrationResponse>> Complete(
        [FromBody] ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Basic validation
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);
            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                return BadRequest(new ChatOrchestrationResponse
                {
                    Success = false,
                    Error = $"Validation failed: {string.Join(", ", validationResults.Select(r => r.ErrorMessage))}"
                });
            }

            if (request.Messages is null || request.Messages.Count == 0)
            {
                return Ok(new ChatOrchestrationResponse
                {
                    Success = true,
                    RequiresClarification = true,
                    Prompt = "What would you like to do? You can ask me to generate a story, refine one, validate JSON, or auto-fix issues."
                });
            }

            // Detect intent from the most recent user message
            var latestUserMessage = request.Messages.LastOrDefault(m => m.MessageType == ChatMessageType.User)?.Content?.Trim() ?? string.Empty;
            var instructionType = await _commandIntentRouter.DetectPrimaryInstructionTypeAsync(latestUserMessage, cancellationToken);
            if (string.IsNullOrWhiteSpace(instructionType))
            {
                return Ok(new ChatOrchestrationResponse
                {
                    Success = true,
                    RequiresClarification = true,
                    Prompt = "I can generate a new story, refine an existing one, validate, or summarize. What should I do?"
                });
            }

            // Handle story generation specially: collect parameters if missing
            if (instructionType == "story_generate_initial")
            {
                var (maybeReq, missing) = TryParseGenerateRequest(latestUserMessage);
                if (maybeReq is null || missing.Count > 0)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("To generate a story, please provide:");
                    if (missing.Contains("title")) sb.AppendLine("- title (e.g., 'The Brave Explorer')");
                    if (missing.Contains("age_group")) sb.AppendLine("- agegroup (i.e., 1-2, 3-5, 6-9, 10-12, 13-18)");
                    if (missing.Contains("minScenes")) sb.AppendLine("- minScenes (e.g., 6)");
                    if (missing.Contains("maxScenes")) sb.AppendLine("- maxScenes (e.g., 12)");
                    sb.AppendLine("Optional: difficulty, session_length, minimum_age, core_axes, archetypes, character_count, tags, tone, description");

                    return Ok(new ChatOrchestrationResponse
                    {
                        Success = true,
                        RequiresClarification = true,
                        Intent = instructionType,
                        Handler = nameof(GenerateStoryCommand),
                        Prompt = sb.ToString().TrimEnd()
                    });
                }

                var command = new GenerateStoryCommand(maybeReq, latestUserMessage);
                var result = await _mediator.Send(command, cancellationToken);
                return Ok(new ChatOrchestrationResponse
                {
                    Success = result.Success,
                    Intent = instructionType,
                    Handler = nameof(GenerateStoryCommand),
                    Result = result,
                    Error = result.Success ? null : result.Error
                });
            }

            // Fallback: call the configured LLM and return the result as a chat response
            var service = !string.IsNullOrWhiteSpace(request.Provider)
                ? _llmServiceFactory.GetService(request.Provider!)
                : _llmServiceFactory.GetDefaultService();
            if (service is null)
            {
                return StatusCode(503, new ChatOrchestrationResponse { Success = false, Error = "No LLM services are currently available" });
            }

            var instructionBlock = await ResolveInstructionBlockAsync(request, cancellationToken);
            if (!string.IsNullOrWhiteSpace(instructionBlock))
            {
                request.Messages.Insert(0, new MystiraChatMessage
                {
                    MessageType = ChatMessageType.System,
                    Content = instructionBlock,
                    Timestamp = DateTime.UtcNow
                });
            }

            var response = await service.CompleteAsync(request, cancellationToken);
            if (!response.Success)
            {
                return StatusCode(502, new ChatOrchestrationResponse { Success = false, Error = response.Error });
            }

            return Ok(new ChatOrchestrationResponse
            {
                Success = true,
                RequiresClarification = false,
                Intent = instructionType,
                Result = response
            });
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, new ChatOrchestrationResponse { Success = false, Error = "Request was cancelled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat completion request");
            return StatusCode(500, new ChatOrchestrationResponse { Success = false, Error = "An unexpected error occurred" });
        }
    }

    private static (GenerateJsonStoryRequest? req, HashSet<string> missing) TryParseGenerateRequest(string text)
    {
        var missing = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "title", "minScenes", "maxScenes", "agegroup" };
        if (string.IsNullOrWhiteSpace(text)) return (null, missing);

        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            var json = text.Substring(start, end - start + 1);
            try
            {
                var opts = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var req = System.Text.Json.JsonSerializer.Deserialize<GenerateJsonStoryRequest>(json, opts);
                if (req != null)
                {
                    if (!string.IsNullOrWhiteSpace(req.Title)) missing.Remove("title");
                    if (!string.IsNullOrWhiteSpace(req.AgeGroup)) missing.Remove("AgeGroup");
                    if (req.MinScenes > 0) missing.Remove("minScenes");
                    if (req.MaxScenes >= req.MinScenes && req.MaxScenes > 0) missing.Remove("maxScenes");
                    return (req, missing);
                }
            }
            catch { }
        }
        return (null, missing);
    }

    private async Task<string?> ResolveInstructionBlockAsync(ChatCompletionRequest request, CancellationToken cancellationToken)
    {
        if (request.Messages == null || request.Messages.Count == 0)
        {
            return null;
        }

        var userMessages = request.Messages
            .Where(message => message.MessageType == ChatMessageType.User)
            .TakeLast(4)
            .ToList();

        if (userMessages.Count == 0)
        {
            return null;
        }

        var builder = new StringBuilder();
        foreach (var message in userMessages)
        {
            builder.AppendLine(message.Content);
        }

        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            builder.AppendLine("SystemPrompt: " + request.SystemPrompt);
        }

        var queryText = builder.ToString();

        var categories = new[] { "story_generation", "validation" };
        var instructionTypes = new[] { "requirements", "guidelines" };

        var intentClassification = await _intentRouterService.ClassifyIntentAsync(queryText, cancellationToken);
        if (intentClassification != null)
        {
            _logger.LogInformation(
                "Intent classified: category={Category}, instructionType={InstructionType}",
                intentClassification.Categories,
                intentClassification.InstructionTypes);

            categories = intentClassification.Categories;
            instructionTypes = intentClassification.InstructionTypes;
        }
        else
        {
            _logger.LogDebug("Using default categories and instruction types for RAG query");
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

    /// <summary>
    /// Get information about available LLM providers
    /// </summary>
    /// <returns>List of available providers</returns>
    [HttpGet("providers")]
    public ActionResult<object> GetProviders()
    {
        try
        {
            var providers = new List<object>();

            var defaultService = _llmServiceFactory.GetDefaultService();
            if (defaultService != null)
            {
                providers.Add(new { Name = defaultService.ProviderName, Available = true });
            }

            return Ok(new
            {
                Providers = providers,
                Count = providers.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provider information");
            return StatusCode(500, new { Error = "Failed to get provider information" });
        }
    }
}
