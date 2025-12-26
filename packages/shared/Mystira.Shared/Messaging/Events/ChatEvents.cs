namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when a conversation is started.
/// </summary>
public sealed record ConversationStarted : IntegrationEventBase
{
    /// <summary>
    /// The conversation ID.
    /// </summary>
    public required string ConversationId { get; init; }

    /// <summary>
    /// Initiator's account ID.
    /// </summary>
    public required string InitiatorAccountId { get; init; }

    /// <summary>
    /// Participant account IDs.
    /// </summary>
    public required string[] ParticipantIds { get; init; }

    /// <summary>
    /// Conversation type (direct, group, session_chat).
    /// </summary>
    public required string ConversationType { get; init; }

    /// <summary>
    /// Related context if any (session ID, scenario ID).
    /// </summary>
    public string? ContextId { get; init; }
}

/// <summary>
/// Published when a message is sent.
/// </summary>
public sealed record MessageSent : IntegrationEventBase
{
    /// <summary>
    /// The message ID.
    /// </summary>
    public required string MessageId { get; init; }

    /// <summary>
    /// The conversation ID.
    /// </summary>
    public required string ConversationId { get; init; }

    /// <summary>
    /// Sender's account ID.
    /// </summary>
    public required string SenderAccountId { get; init; }

    /// <summary>
    /// Message type (text, image, sticker, system).
    /// </summary>
    public required string MessageType { get; init; }

    /// <summary>
    /// Content length (for text) or file size.
    /// </summary>
    public int ContentLength { get; init; }

    /// <summary>
    /// Whether message has attachments.
    /// </summary>
    public bool HasAttachments { get; init; }
}

/// <summary>
/// Published when a message is delivered.
/// </summary>
public sealed record MessageDelivered : IntegrationEventBase
{
    /// <summary>
    /// The message ID.
    /// </summary>
    public required string MessageId { get; init; }

    /// <summary>
    /// The conversation ID.
    /// </summary>
    public required string ConversationId { get; init; }

    /// <summary>
    /// Recipient account ID.
    /// </summary>
    public required string RecipientAccountId { get; init; }

    /// <summary>
    /// Delivery latency in milliseconds.
    /// </summary>
    public int DeliveryLatencyMs { get; init; }
}

/// <summary>
/// Published when a message is read.
/// </summary>
public sealed record MessageRead : IntegrationEventBase
{
    /// <summary>
    /// The message ID.
    /// </summary>
    public required string MessageId { get; init; }

    /// <summary>
    /// The conversation ID.
    /// </summary>
    public required string ConversationId { get; init; }

    /// <summary>
    /// Reader's account ID.
    /// </summary>
    public required string ReaderAccountId { get; init; }

    /// <summary>
    /// Time between send and read in seconds.
    /// </summary>
    public int ReadDelaySeconds { get; init; }
}

/// <summary>
/// Published when a message is deleted.
/// </summary>
public sealed record MessageDeleted : IntegrationEventBase
{
    /// <summary>
    /// The message ID.
    /// </summary>
    public required string MessageId { get; init; }

    /// <summary>
    /// The conversation ID.
    /// </summary>
    public required string ConversationId { get; init; }

    /// <summary>
    /// Who deleted it.
    /// </summary>
    public required string DeletedByAccountId { get; init; }

    /// <summary>
    /// Whether deleted for everyone or just self.
    /// </summary>
    public bool DeletedForEveryone { get; init; }
}

/// <summary>
/// Published when user starts typing.
/// </summary>
public sealed record TypingStarted : IntegrationEventBase
{
    /// <summary>
    /// The conversation ID.
    /// </summary>
    public required string ConversationId { get; init; }

    /// <summary>
    /// Account ID of the person typing.
    /// </summary>
    public required string AccountId { get; init; }
}

/// <summary>
/// Published when a conversation is archived.
/// </summary>
public sealed record ConversationArchived : IntegrationEventBase
{
    /// <summary>
    /// The conversation ID.
    /// </summary>
    public required string ConversationId { get; init; }

    /// <summary>
    /// Who archived it.
    /// </summary>
    public required string ArchivedByAccountId { get; init; }
}

/// <summary>
/// Published when a user is muted in a conversation.
/// </summary>
public sealed record UserMutedInConversation : IntegrationEventBase
{
    /// <summary>
    /// The conversation ID.
    /// </summary>
    public required string ConversationId { get; init; }

    /// <summary>
    /// Muted user's account ID.
    /// </summary>
    public required string MutedAccountId { get; init; }

    /// <summary>
    /// Who muted them.
    /// </summary>
    public required string MutedByAccountId { get; init; }

    /// <summary>
    /// Duration in minutes (null = permanent until unmuted).
    /// </summary>
    public int? DurationMinutes { get; init; }
}
