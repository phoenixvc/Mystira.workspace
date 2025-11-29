using Mystira.StoryGenerator.Contracts.Chat;

namespace Mystira.StoryGenerator.Domain.Commands.Chat;

public class HelpCommand : ICommand<ChatCompletionResponse>
{
    public HelpCommand(string? userQuery)
    {
        UserQuery = userQuery;
    }

    public string? UserQuery { get; }
}
