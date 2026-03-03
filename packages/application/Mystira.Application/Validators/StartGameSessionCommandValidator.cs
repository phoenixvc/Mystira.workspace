using FluentValidation;
using Mystira.Application.CQRS.GameSessions.Commands;

namespace Mystira.Application.Validators;

/// <summary>
/// Validates StartGameSessionCommand before processing.
/// </summary>
public class StartGameSessionCommandValidator : AbstractValidator<StartGameSessionCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StartGameSessionCommandValidator"/> class.
    /// </summary>
    public StartGameSessionCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Game session request is required");

        RuleFor(x => x.Request.ScenarioId)
            .NotEmpty()
            .WithMessage("Scenario ID is required")
            .When(x => x.Request != null);
    }
}
