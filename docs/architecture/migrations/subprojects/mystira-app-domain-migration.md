# Mystira.App.Domain Migration Guide

## Overview

This document outlines the changes required in `Mystira.App.Domain` to support the hybrid data strategy. The Domain layer contains entities and value objects that must work with both Cosmos DB and PostgreSQL.

## Current State

```
Mystira.App.Domain/
├── Models/
│   ├── Account.cs
│   ├── UserProfile.cs
│   ├── Scenario.cs
│   ├── GameSession.cs
│   ├── Badge.cs
│   ├── PendingSignup.cs
│   └── ...
├── Enums/
│   ├── AccountRole.cs
│   ├── SubscriptionType.cs
│   └── ...
├── ValueObjects/
│   └── ...
└── Data/
    ├── Archetypes.json
    ├── CoreAxes.json
    └── ...
```

## Key Principles

1. **Database Agnostic**: Entities should not contain database-specific attributes
2. **Clean Separation**: Use separate configuration classes for EF Core mappings
3. **Backwards Compatible**: Changes must not break existing Cosmos DB functionality
4. **Minimal Changes**: Prefer configuration over entity modification

## Required Changes

### Phase 1: Entity Adjustments

#### 1.1 Add Database-Agnostic Base Entity

```csharp
// Models/Base/Entity.cs
namespace Mystira.App.Domain.Models.Base;

/// <summary>
/// Base entity with common properties for both Cosmos DB and PostgreSQL.
/// </summary>
public abstract class Entity
{
    /// <summary>
    /// Unique identifier. Works with both Guid (PostgreSQL) and string (Cosmos).
    /// </summary>
    public virtual string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Timestamp when entity was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Timestamp when entity was last modified.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Soft delete support.
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Timestamp when entity was soft-deleted.
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }
}
```

#### 1.2 Add Audit Interface

```csharp
// Models/Base/IAuditable.cs
namespace Mystira.App.Domain.Models.Base;

/// <summary>
/// Interface for entities that support audit tracking.
/// </summary>
public interface IAuditable
{
    DateTimeOffset CreatedAt { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
    string? CreatedBy { get; set; }
    string? UpdatedBy { get; set; }
}
```

#### 1.3 Update Account Entity

```csharp
// Models/Account.cs
namespace Mystira.App.Domain.Models;

/// <summary>
/// User account entity.
/// </summary>
public class Account : Entity, IAuditable
{
    public string? Auth0UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public AccountRole Role { get; set; } = AccountRole.Guest;
    public AccountSettings Settings { get; set; } = new();
    public DateTimeOffset LastLoginAt { get; set; } = DateTimeOffset.UtcNow;

    // Audit fields
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties (used by PostgreSQL EF Core)
    public virtual Subscription? Subscription { get; set; }
    public virtual ICollection<UserProfile> Profiles { get; set; } = [];  // C# 12 collection expression
}

/// <summary>
/// Account settings stored as JSONB in PostgreSQL.
/// Uses record for immutability (C# 10+).
/// </summary>
public record AccountSettings
{
    public bool EmailNotifications { get; init; } = true;
    public bool PushNotifications { get; init; } = true;
    public string PreferredLanguage { get; init; } = "en";
    public string Theme { get; init; } = "default";
    public Dictionary<string, object> Custom { get; init; } = [];  // C# 12 collection expression
}
```

#### 1.4 Update UserProfile Entity

```csharp
// Models/UserProfile.cs
namespace Mystira.App.Domain.Models;

/// <summary>
/// User profile entity.
/// </summary>
public class UserProfile : Entity, IAuditable
{
    public string? AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AgeGroup { get; set; } = "6-9";
    public DateTime? DateOfBirth { get; set; }
    public bool IsGuest { get; set; }
    public bool IsNpc { get; set; }
    public string? Pronouns { get; set; }
    public string? Bio { get; set; }
    public string? AvatarMediaId { get; set; }
    public string? SelectedAvatarMediaId { get; set; }
    public ProfileThemes Themes { get; set; } = new();
    public bool Onboarded { get; set; }

    // Audit fields
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public virtual Account? Account { get; set; }
    public virtual ICollection<UserBadge> Badges { get; set; } = [];  // C# 12 collection expression
    public virtual ICollection<CompletedScenario> CompletedScenarios { get; set; } = [];
}

/// <summary>
/// Profile theme preferences stored as JSONB.
/// Uses record for immutability.
/// </summary>
public record ProfileThemes
{
    public string? SelectedEchoType { get; init; }
    public string? SelectedFantasyTheme { get; init; }
    public List<string> UnlockedThemes { get; init; } = [];  // C# 12 collection expression
}
```

#### 1.5 Add Subscription Entity (New)

```csharp
// Models/Subscription.cs
namespace Mystira.App.Domain.Models;

/// <summary>
/// Account subscription entity.
/// </summary>
public class Subscription : Entity
{
    public string AccountId { get; set; } = string.Empty;
    public SubscriptionType Type { get; set; } = SubscriptionType.Free;
    public string? ProductId { get; set; }
    public string Tier { get; set; } = "Free";
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? ValidUntil { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string? PurchaseToken { get; set; }
    public DateTimeOffset? LastVerified { get; set; }
    public List<string> PurchasedScenarios { get; set; } = [];  // C# 12 collection expression

    // Navigation
    public virtual Account? Account { get; set; }
}
```

### Phase 2: Add Value Objects

```csharp
// ValueObjects/Email.cs
namespace Mystira.App.Domain.ValueObjects;

/// <summary>
/// Email value object with validation.
/// </summary>
public readonly record struct Email
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty", nameof(value));

        if (!EmailRegex.IsMatch(value))
            throw new ArgumentException("Invalid email format", nameof(value));

        Value = value.ToLowerInvariant();
    }

    public static implicit operator string(Email email) => email.Value;
    public static explicit operator Email(string value) => new(value);
    public override string ToString() => Value;
}
```

```csharp
// ValueObjects/AccountId.cs
namespace Mystira.App.Domain.ValueObjects;

/// <summary>
/// Strongly-typed account identifier.
/// </summary>
public readonly record struct AccountId
{
    public Guid Value { get; }

    public AccountId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AccountId cannot be empty", nameof(value));
        Value = value;
    }

    public AccountId(string value)
    {
        if (!Guid.TryParse(value, out var guid) || guid == Guid.Empty)
            throw new ArgumentException("Invalid AccountId format", nameof(value));
        Value = guid;
    }

    public static AccountId New() => new(Guid.NewGuid());
    public static implicit operator Guid(AccountId id) => id.Value;
    public static implicit operator string(AccountId id) => id.Value.ToString();
    public override string ToString() => Value.ToString();
}
```

### Phase 3: Add Enums

```csharp
// Enums/MigrationPhase.cs
namespace Mystira.App.Domain.Enums;

/// <summary>
/// Data migration phase for hybrid strategy.
/// </summary>
public enum MigrationPhase
{
    /// <summary>All operations use Cosmos DB</summary>
    CosmosOnly = 0,

    /// <summary>Writes to both, reads from Cosmos DB</summary>
    DualWriteCosmosRead = 1,

    /// <summary>Writes to both, reads from PostgreSQL</summary>
    DualWritePostgresRead = 2,

    /// <summary>All operations use PostgreSQL</summary>
    PostgresOnly = 3
}
```

```csharp
// Enums/SyncOperation.cs
namespace Mystira.App.Domain.Enums;

/// <summary>
/// Sync queue operation type.
/// </summary>
public enum SyncOperation
{
    Insert,
    Update,
    Upsert,
    Delete
}
```

### Phase 4: Add Domain Events (Optional - For Event-Driven Architecture)

```csharp
// Events/DomainEvent.cs
namespace Mystira.App.Domain.Events;

/// <summary>
/// Base class for domain events.
/// </summary>
public abstract record DomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
    public string? CorrelationId { get; init; }
}

/// <summary>
/// Event raised when an account is created.
/// </summary>
public record AccountCreatedEvent(
    string AccountId,
    string Email,
    AccountRole Role) : DomainEvent;

/// <summary>
/// Event raised when a profile is updated.
/// </summary>
public record ProfileUpdatedEvent(
    string ProfileId,
    string AccountId,
    string[] ChangedProperties) : DomainEvent;

/// <summary>
/// Event raised when a game session is completed.
/// Uses required init properties for immutability.
/// </summary>
public record SessionCompletedEvent : DomainEvent
{
    public required string SessionId { get; init; }
    public required string AccountId { get; init; }
    public required string ScenarioId { get; init; }
    public required int Score { get; init; }
    public Dictionary<string, double> AxisScores { get; init; } = [];  // C# 12 collection expression
}
```

## Mapping Considerations

### Cosmos DB Mappings (Infrastructure.Data)

The existing Cosmos DB mappings in `MystiraAppDbContext` use `ToJsonProperty()` for JSON field names. These must be preserved:

```csharp
// Current mapping - DO NOT CHANGE
modelBuilder.Entity<Account>(entity =>
{
    entity.ToContainer("Accounts");
    entity.HasPartitionKey(a => a.Id);
    entity.Property(a => a.Id).ToJsonProperty("id");
    entity.Property(a => a.Email).ToJsonProperty("email");
    // ...
});
```

### PostgreSQL Mappings (Infrastructure.PostgreSQL)

New PostgreSQL mappings will use different conventions:

```csharp
// New PostgreSQL mapping
modelBuilder.Entity<Account>(entity =>
{
    entity.ToTable("accounts");
    entity.HasKey(a => a.Id);
    entity.Property(a => a.Id).HasColumnName("id").HasMaxLength(36);
    entity.Property(a => a.Email).HasColumnName("email").HasMaxLength(255);
    entity.Property(a => a.Settings).HasColumnType("jsonb");
    // ...
});
```

## Backwards Compatibility

### Preserved Behaviors

1. **String IDs**: Keep `string Id` for Cosmos DB compatibility
2. **JSON Serialization**: Complex properties remain serializable
3. **Partition Keys**: Entity structure supports Cosmos partition keys
4. **No Breaking Changes**: Existing API contracts unchanged

### New Capabilities

1. **Navigation Properties**: Added for PostgreSQL foreign keys
2. **Value Objects**: Type-safe domain primitives
3. **Domain Events**: Ready for event-driven architecture
4. **Audit Trail**: CreatedBy/UpdatedBy support

## Testing Checklist

### Unit Tests

- [ ] Entity creation with default values
- [ ] Value object validation
- [ ] Domain event creation

### Integration Tests

- [ ] Entities serialize to JSON correctly (Cosmos)
- [ ] Entities map to PostgreSQL tables correctly
- [ ] Navigation properties load correctly

## Dependencies

The Domain layer should have ZERO external dependencies:

```xml
<ItemGroup>
  <!-- Only .NET BCL references -->
  <!-- No EF Core, no database packages -->
</ItemGroup>
```

## See Also

- [PostgreSQL Schema](../user-domain-postgresql-migration.md)
- [Repository Architecture](../repository-architecture.md)
- [ADR-0013: Data Management Strategy](../../adr/0013-data-management-and-storage-strategy.md)
