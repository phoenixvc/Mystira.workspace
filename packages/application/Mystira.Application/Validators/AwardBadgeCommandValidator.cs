using FluentValidation;
using Mystira.Application.CQRS.UserBadges.Commands;

namespace Mystira.Application.Validators;

/// <summary>
/// Validates AwardBadgeCommand before processing.
/// </summary>
public class AwardBadgeCommandValidator : AbstractValidator<AwardBadgeCommand>
{
    public AwardBadgeCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Badge request is required");

        RuleFor(x => x.Request.UserProfileId)
            .NotEmpty()
            .WithMessage("User profile ID is required")
            .When(x => x.Request != null);

        RuleFor(x => x.Request.BadgeConfigurationId)
            .NotEmpty()
            .WithMessage("Badge configuration ID is required")
            .When(x => x.Request != null);
    }
}
