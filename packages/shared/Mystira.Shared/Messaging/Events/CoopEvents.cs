namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when a user sends a partner/co-op invite.
/// </summary>
public sealed record PartnerInviteSent : IntegrationEventBase
{
    /// <summary>
    /// The invite ID.
    /// </summary>
    public required string InviteId { get; init; }

    /// <summary>
    /// The inviter's account ID.
    /// </summary>
    public required string InviterAccountId { get; init; }

    /// <summary>
    /// The invitee's account ID.
    /// </summary>
    public required string InviteeAccountId { get; init; }

    /// <summary>
    /// Scenario ID for the co-op session.
    /// </summary>
    public required string ScenarioId { get; init; }

    /// <summary>
    /// Invite type (story_partner, spectator, collaborator).
    /// </summary>
    public required string InviteType { get; init; }

    /// <summary>
    /// When the invite expires.
    /// </summary>
    public required DateTimeOffset ExpiresAt { get; init; }
}

/// <summary>
/// Published when a partner invite is accepted.
/// </summary>
public sealed record PartnerInviteAccepted : IntegrationEventBase
{
    /// <summary>
    /// The invite ID.
    /// </summary>
    public required string InviteId { get; init; }

    /// <summary>
    /// The inviter's account ID.
    /// </summary>
    public required string InviterAccountId { get; init; }

    /// <summary>
    /// The invitee's account ID.
    /// </summary>
    public required string InviteeAccountId { get; init; }

    /// <summary>
    /// Scenario ID for the co-op session.
    /// </summary>
    public required string ScenarioId { get; init; }
}

/// <summary>
/// Published when a partner invite is declined.
/// </summary>
public sealed record PartnerInviteDeclined : IntegrationEventBase
{
    /// <summary>
    /// The invite ID.
    /// </summary>
    public required string InviteId { get; init; }

    /// <summary>
    /// The invitee's account ID.
    /// </summary>
    public required string InviteeAccountId { get; init; }

    /// <summary>
    /// Decline reason if provided.
    /// </summary>
    public string? Reason { get; init; }
}

/// <summary>
/// Published when a co-op session is started.
/// </summary>
public sealed record CoopSessionStarted : IntegrationEventBase
{
    /// <summary>
    /// The co-op session ID.
    /// </summary>
    public required string CoopSessionId { get; init; }

    /// <summary>
    /// The underlying game session ID.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Scenario ID.
    /// </summary>
    public required string ScenarioId { get; init; }

    /// <summary>
    /// Host account ID.
    /// </summary>
    public required string HostAccountId { get; init; }

    /// <summary>
    /// List of participant account IDs.
    /// </summary>
    public required string[] ParticipantIds { get; init; }

    /// <summary>
    /// Co-op mode (turn_based, simultaneous, spectator).
    /// </summary>
    public required string CoopMode { get; init; }
}

/// <summary>
/// Published when a partner joins an active co-op session.
/// </summary>
public sealed record PartnerJoinedSession : IntegrationEventBase
{
    /// <summary>
    /// The co-op session ID.
    /// </summary>
    public required string CoopSessionId { get; init; }

    /// <summary>
    /// The joining partner's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Their role (player, spectator).
    /// </summary>
    public required string Role { get; init; }

    /// <summary>
    /// Current participant count after joining.
    /// </summary>
    public required int ParticipantCount { get; init; }
}

/// <summary>
/// Published when a partner leaves a co-op session.
/// </summary>
public sealed record PartnerLeftSession : IntegrationEventBase
{
    /// <summary>
    /// The co-op session ID.
    /// </summary>
    public required string CoopSessionId { get; init; }

    /// <summary>
    /// The leaving partner's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Reason (voluntary, disconnected, kicked).
    /// </summary>
    public required string Reason { get; init; }

    /// <summary>
    /// Remaining participant count.
    /// </summary>
    public required int RemainingParticipants { get; init; }
}

/// <summary>
/// Published when a co-op session ends.
/// </summary>
public sealed record CoopSessionEnded : IntegrationEventBase
{
    /// <summary>
    /// The co-op session ID.
    /// </summary>
    public required string CoopSessionId { get; init; }

    /// <summary>
    /// The underlying session ID.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Total duration in seconds.
    /// </summary>
    public required int DurationSeconds { get; init; }

    /// <summary>
    /// How it ended (completed, abandoned, host_left).
    /// </summary>
    public required string EndReason { get; init; }

    /// <summary>
    /// Participants who were present at end.
    /// </summary>
    public required string[] FinalParticipantIds { get; init; }
}

/// <summary>
/// Published when control is passed in turn-based co-op.
/// </summary>
public sealed record TurnPassed : IntegrationEventBase
{
    /// <summary>
    /// The co-op session ID.
    /// </summary>
    public required string CoopSessionId { get; init; }

    /// <summary>
    /// Account ID passing the turn.
    /// </summary>
    public required string FromAccountId { get; init; }

    /// <summary>
    /// Account ID receiving the turn.
    /// </summary>
    public required string ToAccountId { get; init; }

    /// <summary>
    /// Current turn number.
    /// </summary>
    public required int TurnNumber { get; init; }
}
