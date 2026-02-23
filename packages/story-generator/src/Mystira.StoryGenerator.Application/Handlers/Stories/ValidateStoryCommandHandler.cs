using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Domain.Commands.Stories;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Application.Handlers.Stories;

public static class ValidateStoryCommandHandler
{
    public static async Task<ValidationResponse> Handle(
        ValidateStoryCommand command,
        IStoryValidationService validationService,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await validationService.ValidateStoryAsync(command.Request);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating story");
            return new ValidationResponse
            {
                IsValid = false,
                Errors = new List<ValidationIssue>
                {
                    new ValidationIssue
                    {
                        Path = "internal",
                        Message = "An internal error occurred during validation"
                    }
                }
            };
        }
    }
}
