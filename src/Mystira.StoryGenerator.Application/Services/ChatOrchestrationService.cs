using System.Text;
using MediatR;
using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Domain.Commands.Chat;
using Mystira.StoryGenerator.Domain.Commands.Stories;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Application.Services;

/// <summary>
/// Implementation of chat orchestration service that coordinates intent classification,
/// command dispatch via MediatR, and response mapping
/// </summary>
public class ChatOrchestrationService : IChatOrchestrationService
{
    private readonly ICommandIntentRouter _commandIntentRouter;
    private readonly IMediator _mediator;
    private readonly ILLMServiceFactory _llmServiceFactory;
    private readonly ILogger<ChatOrchestrationService> _logger;

    public ChatOrchestrationService(
        ICommandIntentRouter commandIntentRouter,
        IMediator mediator,
        ILLMServiceFactory llmServiceFactory,
        ILogger<ChatOrchestrationService> logger)
    {
        _commandIntentRouter = commandIntentRouter;
        _mediator = mediator;
        _llmServiceFactory = llmServiceFactory;
        _logger = logger;
    }

    public async Task<ChatOrchestrationResponse> CompleteAsync(ChatContext context, CancellationToken cancellationToken)
    {
        try
        {
            // Extract the latest user message for intent classification
            var latestUserMessage = context.LatestUserMessage;

            // Detect intent using existing classifier
            var instructionType = await _commandIntentRouter.DetectPrimaryInstructionTypeAsync(latestUserMessage, cancellationToken);

            if (string.IsNullOrWhiteSpace(instructionType))
            {
                return new ChatOrchestrationResponse
                {
                    Success = true,
                    RequiresClarification = true,
                    Prompt = "I can generate a new story, refine an existing one, validate, or summarize. What should I do?"
                };
            }

            // Route to appropriate command based on instruction type
            var commandResult = await RouteToCommandAsync(instructionType, context, cancellationToken);

            if (commandResult.RequiresClarification || !commandResult.Success)
            {
                return commandResult;
            }

            // If no specific command was handled, use free-form text handler
            if (commandResult.Result == null)
            {
                return await HandleFreeTextCommandAsync(instructionType, context, cancellationToken);
            }

            return commandResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in chat orchestration");
            return new ChatOrchestrationResponse
            {
                Success = false,
                Error = "An unexpected error occurred during orchestration"
            };
        }
    }

    private async Task<ChatOrchestrationResponse> RouteToCommandAsync(string instructionType, ChatContext context, CancellationToken cancellationToken)
    {
        var latestUserMessage = context.LatestUserMessage;

        try
        {
            switch (instructionType)
            {
                case "story_generate_initial":
                    return await HandleGenerateStoryAsync(latestUserMessage, context, cancellationToken);

                case "story_generate_refine":
                    return await HandleRefineStoryAsync(latestUserMessage, context, cancellationToken);

                case "story_validate":
                    return await HandleValidateStoryAsync(latestUserMessage, context, cancellationToken);

                case "story_autofix":
                    return await HandleAutoFixStoryAsync(latestUserMessage, context, cancellationToken);

                case "story_summarize":
                    return await HandleSummarizeStoryAsync(latestUserMessage, context, cancellationToken);

                case "help":
                    return await HandleHelpCommandAsync(instructionType, latestUserMessage, cancellationToken);

                case "schema_docs":
                    return await HandleSchemaDocsCommandAsync(instructionType, latestUserMessage, context, cancellationToken);

                case "safety_policy":
                    return await HandleSafetyPolicyCommandAsync(instructionType, latestUserMessage, context, cancellationToken);

                case "requirements":
                    return await HandleRequirementsCommandAsync(instructionType, latestUserMessage, context, cancellationToken);

                case "guidelines":
                    return await HandleGuidelinesCommandAsync(instructionType, latestUserMessage, context, cancellationToken);

                default:
                    // Return empty response to trigger fallback
                    return new ChatOrchestrationResponse
                    {
                        Success = true,
                        Intent = instructionType
                    };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error routing to command for instruction type: {InstructionType}", instructionType);
            return new ChatOrchestrationResponse
            {
                Success = false,
                Intent = instructionType,
                Error = $"Failed to process {instructionType}: {ex.Message}"
            };
        }
    }

    private async Task<ChatOrchestrationResponse> HandleGenerateStoryAsync(string userMessage, ChatContext context, CancellationToken cancellationToken)
    {
        // Use lightweight LLM call to check for missing parameters
        var missingParameters = await CheckForMissingStoryParametersAsync(userMessage, context, cancellationToken);

        if (missingParameters.Count > 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine("To generate a story, please provide:");
            foreach (var param in missingParameters)
            {
                sb.AppendLine($"- {param}");
            }
            sb.AppendLine("Optional: difficulty, session_length, minimum_age, core_axes, archetypes, character_count, tags, tone, description");

            return new ChatOrchestrationResponse
            {
                Success = true,
                RequiresClarification = true,
                Intent = "story_generate_initial",
                Handler = nameof(GenerateStoryCommand),
                Prompt = sb.ToString().TrimEnd()
            };
        }

        // Create a basic request with the user message - the handler will extract and validate parameters
        var request = new GenerateJsonStoryRequest
        {
            Provider = context.Provider,
            ModelId = context.ModelId,
            Model = context.Model
        };

        var command = new GenerateStoryCommand(request, userMessage);
        var result = await _mediator.Send(command, cancellationToken);

        return new ChatOrchestrationResponse
        {
            Success = result.Success,
            Intent = "story_generate_initial",
            Handler = nameof(GenerateStoryCommand),
            Result = result,
            Error = result.Success ? null : result.Error
        };
    }

    private async Task<ChatOrchestrationResponse> HandleRefineStoryAsync(string userMessage, ChatContext context, CancellationToken cancellationToken)
    {
        // For refinement, we need to extract the story content and refinement prompt
        // This is a simplified implementation - in practice you might need more sophisticated parsing
        var storyContent = ExtractStoryFromContext(context);
        if (string.IsNullOrWhiteSpace(storyContent))
        {
            return new ChatOrchestrationResponse
            {
                Success = true,
                RequiresClarification = true,
                Intent = "story_generate_refine",
                Handler = nameof(RefineStoryCommand),
                Prompt = "Please provide the story content you'd like me to refine."
            };
        }

        // Create a basic request for refinement
        var request = new GenerateJsonStoryRequest
        {
            Provider = context.Provider,
            ModelId = context.ModelId,
            Model = context.Model
        };

        var command = new RefineStoryCommand(request, userMessage, userMessage, context.CurrentStory);
        var result = await _mediator.Send(command, cancellationToken);

        return new ChatOrchestrationResponse
        {
            Success = result.Success,
            Intent = "story_generate_refine",
            Handler = nameof(RefineStoryCommand),
            Result = result,
            Error = result.Success ? null : result.Error
        };
    }

    private async Task<ChatOrchestrationResponse> HandleValidateStoryAsync(string userMessage, ChatContext context, CancellationToken cancellationToken)
    {
        var storyContent = ExtractStoryFromContext(context);
        if (string.IsNullOrWhiteSpace(storyContent))
        {
            return new ChatOrchestrationResponse
            {
                Success = true,
                RequiresClarification = true,
                Intent = "story_validate",
                Handler = nameof(ValidateStoryCommand),
                Prompt = "Please provide the story content you'd like me to validate."
            };
        }

        var request = new ValidateStoryRequest
        {
            StoryContent = storyContent,
            Format = "json"
        };

        var command = new ValidateStoryCommand(request, userMessage);
        var result = await _mediator.Send(command, cancellationToken);

        return new ChatOrchestrationResponse
        {
            Success = true,
            Intent = "story_validate",
            Handler = nameof(ValidateStoryCommand),
            Result = result
        };
    }

    private async Task<ChatOrchestrationResponse> HandleAutoFixStoryAsync(string userMessage, ChatContext context, CancellationToken cancellationToken)
    {
        var storyContent = ExtractStoryFromContext(context);
        if (string.IsNullOrWhiteSpace(storyContent))
        {
            return new ChatOrchestrationResponse
            {
                Success = true,
                RequiresClarification = true,
                Intent = "story_autofix",
                Handler = nameof(AutoFixStoryJsonCommand),
                Prompt = "Please provide the story JSON you'd like me to auto-fix."
            };
        }

        var command = new AutoFixStoryJsonCommand(storyContent, context.Provider, context.Model, userMessage);
        var result = await _mediator.Send(command, cancellationToken);

        return new ChatOrchestrationResponse
        {
            Success = result.Success,
            Intent = "story_autofix",
            Handler = nameof(AutoFixStoryJsonCommand),
            Result = result,
            Error = result.Success ? null : result.Error
        };
    }

    private async Task<ChatOrchestrationResponse> HandleSummarizeStoryAsync(string userMessage, ChatContext context, CancellationToken cancellationToken)
    {
        var storyContent = ExtractStoryFromContext(context);
        if (string.IsNullOrWhiteSpace(storyContent))
        {
            return new ChatOrchestrationResponse
            {
                Success = true,
                RequiresClarification = true,
                Intent = "story_summarize",
                Handler = nameof(SummarizeStoryCommand),
                Prompt = "Please provide the story content you'd like me to summarize."
            };
        }

        var command = new SummarizeStoryCommand(storyContent, context.Provider, context.Model, userMessage);
        var result = await _mediator.Send(command, cancellationToken);

        return new ChatOrchestrationResponse
        {
            Success = result.Success,
            Intent = "story_summarize",
            Handler = nameof(SummarizeStoryCommand),
            Result = result,
            Error = result.Success ? null : result.Error
        };
    }

    private async Task<ChatOrchestrationResponse> HandleHelpCommandAsync(string instructionType, string userMessage, CancellationToken cancellationToken)
    {
        var command = new HelpCommand(userMessage);
        var result = await _mediator.Send(command, cancellationToken);

        return new ChatOrchestrationResponse
        {
            Success = result.Success,
            Intent = instructionType,
            Handler = nameof(HelpCommand),
            Result = result,
            Error = result.Success ? null : result.Error
        };
    }

    private async Task<ChatOrchestrationResponse> HandleSchemaDocsCommandAsync(string instructionType, string userMessage, ChatContext context, CancellationToken cancellationToken)
    {
        var command = new SchemaDocsCommand(context, userMessage);
        var result = await _mediator.Send(command, cancellationToken);

        return new ChatOrchestrationResponse
        {
            Success = result.Success,
            Intent = instructionType,
            Handler = nameof(SchemaDocsCommand),
            Result = result,
            Error = result.Success ? null : result.Error
        };
    }

    private async Task<ChatOrchestrationResponse> HandleSafetyPolicyCommandAsync(string instructionType, string userMessage, ChatContext context, CancellationToken cancellationToken)
    {
        var command = new SafetyPolicyCommand(context, userMessage);
        var result = await _mediator.Send(command, cancellationToken);

        return new ChatOrchestrationResponse
        {
            Success = result.Success,
            Intent = instructionType,
            Handler = nameof(SafetyPolicyCommand),
            Result = result,
            Error = result.Success ? null : result.Error
        };
    }

    private async Task<ChatOrchestrationResponse> HandleRequirementsCommandAsync(string instructionType, string userMessage, ChatContext context, CancellationToken cancellationToken)
    {
        var command = new RequirementsCommand(context, userMessage);
        var result = await _mediator.Send(command, cancellationToken);

        return new ChatOrchestrationResponse
        {
            Success = result.Success,
            Intent = instructionType,
            Handler = nameof(RequirementsCommand),
            Result = result,
            Error = result.Success ? null : result.Error
        };
    }

    private async Task<ChatOrchestrationResponse> HandleGuidelinesCommandAsync(string instructionType, string userMessage, ChatContext context, CancellationToken cancellationToken)
    {
        var command = new GuidelinesCommand(context, userMessage);
        var result = await _mediator.Send(command, cancellationToken);

        return new ChatOrchestrationResponse
        {
            Success = result.Success,
            Intent = instructionType,
            Handler = nameof(GuidelinesCommand),
            Result = result,
            Error = result.Success ? null : result.Error
        };
    }

    private async Task<ChatOrchestrationResponse> HandleFreeTextCommandAsync(string instructionType, ChatContext context, CancellationToken cancellationToken)
    {
        var command = new FreeTextCommand(context, instructionType);
        var result = await _mediator.Send(command, cancellationToken);

        return new ChatOrchestrationResponse
        {
            Success = result.Success,
            Intent = instructionType,
            Handler = nameof(FreeTextCommand),
            Result = result,
            Error = result.Success ? null : result.Error
        };
    }

    private async Task<List<string>> CheckForMissingStoryParametersAsync(string userMessage, ChatContext context, CancellationToken cancellationToken)
    {
        // Get appropriate LLM service for parameter checking
        var service = !string.IsNullOrWhiteSpace(context.Provider)
            ? _llmServiceFactory.GetService(context.Provider!)
            : _llmServiceFactory.GetDefaultService();

        if (service is null)
        {
            return new List<string> { "title", "agegroup", "minScenes", "maxScenes" };
        }

        // Build a lightweight request to check for missing parameters
        var messages = new List<MystiraChatMessage>
        {
            new()
            {
                MessageType = ChatMessageType.System,
                Content =
                    @"You are a parameter extraction assistant. Analyze the user's request for story generation and identify which required parameters are missing.

Required parameters for story generation:
- title: The story title
- ageGroup: Age group (must be one of: 1-2, 3-5, 6-9, 10-12, 13-18)
- minScenes: Minimum number of scenes (must be > 0)
- maxScenes: Maximum number of scenes (must be >= minScenes)

Respond with ONLY a JSON array of missing required parameter names (using exactly these names).
If no required parameters are missing, respond with an empty array: [].
Do not include optional parameters like difficulty, sessionLength, etc.
Example responses:
[""title"", ""ageGroup""]
[]
[""minScenes"", ""maxScenes""]",
                Timestamp = DateTime.UtcNow
            }
        };
        messages.AddRange(context.Messages);

        var parameterCheckRequest = new ChatCompletionRequest
        {
            Provider = context.Provider,
            ModelId = context.ModelId,
            Model = context.Model,
            Messages = messages,
            Temperature = 0.1, // Low temperature for consistent extraction
            MaxTokens = 100 // Short response for parameter checking
        };

        var response = await service.CompleteAsync(parameterCheckRequest, cancellationToken);
        if (!response.Success || string.IsNullOrWhiteSpace(response.Content))
        {
            return new List<string> { "title", "ageGroup", "minScenes", "maxScenes" };
        }

        try
        {
            // Parse the JSON response
            var missingParams = System.Text.Json.JsonSerializer.Deserialize<List<string>>(response.Content!, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return missingParams ?? new List<string> { "title", "agegroup", "minScenes", "maxScenes" };
        }
        catch
        {
            // If parsing fails, assume all parameters are missing
            return new List<string> { "title", "agegroup", "minScenes", "maxScenes" };
        }
    }

    private static string? ExtractStoryFromContext(ChatContext context)
    {
        // If there is a current story, use that
        if (context.CurrentStory != null)
            return context.CurrentStory.Content;

        // Else, look for story content in the conversation
        var messageWithStory = context.Messages.LastOrDefault(m =>
            m.MessageType == ChatMessageType.User &&
            (m.Content?.Contains('{') == true || m.Content?.Length > 500));

        return messageWithStory?.Content;
    }
}
