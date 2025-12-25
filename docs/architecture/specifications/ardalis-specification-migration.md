# Ardalis.Specification 8.0.0 Migration Guide

**Status**: Implementation Ready
**Version**: 1.0
**Last Updated**: 2025-12-22
**Phase**: 1.6 - Specification Pattern Migration

## Overview

This document provides the implementation guide for migrating to Ardalis.Specification 8.0.0 pattern across all Mystira services. The migration introduces a clean separation between query logic and repository implementations.

## Package Requirements

```xml
<PackageReference Include="Ardalis.Specification" Version="8.0.0" />
<PackageReference Include="Ardalis.Specification.EntityFrameworkCore" Version="8.0.0" />
```

## Specification Implementations

### 1. Account Specifications

#### AccountByIdSpec

```csharp
using Ardalis.Specification;

namespace Mystira.Domain.Specifications;

/// <summary>
/// Specification to find an account by its unique identifier.
/// </summary>
public sealed class AccountByIdSpec : Specification<Account>, ISingleResultSpecification<Account>
{
    public AccountByIdSpec(string accountId)
    {
        Query
            .Where(a => a.Id == accountId)
            .AsNoTracking();
    }
}
```

#### AccountByEmailSpec

```csharp
using Ardalis.Specification;

namespace Mystira.Domain.Specifications;

/// <summary>
/// Specification to find an account by email address (case-insensitive).
/// </summary>
public sealed class AccountByEmailSpec : Specification<Account>, ISingleResultSpecification<Account>
{
    public AccountByEmailSpec(string email)
    {
        Query
            .Where(a => a.Email.ToLower() == email.ToLower())
            .AsNoTracking();
    }
}
```

#### AccountByStatusSpec

```csharp
using Ardalis.Specification;

namespace Mystira.Domain.Specifications;

/// <summary>
/// Specification to find accounts by status.
/// </summary>
public sealed class AccountByStatusSpec : Specification<Account>
{
    public AccountByStatusSpec(AccountStatus status)
    {
        Query
            .Where(a => a.Status == status)
            .OrderByDescending(a => a.CreatedAt)
            .AsNoTracking();
    }
}
```

#### ActiveAccountsSpec

```csharp
using Ardalis.Specification;

namespace Mystira.Domain.Specifications;

/// <summary>
/// Specification to retrieve all active accounts with pagination.
/// </summary>
public sealed class ActiveAccountsSpec : Specification<Account>
{
    public ActiveAccountsSpec(int page = 1, int pageSize = 20)
    {
        Query
            .Where(a => a.Status == AccountStatus.Active)
            .OrderByDescending(a => a.LastLoginAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking();
    }
}
```

### 2. UserProfile Specifications

#### UserProfileByAccountIdSpec

```csharp
using Ardalis.Specification;

namespace Mystira.Domain.Specifications;

/// <summary>
/// Specification to find a user profile by account ID.
/// </summary>
public sealed class UserProfileByAccountIdSpec : Specification<UserProfile>, ISingleResultSpecification<UserProfile>
{
    public UserProfileByAccountIdSpec(string accountId)
    {
        Query
            .Where(p => p.AccountId == accountId)
            .Include(p => p.Preferences)
            .AsNoTracking();
    }
}
```

#### UserProfileByUsernameSpec

```csharp
using Ardalis.Specification;

namespace Mystira.Domain.Specifications;

/// <summary>
/// Specification to find a user profile by username.
/// </summary>
public sealed class UserProfileByUsernameSpec : Specification<UserProfile>, ISingleResultSpecification<UserProfile>
{
    public UserProfileByUsernameSpec(string username)
    {
        Query
            .Where(p => p.Username.ToLower() == username.ToLower())
            .AsNoTracking();
    }
}
```

#### UserProfilesWithRecentActivitySpec

```csharp
using Ardalis.Specification;

namespace Mystira.Domain.Specifications;

/// <summary>
/// Specification to find profiles with activity in the last N days.
/// </summary>
public sealed class UserProfilesWithRecentActivitySpec : Specification<UserProfile>
{
    public UserProfilesWithRecentActivitySpec(int days = 30, int page = 1, int pageSize = 50)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);

        Query
            .Where(p => p.LastActivityAt >= cutoffDate)
            .OrderByDescending(p => p.LastActivityAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking();
    }
}
```

### 3. GameSession Specifications

#### GameSessionByIdSpec

```csharp
using Ardalis.Specification;

namespace Mystira.Domain.Specifications;

/// <summary>
/// Specification to find a game session by ID with related data.
/// </summary>
public sealed class GameSessionByIdSpec : Specification<GameSession>, ISingleResultSpecification<GameSession>
{
    public GameSessionByIdSpec(string sessionId, bool includeEvents = false)
    {
        Query
            .Where(s => s.Id == sessionId)
            .Include(s => s.Scenario);

        if (includeEvents)
        {
            Query.Include(s => s.Events);
        }

        Query.AsNoTracking();
    }
}
```

#### ActiveGameSessionsSpec

```csharp
using Ardalis.Specification;

namespace Mystira.Domain.Specifications;

/// <summary>
/// Specification to find all active (in-progress) game sessions.
/// </summary>
public sealed class ActiveGameSessionsSpec : Specification<GameSession>
{
    public ActiveGameSessionsSpec(int page = 1, int pageSize = 20)
    {
        Query
            .Where(s => s.Status == SessionStatus.InProgress)
            .OrderByDescending(s => s.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking();
    }
}
```

#### GameSessionsByAccountIdSpec

```csharp
using Ardalis.Specification;

namespace Mystira.Domain.Specifications;

/// <summary>
/// Specification to find all game sessions for a specific account.
/// </summary>
public sealed class GameSessionsByAccountIdSpec : Specification<GameSession>
{
    public GameSessionsByAccountIdSpec(string accountId, SessionStatus? status = null)
    {
        Query.Where(s => s.AccountId == accountId);

        if (status.HasValue)
        {
            Query.Where(s => s.Status == status.Value);
        }

        Query
            .OrderByDescending(s => s.StartedAt)
            .Include(s => s.Scenario)
            .AsNoTracking();
    }
}
```

#### CompletedSessionsInDateRangeSpec

```csharp
using Ardalis.Specification;

namespace Mystira.Domain.Specifications;

/// <summary>
/// Specification to find completed sessions within a date range (for analytics).
/// </summary>
public sealed class CompletedSessionsInDateRangeSpec : Specification<GameSession>
{
    public CompletedSessionsInDateRangeSpec(DateTime startDate, DateTime endDate)
    {
        Query
            .Where(s => s.Status == SessionStatus.Completed)
            .Where(s => s.CompletedAt >= startDate && s.CompletedAt <= endDate)
            .OrderBy(s => s.CompletedAt)
            .AsNoTracking();
    }
}
```

### 4. Scenario Specifications

#### ScenarioByIdSpec

```csharp
using Ardalis.Specification;

namespace Mystira.Domain.Specifications;

/// <summary>
/// Specification to find a scenario by ID with optional includes.
/// </summary>
public sealed class ScenarioByIdSpec : Specification<Scenario>, ISingleResultSpecification<Scenario>
{
    public ScenarioByIdSpec(string scenarioId, bool includeChoices = true)
    {
        Query.Where(s => s.Id == scenarioId);

        if (includeChoices)
        {
            Query.Include(s => s.Choices);
        }

        Query.AsNoTracking();
    }
}
```

#### PublishedScenariosSpec

```csharp
using Ardalis.Specification;

namespace Mystira.Domain.Specifications;

/// <summary>
/// Specification to find all published scenarios with pagination.
/// </summary>
public sealed class PublishedScenariosSpec : Specification<Scenario>
{
    public PublishedScenariosSpec(int page = 1, int pageSize = 20, string? category = null)
    {
        Query.Where(s => s.Status == ScenarioStatus.Published);

        if (!string.IsNullOrEmpty(category))
        {
            Query.Where(s => s.Category == category);
        }

        Query
            .OrderByDescending(s => s.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking();
    }
}
```

#### ScenariosByAuthorSpec

```csharp
using Ardalis.Specification;

namespace Mystira.Domain.Specifications;

/// <summary>
/// Specification to find scenarios by author ID.
/// </summary>
public sealed class ScenariosByAuthorSpec : Specification<Scenario>
{
    public ScenariosByAuthorSpec(string authorId, bool includeUnpublished = false)
    {
        Query.Where(s => s.AuthorId == authorId);

        if (!includeUnpublished)
        {
            Query.Where(s => s.Status == ScenarioStatus.Published);
        }

        Query
            .OrderByDescending(s => s.UpdatedAt)
            .AsNoTracking();
    }
}
```

#### FeaturedScenariosSpec

```csharp
using Ardalis.Specification;

namespace Mystira.Domain.Specifications;

/// <summary>
/// Specification to find featured scenarios for homepage.
/// </summary>
public sealed class FeaturedScenariosSpec : Specification<Scenario>
{
    public FeaturedScenariosSpec(int limit = 10)
    {
        Query
            .Where(s => s.Status == ScenarioStatus.Published)
            .Where(s => s.IsFeatured)
            .OrderByDescending(s => s.FeaturedAt)
            .Take(limit)
            .AsNoTracking();
    }
}
```

## Repository Interface with Specification Support

```csharp
using Ardalis.Specification;

namespace Mystira.Domain.Interfaces;

/// <summary>
/// Read-only repository interface with specification support.
/// </summary>
public interface IReadRepository<T> : IReadRepositoryBase<T> where T : class
{
    // Inherits from Ardalis.Specification.IReadRepositoryBase<T>:
    // - Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken ct = default)
    // - Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken ct = default)
    // - Task<T?> SingleOrDefaultAsync(ISingleResultSpecification<T> specification, CancellationToken ct = default)
    // - Task<TResult?> SingleOrDefaultAsync<TResult>(ISingleResultSpecification<T, TResult> specification, CancellationToken ct = default)
    // - Task<List<T>> ListAsync(ISpecification<T> specification, CancellationToken ct = default)
    // - Task<List<TResult>> ListAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken ct = default)
    // - Task<int> CountAsync(ISpecification<T> specification, CancellationToken ct = default)
    // - Task<bool> AnyAsync(ISpecification<T> specification, CancellationToken ct = default)
}

/// <summary>
/// Full repository interface with read and write operations.
/// </summary>
public interface IRepository<T> : IRepositoryBase<T>, IReadRepository<T> where T : class
{
    // Inherits from Ardalis.Specification.IRepositoryBase<T>:
    // - Task<T> AddAsync(T entity, CancellationToken ct = default)
    // - Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    // - Task UpdateAsync(T entity, CancellationToken ct = default)
    // - Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    // - Task DeleteAsync(T entity, CancellationToken ct = default)
    // - Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    // - Task<int> SaveChangesAsync(CancellationToken ct = default)
}
```

## Deprecated Specifications

The following old specifications should be marked as obsolete and replaced:

```csharp
// Mark old specifications with [Obsolete] attribute
[Obsolete("Use AccountByIdSpec instead. Will be removed in v3.0.")]
public class GetAccountByIdQuery { }

[Obsolete("Use AccountByEmailSpec instead. Will be removed in v3.0.")]
public class GetAccountByEmailQuery { }

[Obsolete("Use AccountByStatusSpec instead. Will be removed in v3.0.")]
public class GetAccountsByStatusQuery { }

[Obsolete("Use UserProfileByAccountIdSpec instead. Will be removed in v3.0.")]
public class GetUserProfileQuery { }

[Obsolete("Use GameSessionByIdSpec instead. Will be removed in v3.0.")]
public class GetGameSessionQuery { }

[Obsolete("Use ScenarioByIdSpec instead. Will be removed in v3.0.")]
public class GetScenarioQuery { }
```

## Usage Examples

### In Application Services

```csharp
public class AccountService
{
    private readonly IReadRepository<Account> _repository;

    public AccountService(IReadRepository<Account> repository)
    {
        _repository = repository;
    }

    public async Task<Account?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var spec = new AccountByIdSpec(id);
        return await _repository.SingleOrDefaultAsync(spec, ct);
    }

    public async Task<Account?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var spec = new AccountByEmailSpec(email);
        return await _repository.SingleOrDefaultAsync(spec, ct);
    }

    public async Task<List<Account>> GetActiveAccountsAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var spec = new ActiveAccountsSpec(page, pageSize);
        return await _repository.ListAsync(spec, ct);
    }
}
```

### In Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IReadRepository<Account> _repository;

    public AccountsController(IReadRepository<Account> repository)
    {
        _repository = repository;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AccountDto>> GetById(string id, CancellationToken ct)
    {
        var spec = new AccountByIdSpec(id);
        var account = await _repository.SingleOrDefaultAsync(spec, ct);

        if (account is null)
            return NotFound();

        return Ok(account.ToDto());
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AccountDto>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var spec = new ActiveAccountsSpec(page, pageSize);
        var accounts = await _repository.ListAsync(spec, ct);
        var count = await _repository.CountAsync(new ActiveAccountsSpec(1, int.MaxValue), ct);

        return Ok(new PagedResult<AccountDto>(
            accounts.Select(a => a.ToDto()),
            count,
            page,
            pageSize));
    }
}
```

## Unit Test Examples

Tests should be placed in corresponding test projects with the naming convention `*Spec.Tests.cs`.

```csharp
using Ardalis.Specification;
using FluentAssertions;
using Xunit;

namespace Mystira.Domain.Tests.Specifications;

public class AccountByIdSpecTests
{
    [Fact]
    public void Query_SetsCorrectWhereClause()
    {
        // Arrange
        var accountId = "acc_123";

        // Act
        var spec = new AccountByIdSpec(accountId);

        // Assert
        spec.WhereExpressions.Should().HaveCount(1);
    }

    [Fact]
    public void Query_EnablesNoTracking()
    {
        // Arrange & Act
        var spec = new AccountByIdSpec("acc_123");

        // Assert
        spec.AsNoTracking.Should().BeTrue();
    }
}

public class AccountByEmailSpecTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("USER@EXAMPLE.COM")]
    [InlineData("User@Example.Com")]
    public void Query_IsCaseInsensitive(string email)
    {
        // Arrange & Act
        var spec = new AccountByEmailSpec(email);

        // Assert
        spec.WhereExpressions.Should().HaveCount(1);
    }
}

public class ActiveAccountsSpecTests
{
    [Fact]
    public void Query_AppliesPagination()
    {
        // Arrange
        var page = 2;
        var pageSize = 10;

        // Act
        var spec = new ActiveAccountsSpec(page, pageSize);

        // Assert
        spec.Skip.Should().Be(10);
        spec.Take.Should().Be(10);
    }

    [Fact]
    public void Query_OrdersByLastLoginDescending()
    {
        // Arrange & Act
        var spec = new ActiveAccountsSpec();

        // Assert
        spec.OrderExpressions.Should().HaveCount(1);
    }
}
```

## Migration Checklist

- [ ] Add Ardalis.Specification NuGet packages
- [ ] Create base specification classes
- [ ] Implement Account specifications (4 specs)
- [ ] Implement UserProfile specifications (3 specs)
- [ ] Implement GameSession specifications (4 specs)
- [ ] Implement Scenario specifications (4 specs)
- [ ] Create repository interfaces with specification support
- [ ] Add [Obsolete] attributes to old query classes
- [ ] Write unit tests (minimum 25 tests)
- [ ] Update DI registration
- [ ] Update existing services to use specifications
- [ ] Remove deprecated code after verification period

## References

- [Ardalis.Specification Documentation](https://specification.ardalis.com/)
- [Ardalis.Specification GitHub](https://github.com/ardalis/Specification)
- [ADR-0014: Polyglot Persistence Framework Selection](../adr/0014-polyglot-persistence-framework-selection.md)
- [Master Implementation Checklist](../../planning/master-implementation-checklist.md)
