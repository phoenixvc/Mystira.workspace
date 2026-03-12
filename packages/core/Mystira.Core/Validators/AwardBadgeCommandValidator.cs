using FluentValidation;
using Mystira.Core.CQRS.UserBadges.Commands;

namespace Mystira.Core.Validators;

/// <summary>
/// Validates AwardBadgeCommand before processing.
/// </summary>
public class AwardBadgeCommandValidator : AbstractValidator<AwardBadgeCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AwardBadgeCommandValidator"/> class.
    /// </summary>
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
