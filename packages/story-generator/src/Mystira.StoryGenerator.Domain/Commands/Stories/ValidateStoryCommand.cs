using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Stories;

namespace Mystira.StoryGenerator.Domain.Commands.Stories;

public class ValidateStoryCommand : ICommand<ValidationResponse>
{
    public ValidateStoryCommand(ValidateStoryRequest request, string? userQuery = null, StorySnapshot? currentStory = null, IEnumerable<MystiraChatMessage>? history = null)
    {
        Request = request;
        UserQuery = userQuery;
        CurrentStory = currentStory;
        History = history ?? Enumerable.Empty<MystiraChatMessage>();
    }

    public ValidateStoryRequest Request { get; }
    public string? UserQuery { get; }
    public StorySnapshot? CurrentStory { get; }
    public IEnumerable<MystiraChatMessage> History { get; }
}
