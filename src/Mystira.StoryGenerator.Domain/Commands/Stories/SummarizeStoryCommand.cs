using Mystira.StoryGenerator.Contracts.Chat;

namespace Mystira.StoryGenerator.Domain.Commands.Stories;

public class SummarizeStoryCommand : ICommand<ChatCompletionResponse>
{
    public SummarizeStoryCommand(string storyContent, string? provider = null, string? model = null, string? userQuery = null)
    {
        StoryContent = storyContent;
        Provider = provider;
        Model = model;
        UserQuery = userQuery;
    }

    public string StoryContent { get; }
    public string? Provider { get; }
    public string? Model { get; }
    public string? UserQuery { get; }
}
