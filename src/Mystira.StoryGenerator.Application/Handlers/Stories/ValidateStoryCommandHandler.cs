using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Domain.Commands;
using Mystira.StoryGenerator.Domain.Commands.Stories;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Application.Handlers.Stories;

public class ValidateStoryCommandHandler : ICommandHandler<ValidateStoryCommand, ValidationResponse>
{
    private readonly IStoryValidationService _validationService;
    private readonly ILogger<ValidateStoryCommandHandler> _logger;

    public ValidateStoryCommandHandler(
        IStoryValidationService validationService,
        ILogger<ValidateStoryCommandHandler> logger)
    {
        _validationService = validationService;
        _logger = logger;
    }

    public async Task<ValidationResponse> Handle(ValidateStoryCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _validationService.ValidateStoryAsync(command.Request);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating story");
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
