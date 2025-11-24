using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Domain.Commands;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Domain.Commands.Chat;

public class SchemaDocsCommand : ICommand<ChatCompletionResponse>
{
    public SchemaDocsCommand(ChatContext context, string? userQuery)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        UserQuery = userQuery;
    }

    public ChatContext Context { get; }
    public string? UserQuery { get; }
}
