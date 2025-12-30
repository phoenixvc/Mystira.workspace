namespace Mystira.Domain.Entities;

/// <summary>
/// Base interface for all entities.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public interface IEntity<TId> where TId : notnull
{
    /// <summary>
    /// Gets the unique identifier for this entity.
    /// </summary>
    TId Id { get; }
}

/// <summary>
/// Interface for entities with audit information.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// Gets or sets when the entity was created.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the entity was last updated.
    /// </summary>
    DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets who created the entity.
    /// </summary>
    string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets who last updated the entity.
    /// </summary>
    string? UpdatedBy { get; set; }
}

/// <summary>
/// Interface for soft-deletable entities.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Gets or sets whether the entity is deleted.
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets when the entity was deleted.
    /// </summary>
    DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets who deleted the entity.
    /// </summary>
    string? DeletedBy { get; set; }
}
