using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Stories;

namespace Mystira.StoryGenerator.Domain.Commands.Stories;

public class GenerateStoryCommand : ICommand<GenerateJsonStoryResponse>
{
    public GenerateStoryCommand(GenerateJsonStoryRequest request, string? userQuery = null, IEnumerable<MystiraChatMessage>? history = null)
    {
        Request = request;
        UserQuery = userQuery;
        History = history ?? Enumerable.Empty<MystiraChatMessage>();
    }

    public GenerateJsonStoryRequest Request { get; }
    public string? UserQuery { get; }
    public IEnumerable<MystiraChatMessage> History { get; }
}
