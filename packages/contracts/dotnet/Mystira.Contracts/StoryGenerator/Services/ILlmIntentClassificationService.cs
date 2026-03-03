using Mystira.Contracts.StoryGenerator.Intent;

namespace Mystira.Contracts.StoryGenerator.Services;

/// <summary>
/// Service for classifying user intent using LLM.
/// </summary>
public interface ILlmIntentClassificationService
{
    /// <summary>
    /// Classifies the intent of a user message.
    /// </summary>
    /// <param name="message">The user message to classify.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The intent classification result.</returns>
    Task<IntentClassification> ClassifyAsync(
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Classifies intent with conversation context.
    /// </summary>
    /// <param name="message">The user message to classify.</param>
    /// <param name="conversationHistory">Previous messages for context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The intent classification result.</returns>
    Task<IntentClassification> ClassifyWithContextAsync(
        string message,
        IEnumerable<string> conversationHistory,
        CancellationToken cancellationToken = default);
}
