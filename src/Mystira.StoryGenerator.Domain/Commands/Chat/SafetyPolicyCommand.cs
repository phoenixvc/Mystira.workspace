using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Domain.Commands.Chat;

public class SafetyPolicyCommand : ICommand<ChatCompletionResponse>
{
    public SafetyPolicyCommand(ChatContext context, string? userQuery)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        UserQuery = userQuery;
    }

    public ChatContext Context { get; }
    public string? UserQuery { get; }
}
