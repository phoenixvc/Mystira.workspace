using Mystira.StoryGenerator.Contracts.Stories;

namespace Mystira.StoryGenerator.Domain.Commands.Stories;

public class GenerateStoryCommand : ICommand<GenerateJsonStoryResponse>
{
    public GenerateStoryCommand(GenerateJsonStoryRequest request, string? userQuery = null)
    {
        Request = request;
        UserQuery = userQuery;
    }

    public GenerateJsonStoryRequest Request { get; }
    public string? UserQuery { get; }
}
