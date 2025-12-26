# Mystira.Shared CQRS

This module provides marker interfaces for the Command Query Responsibility Segregation (CQRS) pattern, designed for use with Wolverine.

## Overview

CQRS separates read operations (queries) from write operations (commands):

- **Commands** modify state and may or may not return a result
- **Queries** return data without modifying state

## Interfaces

| Interface | Purpose | Returns |
|-----------|---------|---------|
| `ICommand` | Write operation without result | Nothing |
| `ICommand<TResponse>` | Write operation with result | TResponse |
| `IQuery<TResponse>` | Read operation | TResponse |

## Required Imports

```csharp
using Mystira.Shared.CQRS;           // ICommand, ICommand<T>, IQuery<T>
using Mystira.Shared.Validation;     // IValidatable (for automatic validation)
```

## Why Use These Interfaces?

Wolverine discovers handlers by convention, so these interfaces are **optional**. However, they provide:

1. **Self-documenting code** - Clear intent at the type level
2. **Policy application** - Apply validation, logging, or caching to all commands/queries
3. **IDE support** - Easy filtering and navigation
4. **Consistency** - Workspace-wide conventions

## Usage Examples

### Commands

```csharp
using Mystira.Shared.CQRS;
using Mystira.Shared.Validation;

// Command without result
public record DeleteUserCommand(Guid UserId) : ICommand;

// Command with result (returns created entity)
public record CreateUserCommand(string Name, string Email)
    : ICommand<User>, IValidatable;

// Command handlers (static class, discovered by Wolverine)
public static class UserCommandHandlers
{
    public static async Task Handle(
        DeleteUserCommand command,
        IUserRepository repository,
        CancellationToken ct)
    {
        await repository.DeleteAsync(command.UserId, ct);
    }

    public static async Task<User> Handle(
        CreateUserCommand command,
        IUserRepository repository,
        CancellationToken ct)
    {
        var user = new User(command.Name, command.Email);
        await repository.AddAsync(user, ct);
        return user;
    }
}
```

### Queries

```csharp
using Mystira.Shared.CQRS;

// Simple query
public record GetUserByIdQuery(Guid UserId) : IQuery<User?>;

// Paginated query
public record ListUsersQuery(int Page, int PageSize)
    : IQuery<PagedResult<UserSummary>>;

// Query with filtering
public record SearchUsersQuery(
    string? SearchTerm,
    bool? IsActive,
    string? SortBy)
    : IQuery<IReadOnlyList<UserSummary>>;

// Query handlers
public static class UserQueryHandlers
{
    public static async Task<User?> Handle(
        GetUserByIdQuery query,
        IUserRepository repository,
        CancellationToken ct)
    {
        return await repository.GetByIdAsync(query.UserId, ct);
    }

    public static async Task<PagedResult<UserSummary>> Handle(
        ListUsersQuery query,
        IUserRepository repository,
        CancellationToken ct)
    {
        return await repository.ListPagedAsync(query.Page, query.PageSize, ct);
    }
}
```

## Combining with Other Interfaces

Commands and queries can implement multiple interfaces:

```csharp
// Validated command
public record CreateOrderCommand(OrderRequest Request)
    : ICommand<Order>, IValidatable;

// Cached query (using custom attribute)
[CacheFor(Minutes = 5)]
public record GetProductCatalogQuery : IQuery<ProductCatalog>;
```

## Wolverine Configuration

Configure Wolverine to apply policies based on these interfaces:

```csharp
builder.Host.UseWolverine(opts =>
{
    // Apply validation to all IValidatable messages
    opts.UseFluentValidation();

    // Apply custom middleware to all commands
    opts.Policies.ForMessagesOfType<ICommand>()
        .AddMiddleware<AuditMiddleware>();

    // Apply caching to all queries
    opts.Policies.ForMessagesOfType(typeof(IQuery<>))
        .AddMiddleware<CacheMiddleware>();
});
```

## Migration from Existing Interfaces

If you have existing CQRS interfaces in your application:

1. Update imports: `using Mystira.Shared.CQRS;`
2. Remove local interface definitions
3. Verify handlers still work (Wolverine uses convention-based discovery)

The interfaces are semantically identical, so no behavioral changes are expected.

## Related

- `Mystira.Shared.Validation` - For command validation
- `Mystira.Shared.Messaging` - For domain and integration events
