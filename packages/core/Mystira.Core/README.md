# Mystira.Core

Core internal types for the Mystira platform. This package contains foundational abstractions that are used internally and not exposed via APIs.

## Features

### Result Pattern

Type-safe error handling without exceptions for expected failure cases:

```csharp
using Mystira.Core.Results;

// Success
Result<User> result = Result<User>.Success(user);

// Failure
Result<User> result = Error.NotFound("User", userId);

// Pattern matching
var message = result.Match(
    onSuccess: user => $"Found user: {user.Name}",
    onFailure: error => $"Error: {error.Message}"
);

// Chaining operations
var result = await GetUserAsync(id)
    .Bind(user => ValidateUser(user))
    .Map(user => user.ToDto())
    .Tap(dto => _logger.LogInformation("User: {Id}", dto.Id));
```

### Domain Primitives

Base classes for DDD-style domain modeling:

```csharp
using Mystira.Core.Domain;

// Entity with identity
public class Story : Entity<Guid>
{
    public string Title { get; private set; }
    public string Content { get; private set; }

    public void UpdateTitle(string title)
    {
        Title = title;
        MarkModified();
    }
}

// Value object
public class Email : SingleValueObject<string>
{
    private Email(string value) : base(value) { }

    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Error.Validation("Email cannot be empty");

        if (!value.Contains('@'))
            return Error.Validation("Email must contain @");

        return new Email(value);
    }
}
```

## Installation

```bash
dotnet add package Mystira.Core
```

## When to Use

Use `Mystira.Core` for:
- Internal error handling with `Result<T>`
- Domain entities and value objects
- Foundational types not exposed via APIs

Use `Mystira.Contracts` (generated from OpenAPI) for:
- API request/response types
- DTOs exposed to clients
- Types that need to be serialized to JSON

## License

MIT
