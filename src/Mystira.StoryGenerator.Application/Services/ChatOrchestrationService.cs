using System.Text;
using MediatR;
using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Domain.Commands.Chat;
using Mystira.StoryGenerator.Domain.Commands.Stories;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Contracts.Chat;

namespace Mystira.StoryGenerator.Application.Services;

/// <summary>
/// Implementation of chat orchestration service that coordinates intent classification,
/// command dispatch via MediatR, and response mapping
/// </summary>
public class ChatOrchestrationService : IChatOrchestrationService
{
    private readonly ICommandRouter _commandRouter;
    private readonly IMediator _mediator;
    private readonly ILlmServiceFactory _llmServiceFactory;
    private readonly ILogger<ChatOrchestrationService> _logger;

    public ChatOrchestrationService(
        ICommandRouter commandRouter,
        IMediator mediator,
        ILlmServiceFactory llmServiceFactory,
        ILogger<ChatOrchestrationService> logger)
    {
        _commandRouter = commandRouter;
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
            var instructionType = await _commandRouter.DetectPrimaryInstructionTypeAsync(latestUserMessage, cancellationToken);

            // Heuristic for "continue" intent
            if (string.IsNullOrWhiteSpace(instructionType) || instructionType == "help")
            {
                var lowerMessage = latestUserMessage.ToLowerInvariant();
                if (lowerMessage == "yes" || lowerMessage == "continue" || lowerMessage == "please continue" || lowerMessage == "yes please")
                {
                    instructionType = "story_generate_continue";
                }
            }

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

                case "story_generate_continue":
                    return await HandleContinueStoryAsync(latestUserMessage, context, cancellationToken);

                case "story_validate":
                    return await HandleValidateStoryAsync(latestUserMessage, context, cancellationToken);

                case "story_autofix":
                    return await HandleAutoFixStoryAsync(latestUserMessage, context, cancellationToken);

                case "story_summarize":
                    return await HandleSummarizeStoryAsync(latestUserMessage, context, cancellationToken);

                case "help":
                    return await HandleHelpCommandAsync(instructionType, latestUserMessage, context, cancellationToken);

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

        if (missingParameters.Contains("title"))
        {
            // Try to generate a title if an idea is provided
            var generatedTitle = await TryGenerateTitleFromIdeaAsync(userMessage, context, cancellationToken);
            if (!string.IsNullOrWhiteSpace(generatedTitle))
            {
                missingParameters.Remove("title");
                // We'll pass the generated title through the user message or context if needed,
                // but the ExtractStoryParametersAsync in the handler will also see the history.
                // For now, let's assume if we generated it, it's no longer "missing" for the clarification prompt.
            }
        }

        if (missingParameters.Count > 0)
        {
            // Get a conversational acknowledgement from the LLM instead of a hardcoded list if possible,
            // but for now, let's just make the hardcoded response better as requested.
            var sb = new StringBuilder();

            if (missingParameters.Count == 1 && missingParameters.Contains("title"))
            {
                sb.AppendLine("That sounds like an exciting story idea! To help me bring it to life, could you please specify a title for your story, or just give me a short idea of what happens?");
            }
            else
            {
                sb.AppendLine("That sounds like an exciting story idea! To help me bring it to life, I just need a few more details:");
                sb.AppendLine();

                foreach (var param in missingParameters)
                {
                    var friendlyName = param switch
                    {
                        "title" => "a title (or a short idea of what the story is about)",
                        "ageGroup" => "the target age group (1-2, 3-5, 6-9, 10-12, or 13-18)",
                        "minScenes" => "the minimum number of scenes",
                        "maxScenes" => "the maximum number of scenes",
                        _ => param.Replace("_", " ")
                    };
                    sb.AppendLine($"- {friendlyName}");
                }

                sb.AppendLine();
                sb.AppendLine("You can also optionally specify things like difficulty, session length, minimum age, core axes, archetypes, character count, tags, tone, or a more detailed description.");
            }

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
            Model = context.Model,
        };

        var command = new GenerateStoryCommand(request, userMessage, context.Messages);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.Success && result.IsIncomplete)
        {
            return new ChatOrchestrationResponse
            {
                Success = true,
                RequiresClarification = true,
                Intent = "story_generate_initial",
                Handler = nameof(GenerateStoryCommand),
                Prompt = "The LLM hasn't finished generating yet but here are the key updates. Would you like to continue generating?",
                Result = result
            };
        }

        return new ChatOrchestrationResponse
        {
            Success = result.Success,
            Intent = "story_generate_initial",
            Handler = nameof(GenerateStoryCommand),
            Result = result,
            Error = result.Success ? null : result.Error
        };
    }

    private async Task<string?> TryGenerateTitleFromIdeaAsync(string userMessage, ChatContext context, CancellationToken cancellationToken)
    {
        var service = !string.IsNullOrWhiteSpace(context.Provider)
            ? _llmServiceFactory.GetService(context.Provider!, context.Model)
            : _llmServiceFactory.GetDefaultService();

        if (service is null) return null;

        var messages = new List<MystiraChatMessage>();
        messages.AddRange(context.Messages);

        // Ensure the latest user message is included if not already in context.Messages
        if (context.Messages.LastOrDefault()?.Content != userMessage)
        {
            messages.Add(new MystiraChatMessage { MessageType = ChatMessageType.User, Content = userMessage });
        }

        var prompt = @"Analyze the conversation and user request.
If the user has provided a story idea, theme, or plot, generate a short, catchy title (3-6 words) for it.
If the user HAS NOT provided any story idea yet (e.g. just said 'I want to make a story'), return 'NO_IDEA'.
Respond with ONLY the title or 'NO_IDEA'.";

        var request = new ChatCompletionRequest
        {
            Provider = context.Provider,
            ModelId = context.ModelId,
            Model = context.Model,
            Messages = messages,
            SystemPrompt = prompt,
            Temperature = 0.3,
            MaxTokens = 20
        };

        var response = await service.CompleteAsync(request, cancellationToken);
        if (response.Success && !string.IsNullOrWhiteSpace(response.Content))
        {
            var content = response.Content.Trim().Trim('"');
            if (content.Equals("NO_IDEA", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            return content;
        }

        return null;
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

        var command = new RefineStoryCommand(request, userMessage, userMessage, context.CurrentStory, context.Messages);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.Success && result.IsIncomplete)
        {
            return new ChatOrchestrationResponse
            {
                Success = true,
                RequiresClarification = true,
                Intent = "story_generate_refine",
                Handler = nameof(RefineStoryCommand),
                Prompt = "The LLM hasn't finished generating yet but here are the key updates. Would you like to continue generating?",
                Result = result
            };
        }

        return new ChatOrchestrationResponse
        {
            Success = result.Success,
            Intent = "story_generate_refine",
            Handler = nameof(RefineStoryCommand),
            Result = result,
            Error = result.Success ? null : result.Error
        };
    }

    private async Task<ChatOrchestrationResponse> HandleContinueStoryAsync(string userMessage, ChatContext context, CancellationToken cancellationToken)
    {
        var storyContent = ExtractStoryFromContext(context);
        if (string.IsNullOrWhiteSpace(storyContent))
        {
            return new ChatOrchestrationResponse
            {
                Success = true,
                RequiresClarification = true,
                Intent = "story_generate_continue",
                Handler = nameof(RefineStoryCommand),
                Prompt = "I don't have enough story content to continue. Please provide the partial story."
            };
        }

        // Create a basic request for refinement/continuation
        var request = new GenerateJsonStoryRequest
        {
            Provider = context.Provider,
            ModelId = context.ModelId,
            Model = context.Model
        };

        var refinementPrompt = "Continue generating the story from where you left off. Return ONLY the complete story JSON incorporating all the content provided below and completing it.";

        var command = new RefineStoryCommand(request, refinementPrompt, refinementPrompt, context.CurrentStory, context.Messages);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.Success && result.IsIncomplete)
        {
            return new ChatOrchestrationResponse
            {
                Success = true,
                RequiresClarification = true,
                Intent = "story_generate_continue",
                Handler = nameof(RefineStoryCommand),
                Prompt = "The LLM hasn't finished generating yet but here are the key updates. Would you like to continue generating?",
                Result = result
            };
        }

        return new ChatOrchestrationResponse
        {
            Success = result.Success,
            Intent = "story_generate_continue",
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

        var command = new ValidateStoryCommand(request, userMessage, history: context.Messages);
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

        var command = new AutoFixStoryJsonCommand(storyContent, context.Provider, context.Model, userMessage, history: context.Messages);
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

        var command = new SummarizeStoryCommand(storyContent, context.Provider, context.Model, userMessage, context.Messages);
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

    private async Task<ChatOrchestrationResponse> HandleHelpCommandAsync(string instructionType, string userMessage,
        ChatContext context, CancellationToken cancellationToken)
    {
        // Help command does not require an LLM service to be available; route directly to handler.
        var command = new HelpCommand(userMessage, context.Messages);
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
        var command = new SchemaDocsCommand(context, userMessage, context.Messages);
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
        var command = new SafetyPolicyCommand(context, userMessage, context.Messages);
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
        var command = new RequirementsCommand(context, userMessage, context.Messages);
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
        var command = new GuidelinesCommand(context, userMessage, context.Messages);
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
        var command = new FreeTextCommand(context, instructionType, context.Messages);
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
            ? _llmServiceFactory.GetService(context.Provider!, context.Model)
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
