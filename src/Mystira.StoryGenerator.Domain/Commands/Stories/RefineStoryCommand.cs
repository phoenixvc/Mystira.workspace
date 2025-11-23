using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Stories;

namespace Mystira.StoryGenerator.Domain.Commands.Stories;

public class RefineStoryCommand : ICommand<GenerateJsonStoryResponse>
{
    public RefineStoryCommand(GenerateJsonStoryRequest request, string refinementPrompt, string? userQuery = null, StorySnapshot? currentStory = null)
    {
        Request = request;
        RefinementPrompt = refinementPrompt;
        UserQuery = userQuery;
        CurrentStory = currentStory;
    }

    public GenerateJsonStoryRequest Request { get; }
    public string RefinementPrompt { get; }
    public string? UserQuery { get; }
    public StorySnapshot? CurrentStory { get; }
}
