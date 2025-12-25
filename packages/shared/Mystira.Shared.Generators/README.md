# Mystira.Shared.Generators

> **STATUS: EXPERIMENTAL - NOT RECOMMENDED FOR PRODUCTION USE**

This source generator project is available but **commented out by default**.

## Why It's Commented Out

The **Generic Repository + Ardalis.Specification** pattern covers 90% of use cases with:
- Zero boilerplate code
- Better testability
- Simpler debugging
- No Roslyn learning curve

## Recommended Approach Instead

```csharp
// Register generic repository (one line, covers all entities)
services.AddScoped(typeof(IRepository<>), typeof(RepositoryBase<>));

// Use specifications for complex queries
public class LowStockProductsSpec : Specification<Product>
{
    public LowStockProductsSpec(int threshold)
    {
        Query.Where(p => p.StockQuantity < threshold)
             .OrderBy(p => p.Name);
    }
}

// Usage
var products = await _repository.ListAsync(new LowStockProductsSpec(10));
```

## When To Consider Source Generators

Only enable if you have:
- 20+ repositories with similar patterns
- Custom methods that can't be expressed as specifications
- Team familiarity with Roslyn source generators

## Enabling Source Generators

If you decide to use them, uncomment in `Mystira.Shared.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\Mystira.Shared.Generators\Mystira.Shared.Generators.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

## Future Alternatives

Consider these modern AOP approaches instead:
- **.NET 8+ Interceptors** - Compile-time, type-safe
- **Scrutor** - Convention-based registration + decorators
- **Metalama** - Full compile-time AOP (licensed)

See: `docs/guides/entity-and-repository-guide.md` for detailed comparison.
