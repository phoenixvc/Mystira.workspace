namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when a user verifies their email.
/// </summary>
public sealed record EmailVerified : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The verified email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Verification method (link, code).
    /// </summary>
    public required string Method { get; init; }
}

/// <summary>
/// Published when a user starts onboarding.
/// </summary>
public sealed record OnboardingStarted : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Onboarding flow version.
    /// </summary>
    public required string FlowVersion { get; init; }

    /// <summary>
    /// Source of signup (organic, referral, campaign).
    /// </summary>
    public string? Source { get; init; }
}

/// <summary>
/// Published when a user completes a specific onboarding step.
/// </summary>
public sealed record OnboardingStepCompleted : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Step identifier.
    /// </summary>
    public required string StepId { get; init; }

    /// <summary>
    /// Step number in sequence.
    /// </summary>
    public required int StepNumber { get; init; }

    /// <summary>
    /// Total steps in onboarding.
    /// </summary>
    public required int TotalSteps { get; init; }

    /// <summary>
    /// Time spent on this step in seconds.
    /// </summary>
    public int DurationSeconds { get; init; }
}

/// <summary>
/// Published when a user completes onboarding.
/// </summary>
public sealed record OnboardingCompleted : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Onboarding flow version.
    /// </summary>
    public required string FlowVersion { get; init; }

    /// <summary>
    /// Total duration in seconds.
    /// </summary>
    public required int TotalDurationSeconds { get; init; }

    /// <summary>
    /// Steps completed vs skipped.
    /// </summary>
    public required int StepsCompleted { get; init; }

    /// <summary>
    /// Steps skipped.
    /// </summary>
    public int StepsSkipped { get; init; }
}

/// <summary>
/// Published when a user skips onboarding.
/// </summary>
public sealed record OnboardingSkipped : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Step where they skipped.
    /// </summary>
    public required string SkippedAtStep { get; init; }

    /// <summary>
    /// Reason if provided.
    /// </summary>
    public string? Reason { get; init; }
}

/// <summary>
/// Published when a user completes their profile.
/// </summary>
public sealed record ProfileCompleted : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Completion percentage (some fields optional).
    /// </summary>
    public required int CompletionPercent { get; init; }

    /// <summary>
    /// Whether avatar was uploaded.
    /// </summary>
    public bool HasAvatar { get; init; }

    /// <summary>
    /// Whether bio was added.
    /// </summary>
    public bool HasBio { get; init; }
}

/// <summary>
/// Published when a user signs up via referral.
/// </summary>
public sealed record ReferralSignup : IntegrationEventBase
{
    /// <summary>
    /// The new user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The referrer's account ID.
    /// </summary>
    public required string ReferrerAccountId { get; init; }

    /// <summary>
    /// Referral code used.
    /// </summary>
    public required string ReferralCode { get; init; }

    /// <summary>
    /// Campaign if applicable.
    /// </summary>
    public string? Campaign { get; init; }
}

/// <summary>
/// Published when a referral reward is granted.
/// </summary>
public sealed record ReferralRewardGranted : IntegrationEventBase
{
    /// <summary>
    /// The referrer's account ID.
    /// </summary>
    public required string ReferrerAccountId { get; init; }

    /// <summary>
    /// The referred user's account ID.
    /// </summary>
    public required string ReferredAccountId { get; init; }

    /// <summary>
    /// Reward type (xp, credits, premium_days).
    /// </summary>
    public required string RewardType { get; init; }

    /// <summary>
    /// Reward amount.
    /// </summary>
    public required int RewardAmount { get; init; }
}
