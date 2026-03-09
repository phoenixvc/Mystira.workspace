using Mystira.StoryGenerator.Contracts.Chat;

namespace Mystira.StoryGenerator.Domain.Commands.Chat;

public class HelpCommand : ICommand<ChatCompletionResponse>
{
    public HelpCommand(string? userQuery, IEnumerable<MystiraChatMessage>? history = null)
    {
        UserQuery = userQuery;
        History = history ?? Enumerable.Empty<MystiraChatMessage>();
    }

    public string? UserQuery { get; }
    public IEnumerable<MystiraChatMessage> History { get; }
}
