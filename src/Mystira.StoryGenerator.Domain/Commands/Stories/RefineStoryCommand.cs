using Mystira.StoryGenerator.Contracts.Stories;

namespace Mystira.StoryGenerator.Domain.Commands.Stories;

public class RefineStoryCommand : ICommand<GenerateJsonStoryResponse>
{
    public RefineStoryCommand(GenerateJsonStoryRequest request, string refinementPrompt, string? userQuery = null)
    {
        Request = request;
        RefinementPrompt = refinementPrompt;
        UserQuery = userQuery;
    }

    public GenerateJsonStoryRequest Request { get; }
    public string RefinementPrompt { get; }
    public string? UserQuery { get; }
}
