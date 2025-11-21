using Mystira.StoryGenerator.Contracts.Stories;

namespace Mystira.StoryGenerator.Domain.Commands.Stories;

public class ValidateStoryCommand : ICommand<ValidationResponse>
{
    public ValidateStoryCommand(ValidateStoryRequest request, string? userQuery = null)
    {
        Request = request;
        UserQuery = userQuery;
    }

    public ValidateStoryRequest Request { get; }
    public string? UserQuery { get; }
}
