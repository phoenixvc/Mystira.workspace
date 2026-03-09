using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Domain.Commands.Chat;

public class SafetyPolicyCommand : ICommand<ChatCompletionResponse>
{
    public SafetyPolicyCommand(ChatContext context, string? userQuery, IEnumerable<MystiraChatMessage>? history = null)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        UserQuery = userQuery;
        History = history ?? Enumerable.Empty<MystiraChatMessage>();
    }

    public ChatContext Context { get; }
    public string? UserQuery { get; }
    public IEnumerable<MystiraChatMessage> History { get; }
}
