namespace Mystira.Shared.Data.Entities;

/// <summary>
/// Base entity with common Id property.
/// Use as base class for all domain entities.
/// </summary>
public abstract class Entity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public string Id { get; set; } = string.Empty;
}

/// <summary>
/// Auditable entity with automatic tracking of creation and modification.
/// </summary>
public abstract class AuditableEntity : Entity
{
    /// <summary>
    /// When the entity was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Who created the entity (user ID or system identifier).
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// When the entity was last updated (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Who last updated the entity (user ID or system identifier).
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Marks the entity as updated.
    /// </summary>
    /// <param name="updatedBy">Who is updating the entity.</param>
    public virtual void MarkUpdated(string? updatedBy = null)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
}

/// <summary>
/// Soft-deletable entity that supports logical deletion instead of physical deletion.
/// </summary>
public abstract class SoftDeletableEntity : AuditableEntity
{
    /// <summary>
    /// Indicates if this entity has been soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// When the entity was soft-deleted (UTC).
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Who soft-deleted the entity (user ID or system identifier).
    /// </summary>
    public string? DeletedBy { get; set; }

    /// <summary>
    /// Soft delete this entity.
    /// </summary>
    /// <param name="deletedBy">Who is deleting the entity.</param>
    public virtual void Delete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    /// <summary>
    /// Restore a soft-deleted entity.
    /// </summary>
    public virtual void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }
}

/// <summary>
/// Entity with GUID-based ID for scenarios requiring globally unique identifiers.
/// </summary>
public abstract class GuidEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
}

/// <summary>
/// Auditable entity with GUID ID.
/// </summary>
public abstract class AuditableGuidEntity : GuidEntity
{
    /// <summary>
    /// When the entity was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Who created the entity (user ID or system identifier).
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// When the entity was last updated (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Who last updated the entity (user ID or system identifier).
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Marks the entity as updated.
    /// </summary>
    /// <param name="updatedBy">Who is updating the entity.</param>
    public virtual void MarkUpdated(string? updatedBy = null)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
}
