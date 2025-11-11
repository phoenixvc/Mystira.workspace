using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Api.Services.LLM;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;

namespace Mystira.StoryGenerator.Api.Controllers;

/// <summary>
/// Controller for chat completion endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ILLMServiceFactory _llmServiceFactory;
    private readonly AiSettings _aiSettings;
    private readonly ILogger<ChatController> _logger;

    public ChatController(ILLMServiceFactory llmServiceFactory, IOptions<AiSettings> aiOptions, ILogger<ChatController> logger)
    {
        _llmServiceFactory = llmServiceFactory;
        _aiSettings = aiOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Generate a chat completion using the specified LLM provider
    /// </summary>
    /// <param name="request">The chat completion request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The chat completion response</returns>
    [HttpPost("complete")]
    public async Task<ActionResult<ChatCompletionResponse>> Complete(
        [FromBody] ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate the request
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);

            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                var errors = validationResults.Select(r => r.ErrorMessage).ToList();
                _logger.LogWarning("Invalid chat completion request: {Errors}", string.Join(", ", errors));

                return BadRequest(new ChatCompletionResponse
                {
                    Success = false,
                    Error = $"Validation failed: {string.Join(", ", errors)}"
                });
            }

            var llmService = _llmServiceFactory.GetService(request.Provider);

            if (llmService == null)
            {
                _logger.LogError("No LLM services are available");
                return StatusCode(503, new ChatCompletionResponse
                {
                    Success = false,
                    Error = "No LLM services are currently available"
                });
            }

            var resolvedModelName = request.ModelId;

            request.Provider = llmService.ProviderName;
            request.ModelId = resolvedModelName;
            request.Model = resolvedModelName;

            if (request.Temperature is var configuredTemp && Math.Abs(request.Temperature - _aiSettings.DefaultTemperature) < 0.0001)
            {
                request.Temperature = configuredTemp;
            }

            if (request.MaxTokens is var configuredMax)
            {
                request.MaxTokens = Math.Max(request.MaxTokens, configuredMax);
            }

            _logger.LogInformation("Processing chat completion request with provider: {Provider}, modelId: {ModelId}, model: {Model}",
                llmService.ProviderName, resolvedModelName, resolvedModelName);

            // Call the LLM service
            var response = await llmService.CompleteAsync(request, cancellationToken);

            if (string.IsNullOrWhiteSpace(response.ModelId) && !string.IsNullOrWhiteSpace(resolvedModelName))
            {
                response.ModelId = resolvedModelName;
            }

            if (string.IsNullOrWhiteSpace(response.Model) && !string.IsNullOrWhiteSpace(resolvedModelName))
            {
                response.Model = resolvedModelName;
            }

            if (string.IsNullOrWhiteSpace(response.Provider))
            {
                response.Provider = llmService.ProviderName;
            }

            if (!response.Success)
            {
                _logger.LogError("LLM service returned error: {Error}", response.Error);
                return StatusCode(502, response);
            }

            _logger.LogInformation("Chat completion successful. Provider: {Provider}, Model: {Model}, ModelId: {ModelId}, Tokens: {Tokens}",
                response.Provider, response.Model, response.ModelId, response.Usage?.TotalTokens ?? 0);

            return Ok(response);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Chat completion request was cancelled");
            return StatusCode(499, new ChatCompletionResponse
            {
                Success = false,
                Error = "Request was cancelled"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing chat completion request");
            return StatusCode(500, new ChatCompletionResponse
            {
                Success = false,
                Error = "An unexpected error occurred"
            });
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
            var availableServices = _llmServiceFactory.GetAvailableServices().ToList();

            var providers = availableServices.Select(s => new
            {
                Name = s.ProviderName,
                Available = s.IsAvailable()
            }).ToList();

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
