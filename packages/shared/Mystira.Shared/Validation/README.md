# Mystira.Shared Validation

This module provides FluentValidation integration with Wolverine middleware for automatic command/query validation.

## Features

- **Automatic Validation**: Messages implementing `IValidatable` are validated before handlers execute
- **Declarative Rules**: Use FluentValidation's fluent API for clean, maintainable validation
- **Consistent Errors**: Validation failures are converted to RFC 7807 Problem Details via GlobalExceptionHandler
- **Testable**: Validators can be unit tested in isolation

## Quick Start

### 1. Mark Commands as Validatable

```csharp
using Mystira.Shared.CQRS;
using Mystira.Shared.Validation;

public record CreateUserCommand(string Name, string Email, int Age)
    : ICommand<User>, IValidatable;
```

### 2. Create a Validator

```csharp
using FluentValidation;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(IUserRepository repository)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MustAsync(async (email, ct) => !await repository.EmailExistsAsync(email, ct))
            .WithMessage("Email already in use");

        RuleFor(x => x.Age)
            .InclusiveBetween(0, 150).WithMessage("Age must be between 0 and 150");
    }
}
```

### 3. Register Validators

```csharp
// In Program.cs or Startup
builder.Services.AddValidation<CreateUserCommandValidator>();

// Or register from multiple assemblies
builder.Services.AddValidation(
    typeof(CreateUserCommandValidator),
    typeof(OtherValidator));
```

### 4. Configure Wolverine Middleware

```csharp
builder.Host.UseWolverine(opts =>
{
    // Enable automatic validation for IValidatable messages
    opts.UseFluentValidation();

    // Other Wolverine configuration...
});
```

## Error Response Format

When validation fails, a `ValidationException` is thrown and converted by `GlobalExceptionHandler` to:

```json
{
    "type": "https://tools.ietf.org/html/rfc7807",
    "title": "Validation Error",
    "status": 400,
    "detail": "One or more validation errors occurred.",
    "errors": {
        "Name": ["Name is required"],
        "Email": ["Invalid email format", "Email already in use"],
        "Age": ["Age must be between 0 and 150"]
    }
}
```

## Advanced Usage

### Conditional Validation

```csharp
public class OrderValidator : AbstractValidator<CreateOrderCommand>
{
    public OrderValidator()
    {
        // Only validate shipping address for physical products
        When(x => x.IsPhysicalProduct, () =>
        {
            RuleFor(x => x.ShippingAddress).NotEmpty();
            RuleFor(x => x.ShippingAddress.PostalCode).NotEmpty();
        });
    }
}
```

### Collection Validation

```csharp
public class OrderValidator : AbstractValidator<CreateOrderCommand>
{
    public OrderValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must have at least one item");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}
```

### Custom Validators

```csharp
public static class CustomValidators
{
    public static IRuleBuilderOptions<T, string> MustBeValidSlug<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("'{PropertyName}' must be a valid URL slug");
    }
}

// Usage
RuleFor(x => x.Slug).MustBeValidSlug();
```

### Manual Validation

For scenarios outside the message pipeline:

```csharp
public class MyService
{
    private readonly IServiceProvider _serviceProvider;

    public async Task ProcessAsync(CreateUserCommand command)
    {
        // Validate and throw if invalid
        await _serviceProvider.ValidateAndThrowAsync(command);

        // Or get validation result
        var result = await _serviceProvider.ValidateAsync(command);
        if (!result.IsValid)
        {
            // Handle errors manually
            foreach (var error in result.Errors)
            {
                _logger.LogWarning("Validation error: {Property} - {Message}",
                    error.PropertyName, error.ErrorMessage);
            }
        }
    }
}
```

## Testing Validators

```csharp
public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator;

    public CreateUserCommandValidatorTests()
    {
        var mockRepo = new Mock<IUserRepository>();
        mockRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), default))
            .ReturnsAsync(false);

        _validator = new CreateUserCommandValidator(mockRepo.Object);
    }

    [Fact]
    public async Task Should_Fail_When_Name_Is_Empty()
    {
        var command = new CreateUserCommand("", "test@example.com", 25);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task Should_Pass_When_Valid()
    {
        var command = new CreateUserCommand("John", "john@example.com", 25);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }
}
```

## Best Practices

1. **One Validator Per Command** - Keep validators focused and cohesive
2. **Use Dependency Injection** - Inject repositories for async validation rules
3. **Descriptive Messages** - Include property names and expected values in messages
4. **Error Codes** - Use `.WithErrorCode()` for programmatic error handling
5. **Test Thoroughly** - Validators are easy to unit test, so aim for high coverage
