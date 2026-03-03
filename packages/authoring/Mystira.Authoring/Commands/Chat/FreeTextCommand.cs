using Mystira.Authoring.Abstractions.Commands;
using Mystira.Authoring.Abstractions.Services;
using Mystira.Contracts.StoryGenerator.Chat;

namespace Mystira.Authoring.Commands.Chat;

/// <summary>
/// Command for free-text chat interactions.
/// </summary>
public class FreeTextCommand : ICommand<ChatOrchestrationResponse>
{
    /// <summary>
    /// The authoring context for the chat.
    /// </summary>
    public AuthoringContext Context { get; set; } = new();
}
