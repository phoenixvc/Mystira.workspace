using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Domain.Commands;

namespace Mystira.StoryGenerator.Domain.Commands.Chat;

public class HelpCommand : ICommand<ChatCompletionResponse>
{
    public HelpCommand(string? userQuery)
    {
        UserQuery = userQuery;
    }

    public string? UserQuery { get; }
}
