using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Stories;

namespace Mystira.StoryGenerator.Domain.Commands.Stories;

public class AutoFixStoryJsonCommand : ICommand<GenerateJsonStoryResponse>
{
    public AutoFixStoryJsonCommand(string storyJson, string? provider = null, string? model = null, string? userQuery = null, StorySnapshot? currentStory = null, IEnumerable<MystiraChatMessage>? history = null)
    {
        StoryJson = storyJson;
        Provider = provider;
        Model = model;
        UserQuery = userQuery;
        CurrentStory = currentStory;
        History = history ?? Enumerable.Empty<MystiraChatMessage>();
    }

    public string StoryJson { get; }
    public string? Provider { get; }
    public string? Model { get; }
    public string? UserQuery { get; }
    public StorySnapshot? CurrentStory { get; }
    public IEnumerable<MystiraChatMessage> History { get; }
}
