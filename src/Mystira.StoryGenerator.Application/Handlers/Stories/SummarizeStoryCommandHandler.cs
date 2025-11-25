using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Domain.Commands;
using Mystira.StoryGenerator.Domain.Commands.Stories;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Application.Handlers.Stories;

public class SummarizeStoryCommandHandler : ICommandHandler<SummarizeStoryCommand, ChatCompletionResponse>
{
    private readonly ILLMServiceFactory _llmFactory;
    private readonly AiSettings _settings;
    private readonly ILogger<SummarizeStoryCommandHandler> _logger;

    public SummarizeStoryCommandHandler(
        ILLMServiceFactory llmFactory,
        IOptions<AiSettings> aiOptions,
        ILogger<SummarizeStoryCommandHandler> logger)
    {
        _llmFactory = llmFactory;
        _settings = aiOptions.Value;
        _logger = logger;
    }

    public async Task<ChatCompletionResponse> Handle(SummarizeStoryCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var service = !string.IsNullOrWhiteSpace(command.Provider)
                ? _llmFactory.GetService(command.Provider!, command.Model)
                : _llmFactory.GetDefaultService();

            if (service is null)
            {
                return new ChatCompletionResponse
                {
                    Success = false,
                    Error = "No LLM services are currently available"
                };
            }

            var resolvedModelName = string.IsNullOrWhiteSpace(command.Model) ? null : command.Model;
            var temperature = Math.Min(0.5, _settings.DefaultTemperature);
            var maxTokens = Math.Max(500, _settings.DefaultMaxTokens);

            var systemPrompt = BuildSummarizationSystemPrompt();
            var messages = new List<MystiraChatMessage>
            {
                new MystiraChatMessage
                {
                    MessageType = ChatMessageType.User,
                    Content = $"Summarize this story:\n\n{command.StoryContent}",
                    Timestamp = DateTime.UtcNow
                }
            };

            var chatRequest = new ChatCompletionRequest
            {
                Provider = service.ProviderName,
                Model = resolvedModelName,
                Temperature = temperature,
                MaxTokens = maxTokens,
                Messages = messages,
                SystemPrompt = systemPrompt
            };

            var response = await service.CompleteAsync(chatRequest, cancellationToken);
            return response;
        }
        catch (OperationCanceledException)
        {
            return new ChatCompletionResponse
            {
                Success = false,
                Error = "Request was cancelled"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error summarizing story");
            return new ChatCompletionResponse
            {
                Success = false,
                Error = "An unexpected error occurred during summarization"
            };
        }
    }

    private static string BuildSummarizationSystemPrompt()
    {
        return @"
You are a story summarization specialist. Your job is to create concise, engaging summaries of branching adventure stories.

When summarizing a story:
- Capture the main plot, key characters, and central themes
- Highlight important decision points and branching paths
- Keep the summary concise (2-3 paragraphs)
- Use language appropriate for the target age group if specified
- Maintain the excitement and atmosphere of the original story

Provide a well-structured summary that gives readers a sense of what the story is about and what they can expect.
";
    }
}
