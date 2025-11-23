using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Contracts.Extensions;
using Mystira.StoryGenerator.Domain.Commands.Stories;
using Mystira.StoryGenerator.Llm.Services.Intent;
using MediatR;

namespace Mystira.StoryGenerator.Api.Controllers;

/// <summary>
/// Controller for chat completion and orchestration
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatOrchestrationService _chatOrchestrationService;
    private readonly ILLMServiceFactory _llmServiceFactory;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IChatOrchestrationService chatOrchestrationService,
        ILLMServiceFactory llmServiceFactory,
        ILogger<ChatController> logger)
    {
        _chatOrchestrationService = chatOrchestrationService;
        _llmServiceFactory = llmServiceFactory;
        _logger = logger;
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

            // Build chat context from request
            var chatContext = new ChatContext
            {
                Messages = request.Messages,
                CurrentStory = request.CurrentStory,
                Provider = request.Provider,
                ModelId = request.ModelId,
                Model = request.Model,
                Temperature = request.Temperature,
                MaxTokens = request.MaxTokens,
                SystemPrompt = request.SystemPrompt,
                JsonSchemaFormat = request.JsonSchemaFormat,
                IsSchemaValidationStrict = request.IsSchemaValidationStrict
            };

            // Delegate to orchestration service
            var response = await _chatOrchestrationService.CompleteAsync(chatContext, cancellationToken);
            
            return Ok(response);
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
