namespace Mystira.App.Domain.Models;

/// <summary>
/// Data deletion request for COPPA compliance.
/// Tracks the lifecycle of a child data deletion request.
/// </summary>
public class DataDeletionRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The child profile to delete
    /// </summary>
    public string ChildProfileId { get; set; } = string.Empty;

    /// <summary>
    /// Who initiated the deletion (Parent, System, Support)
    /// </summary>
    public DeletionRequestSource RequestedBy { get; set; }

    /// <summary>
    /// Current status of the deletion
    /// </summary>
    public DeletionStatus Status { get; set; } = DeletionStatus.Pending;

    /// <summary>
    /// When the deletion was requested
    /// </summary>
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the soft-delete period ends and permanent deletion begins.
    /// COPPA: 7 days for revoked consent, immediately for parent request.
    /// </summary>
    public DateTime ScheduledDeletionAt { get; set; }

    /// <summary>
    /// When the deletion was completed (null if not yet completed)
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Scope of data to delete
    /// </summary>
    public List<string> DeletionScope { get; set; } = new() { "CosmosDB", "BlobStorage", "Logs" };

    /// <summary>
    /// Whether a confirmation email was sent to the parent
    /// </summary>
    public bool ConfirmationEmailSent { get; set; }

    /// <summary>
    /// Number of retry attempts for failed deletions
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// When the next retry should be attempted (null if not scheduled).
    /// Uses exponential backoff: 1h, 2h, 4h, 8h, 16h.
    /// </summary>
    public DateTime? NextRetryAt { get; set; }

    /// <summary>
    /// Maximum number of retry attempts before giving up
    /// </summary>
    public const int MaxRetries = 5;

    /// <summary>
    /// Audit trail of actions taken during deletion
    /// </summary>
    public List<DeletionAuditEntry> AuditTrail { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Add an audit entry
    /// </summary>
    public void AddAuditEntry(string action, string performedByHash)
    {
        AuditTrail.Add(new DeletionAuditEntry
        {
            Action = action,
            PerformedByHash = performedByHash,
            Timestamp = DateTime.UtcNow
        });
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark deletion as complete
    /// </summary>
    public void Complete()
    {
        Status = DeletionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Who requested the deletion
/// </summary>
public enum DeletionRequestSource
{
    Parent,
    System,
    Support
}

/// <summary>
/// Deletion lifecycle status
/// </summary>
public enum DeletionStatus
{
    Pending,
    SoftDeleted,
    InProgress,
    Completed,
    Failed
}

/// <summary>
/// Audit trail entry for data deletion
/// </summary>
public class DeletionAuditEntry
{
    public string Action { get; set; } = string.Empty;
    public string PerformedByHash { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
