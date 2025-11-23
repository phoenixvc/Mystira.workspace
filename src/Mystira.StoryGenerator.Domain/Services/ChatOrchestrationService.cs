using System.Text;
using Microsoft.Extensions.Logging;
using MediatR;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Intent;
using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Domain.Commands.Stories;
using Mystira.StoryGenerator.Llm.Services.Intent;

namespace Mystira.StoryGenerator.Domain.Services;

/// <summary>
/// Implementation of chat orchestration service that coordinates intent classification,
/// command dispatch via MediatR, and response mapping
/// </summary>
public class ChatOrchestrationService : IChatOrchestrationService
{
    private readonly ICommandIntentRouter _commandIntentRouter;
    private readonly IMediator _mediator;
    private readonly ILLMServiceFactory _llmServiceFactory;
    private readonly IInstructionBlockService _instructionBlockService;
    private readonly IIntentRouterService _intentRouterService;
    private readonly ILogger<ChatOrchestrationService> _logger;

    public ChatOrchestrationService(
        ICommandIntentRouter commandIntentRouter,
        IMediator mediator,
        ILLMServiceFactory llmServiceFactory,
        IInstructionBlockService instructionBlockService,
        IIntentRouterService intentRouterService,
        ILogger<ChatOrchestrationService> logger)
    {
        _commandIntentRouter = commandIntentRouter;
        _mediator = mediator;
        _llmServiceFactory = llmServiceFactory;
        _instructionBlockService = instructionBlockService;
        _intentRouterService = intentRouterService;
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
            
            if (commandResult.RequiresClarification)
            {
                return commandResult;
            }

            // If no specific command was handled, use free-form text handler
            if (commandResult.Result == null)
            {
                return await HandleFreeFormTextAsync(instructionType, context, cancellationToken);
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

        var command = new RefineStoryCommand(request, userMessage, userMessage);
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

    private async Task<ChatOrchestrationResponse> HandleFreeFormTextAsync(string instructionType, ChatContext context, CancellationToken cancellationToken)
    {
        // Get appropriate LLM service
        var service = !string.IsNullOrWhiteSpace(context.Provider)
            ? _llmServiceFactory.GetService(context.Provider!)
            : _llmServiceFactory.GetDefaultService();
            
        if (service is null)
        {
            return new ChatOrchestrationResponse 
            { 
                Success = false, 
                Error = "No LLM services are currently available" 
            };
        }

        // Build chat completion request from context
        var request = new ChatCompletionRequest
        {
            Provider = context.Provider,
            ModelId = context.ModelId,
            Model = context.Model,
            Messages = context.Messages,
            Temperature = context.Temperature,
            MaxTokens = context.MaxTokens,
            SystemPrompt = context.SystemPrompt,
            JsonSchemaFormat = context.JsonSchemaFormat,
            IsSchemaValidationStrict = context.IsSchemaValidationStrict
        };

        // Add instruction block if available
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
            return new ChatOrchestrationResponse 
            { 
                Success = false, 
                Error = response.Error 
            };
        }

        return new ChatOrchestrationResponse
        {
            Success = true,
            RequiresClarification = false,
            Intent = instructionType,
            Result = response
        };
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

        var searchContext = new InstructionSearchContext
        {
            QueryText = queryText,
            Categories = categories,
            InstructionTypes = instructionTypes,
            TopK = 8
        };

        return await _instructionBlockService.BuildInstructionBlockAsync(searchContext, cancellationToken);
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
        var parameterCheckRequest = new ChatCompletionRequest
        {
            Provider = context.Provider,
            ModelId = context.ModelId,
            Model = context.Model,
            Messages = new List<MystiraChatMessage>
            {
                new()
                {
                    MessageType = ChatMessageType.System,
                    Content = @"You are a parameter extraction assistant. Analyze the user's request for story generation and identify which required parameters are missing.

Required parameters for story generation:
- title: The story title
- agegroup: Age group (must be one of: 1-2, 3-5, 6-9, 10-12, 13-18)
- minScenes: Minimum number of scenes (must be > 0)
- maxScenes: Maximum number of scenes (must be >= minScenes)

Respond with ONLY a JSON array of missing required parameter names. If no required parameters are missing, respond with an empty array: [].
Do not include optional parameters like difficulty, session_length, etc.
Example responses:
[""title"", ""agegroup""]
[]
[""minScenes"", ""maxScenes""]",
                    Timestamp = DateTime.UtcNow
                },
                new()
                {
                    MessageType = ChatMessageType.User,
                    Content = userMessage,
                    Timestamp = DateTime.UtcNow
                }
            },
            Temperature = 0.1, // Low temperature for consistent extraction
            MaxTokens = 100 // Short response for parameter checking
        };

        var response = await service.CompleteAsync(parameterCheckRequest, cancellationToken);
        if (!response.Success || string.IsNullOrWhiteSpace(response.Content))
        {
            return new List<string> { "title", "agegroup", "minScenes", "maxScenes" };
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
        // Look for story content in the conversation
        // This is a simplified implementation - you might need more sophisticated extraction
        var messageWithStory = context.Messages.LastOrDefault(m => 
            m.MessageType == ChatMessageType.User && 
            (m.Content?.Contains('{') == true || m.Content?.Length > 500));
        
        return messageWithStory?.Content;
    }
}