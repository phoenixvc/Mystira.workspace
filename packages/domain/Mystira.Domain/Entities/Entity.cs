namespace Mystira.Domain.Entities;

/// <summary>
/// Base class for entities with a string ID.
/// </summary>
public abstract class Entity : IEntity<string>, IAuditableEntity
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public string Id { get; set; } = EntityId.NewId();

    /// <summary>
    /// Gets or sets when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets who created the entity.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets who last updated the entity.
    /// </summary>
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Base class for entities with a typed ID.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public abstract class Entity<TId> : IEntity<TId>, IAuditableEntity where TId : notnull
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public TId Id { get; set; } = default!;

    /// <summary>
    /// Gets or sets when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets who created the entity.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets who last updated the entity.
    /// </summary>
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Base class for soft-deletable entities.
/// </summary>
public abstract class SoftDeletableEntity : Entity, ISoftDeletable
{
    /// <summary>
    /// Gets or sets whether the entity is deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets when the entity was deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets who deleted the entity.
    /// </summary>
    public string? DeletedBy { get; set; }
}
