namespace Mystira.Core.Domain;

/// <summary>
/// Base class for domain entities with identity.
/// </summary>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    public TId Id { get; protected set; } = default!;

    /// <summary>
    /// Gets the date and time when the entity was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the entity was last modified.
    /// </summary>
    public DateTimeOffset? ModifiedAt { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TId}"/> class.
    /// </summary>
    protected Entity() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity{TId}"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    protected Entity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Marks the entity as modified.
    /// </summary>
    protected void MarkModified()
    {
        ModifiedAt = DateTimeOffset.UtcNow;
    }

    /// <inheritdoc />
    public bool Equals(Entity<TId>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as Entity<TId>);

    /// <inheritdoc />
    public override int GetHashCode() => 
        Id is null ? 0 : Id.GetHashCode();

    /// <summary>
    /// Determines whether two entities are equal.
    /// </summary>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>
    /// Determines whether two entities are not equal.
    /// </summary>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !(left == right);
}

/// <summary>
/// Base class for domain entities with a GUID identifier.
/// </summary>
public abstract class Entity : Entity<Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Entity"/> class with a new GUID.
    /// </summary>
    protected Entity() : base(Guid.NewGuid()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Entity"/> class with the specified GUID.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    protected Entity(Guid id) : base(id) { }
}
