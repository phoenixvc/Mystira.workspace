# Mystira.Shared Mapping

This module provides compile-time object mapping using [Mapperly](https://mapperly.riok.app/), a Roslyn source generator.

## Why Mapperly?

| Feature | AutoMapper | Mapperly |
|---------|------------|----------|
| Runtime Overhead | Reflection-based | Zero (compile-time) |
| AOT Compatible | Limited | Full support |
| Debugging | Opaque | View generated code |
| IDE Support | Runtime errors | Compile-time errors |
| Configuration | Runtime profiles | Attributes |
| .NET 9 Native AOT | Requires config | Works out of box |

## Usage

### 1. Create a Mapper Class

```csharp
using Riok.Mapperly.Abstractions;
using Mystira.Shared.Mapping;

namespace MyApp.Mapping;

[Mapper]
public static partial class UserMapper
{
    // Auto-mapped by convention (matching property names)
    public static partial UserResponse ToResponse(this User entity);

    // Custom mapping with attribute
    [MapProperty(nameof(User.FullName), nameof(UserSummary.DisplayName))]
    public static partial UserSummary ToSummary(this User entity);

    // Request to domain entity
    public static partial User ToDomain(this CreateUserRequest request);
}
```

### 2. Use Extension Methods

```csharp
public async Task<UserResponse> GetUserAsync(Guid userId)
{
    var user = await _repository.GetByIdAsync(userId);
    return user?.ToResponse(); // Extension method from mapper
}

public async Task<IReadOnlyList<UserSummary>> GetUsersAsync()
{
    var users = await _repository.ListAsync();
    return users.Select(u => u.ToSummary()).ToList();
}
```

### 3. Advanced Mappings

```csharp
[Mapper]
public static partial class OrderMapper
{
    // Nested object mapping
    public static partial OrderResponse ToResponse(this Order order);

    // Custom value conversion
    [MapProperty(nameof(Order.Status), nameof(OrderResponse.StatusText))]
    public static partial OrderResponse ToResponseWithStatus(this Order order);

    // Ignore properties
    [MapperIgnoreSource(nameof(Order.InternalNotes))]
    public static partial OrderSummary ToSummary(this Order order);

    // Collection mapping
    public static partial List<OrderItemResponse> ToResponses(this ICollection<OrderItem> items);
}
```

## Interfaces

The module provides marker interfaces for implementing mappers:

- `IMapper<TSource, TDestination>` - Single-direction mapping
- `IBidirectionalMapper<T1, T2>` - Two-way mapping
- `ICollectionMapper<TSource, TDestination>` - Collection-aware mapping

These are optional - Mapperly works with static partial classes without implementing any interface.

## Migration from Manual Mapping

1. Identify files with `MapTo*` or `ToDto*` methods
2. Create a mapper class with `[Mapper]` attribute
3. Define partial methods matching your existing signatures
4. Replace manual implementations with generated calls
5. Remove old manual mapping code

## Best Practices

1. **One mapper per domain aggregate** - Keep related mappings together
2. **Use extension methods** - Makes mapping chainable and fluent
3. **Prefer static mappers** - No DI overhead, better performance
4. **Document complex mappings** - Add XML comments for non-obvious conversions
