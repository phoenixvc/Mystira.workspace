# Internal API Contracts

This document defines the internal API contracts between Mystira services, enabling consistent communication patterns and shared data models across the platform.

## Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           Service Communication                              │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────────┐     HTTP/gRPC      ┌──────────────┐                       │
│  │  App API     │◄──────────────────►│  Admin API   │                       │
│  │  (Public)    │                    │  (Internal)  │                       │
│  └──────┬───────┘                    └──────┬───────┘                       │
│         │                                   │                               │
│         │  ┌───────────────────────────────┼───────────────────────┐        │
│         │  │                               │                       │        │
│         ▼  ▼                               ▼                       │        │
│  ┌──────────────────────────────────────────────────────────┐      │        │
│  │                    Shared Contracts                       │      │        │
│  │  • Domain Events    • DTOs           • Health Checks     │      │        │
│  │  • Value Objects    • Enums          • Error Codes       │      │        │
│  └──────────────────────────────────────────────────────────┘      │        │
│         │                                                           │        │
│         ▼                                                           ▼        │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────────────────┐       │
│  │  Cosmos DB   │    │  PostgreSQL  │    │  Azure Service Bus       │       │
│  │  (Documents) │    │  (Relational)│    │  (Events/Messages)       │       │
│  └──────────────┘    └──────────────┘    └──────────────────────────┘       │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Shared Contract Packages

### Package Structure

```
src/
├── Mystira.App.Contracts/           # Shared contracts NuGet package
│   ├── DTOs/
│   │   ├── Accounts/
│   │   ├── Users/
│   │   ├── Scenarios/
│   │   └── GameSessions/
│   ├── Events/
│   │   ├── AccountEvents.cs
│   │   ├── UserProfileEvents.cs
│   │   └── GameSessionEvents.cs
│   ├── Enums/
│   │   ├── AccountStatus.cs
│   │   ├── SubscriptionTier.cs
│   │   └── MigrationPhase.cs
│   ├── ValueObjects/
│   │   ├── Email.cs
│   │   ├── AccountId.cs
│   │   └── ProfileId.cs
│   ├── Errors/
│   │   ├── ErrorCodes.cs
│   │   └── ProblemDetails.cs
│   └── Health/
│       ├── HealthStatus.cs
│       └── ServiceHealth.cs
```

---

## 1. Account Domain Contracts

### DTOs

```csharp
// Mystira.App.Contracts/DTOs/Accounts/AccountDto.cs
namespace Mystira.App.Contracts.DTOs.Accounts;

/// <summary>
/// Account data transfer object for inter-service communication.
/// </summary>
public record AccountDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required string Username { get; init; }
    public required AccountStatus Status { get; init; }
    public required SubscriptionTier SubscriptionTier { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastLoginAt { get; init; }
    public required bool EmailVerified { get; init; }
}

/// <summary>
/// Request to create a new account.
/// </summary>
public record CreateAccountRequest
{
    public required string Email { get; init; }
    public required string Username { get; init; }
    public required string PasswordHash { get; init; }
    public string? DisplayName { get; init; }
    public string? ReferralCode { get; init; }
}

/// <summary>
/// Response after account creation.
/// </summary>
public record CreateAccountResponse
{
    public required Guid AccountId { get; init; }
    public required string VerificationToken { get; init; }
    public required DateTimeOffset TokenExpiresAt { get; init; }
}

/// <summary>
/// Request to update account settings.
/// </summary>
public record UpdateAccountRequest
{
    public string? Username { get; init; }
    public string? DisplayName { get; init; }
    public NotificationPreferences? Notifications { get; init; }
    public PrivacySettings? Privacy { get; init; }
}
```

### Events

```csharp
// Mystira.App.Contracts/Events/AccountEvents.cs
namespace Mystira.App.Contracts.Events;

/// <summary>
/// Base class for all account-related domain events.
/// </summary>
public abstract record AccountEvent
{
    public required Guid AccountId { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
    public required string CorrelationId { get; init; }
}

/// <summary>
/// Raised when a new account is created.
/// </summary>
public record AccountCreatedEvent : AccountEvent
{
    public required string Email { get; init; }
    public required string Username { get; init; }
    public required SubscriptionTier InitialTier { get; init; }
}

/// <summary>
/// Raised when an account email is verified.
/// </summary>
public record AccountVerifiedEvent : AccountEvent
{
    public required DateTimeOffset VerifiedAt { get; init; }
}

/// <summary>
/// Raised when subscription tier changes.
/// </summary>
public record SubscriptionChangedEvent : AccountEvent
{
    public required SubscriptionTier PreviousTier { get; init; }
    public required SubscriptionTier NewTier { get; init; }
    public required string ChangeReason { get; init; }
}

/// <summary>
/// Raised when an account is deactivated.
/// </summary>
public record AccountDeactivatedEvent : AccountEvent
{
    public required string Reason { get; init; }
    public required bool SoftDelete { get; init; }
}
```

---

## 2. User Profile Contracts

### DTOs

```csharp
// Mystira.App.Contracts/DTOs/Users/UserProfileDto.cs
namespace Mystira.App.Contracts.DTOs.Users;

/// <summary>
/// User profile data transfer object.
/// </summary>
public record UserProfileDto
{
    public required Guid Id { get; init; }
    public required Guid AccountId { get; init; }
    public required string DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
    public string? Bio { get; init; }
    public CompassProfileDto? CompassProfile { get; init; }
    public IReadOnlyList<BadgeDto> Badges { get; init; } = [];
    public ProfileStatsDto? Stats { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

/// <summary>
/// Compass personality profile.
/// </summary>
public record CompassProfileDto
{
    public required Dictionary<string, double> AxisScores { get; init; }
    public required string PrimaryArchetype { get; init; }
    public string? SecondaryArchetype { get; init; }
    public required DateTimeOffset CalculatedAt { get; init; }
}

/// <summary>
/// User profile statistics.
/// </summary>
public record ProfileStatsDto
{
    public required int ScenariosCompleted { get; init; }
    public required int TotalPlayTimeMinutes { get; init; }
    public required int BadgesEarned { get; init; }
    public required int CurrentStreak { get; init; }
    public required int LongestStreak { get; init; }
}

/// <summary>
/// Badge information.
/// </summary>
public record BadgeDto
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string ImageUrl { get; init; }
    public required DateTimeOffset EarnedAt { get; init; }
}
```

### Events

```csharp
// Mystira.App.Contracts/Events/UserProfileEvents.cs
namespace Mystira.App.Contracts.Events;

/// <summary>
/// Base class for profile-related events.
/// </summary>
public abstract record ProfileEvent
{
    public required Guid ProfileId { get; init; }
    public required Guid AccountId { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
    public required string CorrelationId { get; init; }
}

/// <summary>
/// Raised when a user profile is created.
/// </summary>
public record ProfileCreatedEvent : ProfileEvent
{
    public required string DisplayName { get; init; }
}

/// <summary>
/// Raised when compass scores are updated.
/// </summary>
public record CompassScoresUpdatedEvent : ProfileEvent
{
    public required Dictionary<string, double> PreviousScores { get; init; }
    public required Dictionary<string, double> NewScores { get; init; }
    public required string CalculationSource { get; init; }
}

/// <summary>
/// Raised when a badge is earned.
/// </summary>
public record BadgeEarnedEvent : ProfileEvent
{
    public required Guid BadgeId { get; init; }
    public required string BadgeCode { get; init; }
    public required string TriggerAction { get; init; }
}

/// <summary>
/// Raised when profile avatar is updated.
/// </summary>
public record AvatarUpdatedEvent : ProfileEvent
{
    public string? PreviousAvatarUrl { get; init; }
    public required string NewAvatarUrl { get; init; }
}
```

---

## 3. Game Session Contracts

### DTOs

```csharp
// Mystira.App.Contracts/DTOs/GameSessions/GameSessionDto.cs
namespace Mystira.App.Contracts.DTOs.GameSessions;

/// <summary>
/// Game session data transfer object.
/// </summary>
public record GameSessionDto
{
    public required Guid Id { get; init; }
    public required Guid AccountId { get; init; }
    public required Guid ScenarioId { get; init; }
    public required SessionStatus Status { get; init; }
    public required DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public required int CurrentNodeIndex { get; init; }
    public IReadOnlyList<SessionChoiceDto> Choices { get; init; } = [];
    public SessionResultDto? Result { get; init; }
}

/// <summary>
/// Session status enum.
/// </summary>
public enum SessionStatus
{
    InProgress,
    Completed,
    Abandoned,
    Paused
}

/// <summary>
/// A choice made during a session.
/// </summary>
public record SessionChoiceDto
{
    public required int NodeIndex { get; init; }
    public required Guid ChoiceId { get; init; }
    public required string ChoiceText { get; init; }
    public required DateTimeOffset ChosenAt { get; init; }
    public required int ResponseTimeMs { get; init; }
}

/// <summary>
/// Session completion result.
/// </summary>
public record SessionResultDto
{
    public required Dictionary<string, double> AxisContributions { get; init; }
    public required int TotalScore { get; init; }
    public required int CompletionTimeSeconds { get; init; }
    public IReadOnlyList<string> BadgesUnlocked { get; init; } = [];
}
```

### Events

```csharp
// Mystira.App.Contracts/Events/GameSessionEvents.cs
namespace Mystira.App.Contracts.Events;

/// <summary>
/// Base class for game session events.
/// </summary>
public abstract record SessionEvent
{
    public required Guid SessionId { get; init; }
    public required Guid AccountId { get; init; }
    public required Guid ScenarioId { get; init; }
    public required DateTimeOffset OccurredAt { get; init; }
    public required string CorrelationId { get; init; }
}

/// <summary>
/// Raised when a new game session starts.
/// </summary>
public record SessionStartedEvent : SessionEvent
{
    public required string ScenarioName { get; init; }
    public required int TotalNodes { get; init; }
}

/// <summary>
/// Raised when a choice is made in a session.
/// </summary>
public record ChoiceMadeEvent : SessionEvent
{
    public required int NodeIndex { get; init; }
    public required Guid ChoiceId { get; init; }
    public required int ResponseTimeMs { get; init; }
}

/// <summary>
/// Raised when a session is completed.
/// </summary>
public record SessionCompletedEvent : SessionEvent
{
    public required int TotalTimeSeconds { get; init; }
    public required int TotalScore { get; init; }
    public required Dictionary<string, double> AxisContributions { get; init; }
    public IReadOnlyList<string> BadgesUnlocked { get; init; } = [];
}

/// <summary>
/// Raised when a session is abandoned.
/// </summary>
public record SessionAbandonedEvent : SessionEvent
{
    public required int LastNodeIndex { get; init; }
    public required string AbandonReason { get; init; }
}
```

---

## 4. Enums and Value Objects

### Enums

```csharp
// Mystira.App.Contracts/Enums/AccountStatus.cs
namespace Mystira.App.Contracts.Enums;

/// <summary>
/// Account status enumeration.
/// </summary>
public enum AccountStatus
{
    PendingVerification = 0,
    Active = 1,
    Suspended = 2,
    Deactivated = 3,
    Deleted = 4
}

/// <summary>
/// Subscription tier enumeration.
/// </summary>
public enum SubscriptionTier
{
    Free = 0,
    Basic = 1,
    Premium = 2,
    Enterprise = 3
}

/// <summary>
/// Data migration phase for hybrid data strategy.
/// </summary>
public enum MigrationPhase
{
    /// <summary>Cosmos DB only - no PostgreSQL</summary>
    CosmosOnly = 0,

    /// <summary>Dual-write to both, read from Cosmos</summary>
    DualWriteCosmosRead = 1,

    /// <summary>Dual-write to both, read from PostgreSQL</summary>
    DualWritePostgresRead = 2,

    /// <summary>PostgreSQL only - Cosmos for archive/legacy</summary>
    PostgresOnly = 3
}

/// <summary>
/// Storage tier for blob content.
/// </summary>
public enum StorageTier
{
    Hot = 0,
    Cool = 1,
    Archive = 2
}
```

### Value Objects

```csharp
// Mystira.App.Contracts/ValueObjects/Email.cs
namespace Mystira.App.Contracts.ValueObjects;

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
    public override string ToString() => Value;
}

// Mystira.App.Contracts/ValueObjects/AccountId.cs
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

    public static AccountId New() => new(Guid.NewGuid());
    public static implicit operator Guid(AccountId id) => id.Value;
    public override string ToString() => Value.ToString();
}
```

---

## 5. Error Contracts

### Error Codes

```csharp
// Mystira.App.Contracts/Errors/ErrorCodes.cs
namespace Mystira.App.Contracts.Errors;

/// <summary>
/// Standardized error codes for inter-service communication.
/// </summary>
public static class ErrorCodes
{
    // Account errors (1xxx)
    public const string AccountNotFound = "ACCOUNT_1001";
    public const string AccountAlreadyExists = "ACCOUNT_1002";
    public const string AccountSuspended = "ACCOUNT_1003";
    public const string AccountDeactivated = "ACCOUNT_1004";
    public const string InvalidCredentials = "ACCOUNT_1005";
    public const string EmailNotVerified = "ACCOUNT_1006";
    public const string VerificationTokenExpired = "ACCOUNT_1007";

    // Profile errors (2xxx)
    public const string ProfileNotFound = "PROFILE_2001";
    public const string ProfileAlreadyExists = "PROFILE_2002";
    public const string InvalidAvatarFormat = "PROFILE_2003";
    public const string AvatarSizeTooLarge = "PROFILE_2004";

    // Session errors (3xxx)
    public const string SessionNotFound = "SESSION_3001";
    public const string SessionAlreadyCompleted = "SESSION_3002";
    public const string InvalidSessionState = "SESSION_3003";
    public const string ScenarioNotAvailable = "SESSION_3004";

    // Scenario errors (4xxx)
    public const string ScenarioNotFound = "SCENARIO_4001";
    public const string ScenarioNotPublished = "SCENARIO_4002";
    public const string InsufficientSubscription = "SCENARIO_4003";

    // Storage errors (5xxx)
    public const string StorageError = "STORAGE_5001";
    public const string FileTooLarge = "STORAGE_5002";
    public const string InvalidFileType = "STORAGE_5003";

    // General errors (9xxx)
    public const string ValidationFailed = "GENERAL_9001";
    public const string InternalError = "GENERAL_9002";
    public const string ServiceUnavailable = "GENERAL_9003";
    public const string RateLimitExceeded = "GENERAL_9004";
}
```

### Problem Details

```csharp
// Mystira.App.Contracts/Errors/ApiProblemDetails.cs
namespace Mystira.App.Contracts.Errors;

/// <summary>
/// RFC 7807 Problem Details for HTTP APIs.
/// </summary>
public record ApiProblemDetails
{
    /// <summary>Error type URI</summary>
    public required string Type { get; init; }

    /// <summary>Short human-readable summary</summary>
    public required string Title { get; init; }

    /// <summary>HTTP status code</summary>
    public required int Status { get; init; }

    /// <summary>Detailed explanation</summary>
    public string? Detail { get; init; }

    /// <summary>URI identifying the specific occurrence</summary>
    public string? Instance { get; init; }

    /// <summary>Internal error code</summary>
    public string? ErrorCode { get; init; }

    /// <summary>Correlation ID for tracing</summary>
    public string? CorrelationId { get; init; }

    /// <summary>Validation errors by field</summary>
    public IDictionary<string, string[]>? Errors { get; init; }
}
```

---

## 6. Health Check Contracts

```csharp
// Mystira.App.Contracts/Health/HealthContracts.cs
namespace Mystira.App.Contracts.Health;

/// <summary>
/// Health check status.
/// </summary>
public enum HealthStatus
{
    Healthy = 0,
    Degraded = 1,
    Unhealthy = 2
}

/// <summary>
/// Individual health check result.
/// </summary>
public record HealthCheckResult
{
    public required string Name { get; init; }
    public required HealthStatus Status { get; init; }
    public string? Description { get; init; }
    public TimeSpan? Duration { get; init; }
    public IDictionary<string, object>? Data { get; init; }
}

/// <summary>
/// Aggregated service health report.
/// </summary>
public record ServiceHealthReport
{
    public required string ServiceName { get; init; }
    public required string Version { get; init; }
    public required HealthStatus Status { get; init; }
    public required DateTimeOffset CheckedAt { get; init; }
    public required TimeSpan TotalDuration { get; init; }
    public IReadOnlyList<HealthCheckResult> Checks { get; init; } = [];
}

/// <summary>
/// Database-specific health data.
/// </summary>
public record DatabaseHealthData
{
    public required string Provider { get; init; }
    public required bool IsConnected { get; init; }
    public TimeSpan? Latency { get; init; }
    public int? ActiveConnections { get; init; }
    public string? Version { get; init; }
}

/// <summary>
/// Cache-specific health data.
/// </summary>
public record CacheHealthData
{
    public required string Provider { get; init; }
    public required bool IsConnected { get; init; }
    public TimeSpan? Latency { get; init; }
    public long? UsedMemoryBytes { get; init; }
    public double? HitRate { get; init; }
}
```

---

## 7. Service Communication Patterns

### Internal HTTP Client Contract

```csharp
// Mystira.App.Contracts/Services/IInternalServiceClient.cs
namespace Mystira.App.Contracts.Services;

/// <summary>
/// Contract for internal service-to-service HTTP communication.
/// </summary>
public interface IInternalServiceClient
{
    /// <summary>
    /// Get account by ID from account service.
    /// </summary>
    Task<Result<AccountDto>> GetAccountAsync(Guid accountId, CancellationToken ct = default);

    /// <summary>
    /// Get user profile by account ID.
    /// </summary>
    Task<Result<UserProfileDto>> GetProfileByAccountAsync(Guid accountId, CancellationToken ct = default);

    /// <summary>
    /// Validate account subscription tier.
    /// </summary>
    Task<Result<bool>> ValidateSubscriptionAsync(Guid accountId, SubscriptionTier requiredTier, CancellationToken ct = default);
}

/// <summary>
/// Generic result wrapper for service calls.
/// </summary>
public record Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public ApiProblemDetails? Error { get; init; }

    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Failure(ApiProblemDetails error) => new() { IsSuccess = false, Error = error };
}
```

### Event Bus Contract

```csharp
// Mystira.App.Contracts/Services/IEventBus.cs
namespace Mystira.App.Contracts.Services;

/// <summary>
/// Contract for publishing domain events.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publish a domain event to the event bus.
    /// </summary>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class;

    /// <summary>
    /// Publish multiple events in a batch.
    /// </summary>
    Task PublishBatchAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken ct = default)
        where TEvent : class;
}

/// <summary>
/// Contract for consuming domain events.
/// </summary>
public interface IEventHandler<in TEvent> where TEvent : class
{
    /// <summary>
    /// Handle the incoming event.
    /// </summary>
    Task HandleAsync(TEvent @event, CancellationToken ct = default);
}
```

---

## 8. API Versioning

### Version Headers

All internal APIs must include the following headers:

| Header              | Description                  | Example           |
| ------------------- | ---------------------------- | ----------------- |
| `X-Api-Version`     | API version                  | `1.0`             |
| `X-Correlation-Id`  | Request correlation ID       | `uuid`            |
| `X-Service-Name`    | Calling service name         | `mystira-app-api` |
| `X-Migration-Phase` | Current data migration phase | `1`               |

### Version Compatibility

```csharp
// Mystira.App.Contracts/Versioning/ApiVersion.cs
namespace Mystira.App.Contracts.Versioning;

/// <summary>
/// API version information.
/// </summary>
public static class ApiVersions
{
    public const string V1 = "1.0";
    public const string V2 = "2.0";
    public const string Current = V1;
}

/// <summary>
/// Contract version for backwards compatibility checking.
/// </summary>
public record ContractVersion
{
    public required int Major { get; init; }
    public required int Minor { get; init; }

    public bool IsCompatibleWith(ContractVersion other)
        => Major == other.Major && Minor >= other.Minor;
}
```

---

## Usage Examples

### Publishing Events

```csharp
public class AccountService
{
    private readonly IEventBus _eventBus;

    public async Task<AccountDto> CreateAccountAsync(CreateAccountRequest request)
    {
        var account = // ... create account

        await _eventBus.PublishAsync(new AccountCreatedEvent
        {
            AccountId = account.Id,
            Email = account.Email,
            Username = account.Username,
            InitialTier = SubscriptionTier.Free,
            OccurredAt = DateTimeOffset.UtcNow,
            CorrelationId = Activity.Current?.Id ?? Guid.NewGuid().ToString()
        });

        return account;
    }
}
```

### Consuming Events

```csharp
public class ProfileCreationHandler : IEventHandler<AccountCreatedEvent>
{
    private readonly IProfileRepository _profiles;

    public async Task HandleAsync(AccountCreatedEvent @event, CancellationToken ct)
    {
        var profile = new UserProfile
        {
            Id = Guid.NewGuid(),
            AccountId = @event.AccountId,
            DisplayName = @event.Username,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _profiles.CreateAsync(profile, ct);
    }
}
```

### Service-to-Service Calls

```csharp
public class ScenarioAccessValidator
{
    private readonly IInternalServiceClient _serviceClient;

    public async Task<bool> CanAccessScenarioAsync(Guid accountId, Guid scenarioId)
    {
        var scenario = await _scenarioRepository.GetByIdAsync(scenarioId);
        if (scenario is null) return false;

        var result = await _serviceClient.ValidateSubscriptionAsync(
            accountId,
            scenario.RequiredTier);

        return result.IsSuccess && result.Value;
    }
}
```

## See Also

- [ADR-0013: Data Management and Storage Strategy](../adr/0013-data-management-and-storage-strategy.md)
- [Repository Architecture](../migrations/repository-architecture.md)
- [PostgreSQL Migration Guide](../migrations/user-domain-postgresql-migration.md)
