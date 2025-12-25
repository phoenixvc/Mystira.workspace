# Entity and Repository Development Guide

This guide covers the complete workflow for adding new entities, repositories, and data access patterns in Mystira services.

## Table of Contents

- [Quick Start: Adding a New Entity](#quick-start-adding-a-new-entity)
- [Architecture Overview](#architecture-overview)
- [Approach Comparison](#approach-comparison)
- [Detailed Workflows](#detailed-workflows)
- [Reusable Patterns](#reusable-patterns)
- [Best Practices](#best-practices)

---

## Quick Start: Adding a New Entity

### Step 1: Define the Entity

```csharp
// Domain/Entities/Product.cs
using Mystira.Shared.Data.Entities;

public class Product : Entity<Guid>  // or AuditableEntity<Guid> for timestamps
{
    public required string Name { get; set; }
    public required string Sku { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public Guid CategoryId { get; set; }

    // Navigation properties
    public Category? Category { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
```

### Step 2: Choose Your Repository Approach

| Approach | When to Use | Effort |
|----------|-------------|--------|
| **Generic Repository** | Simple CRUD, no custom queries | Lowest |
| **Inherited Repository** | Custom queries needed | Low |
| **Source Generated** | Many repositories with similar patterns | Medium |
| **Full Custom** | Complex domain logic, multiple data sources | Higher |

### Step 3: Implement (3 Options)

#### Option A: Generic Repository (Zero Code)

```csharp
// Just register the generic repository
services.AddScoped<IRepository<Product>, RepositoryBase<Product>>();
services.AddScoped<IRepository<Product, Guid>, RepositoryBase<Product, Guid>>();

// Use directly
public class ProductService
{
    private readonly IRepository<Product> _products;

    public async Task<Product?> GetAsync(Guid id)
        => await _products.GetByIdAsync(id);
}
```

#### Option B: Inherited Repository (Recommended)

```csharp
// Interfaces/IProductRepository.cs
public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);
    Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default);
    Task<IEnumerable<Product>> GetLowStockAsync(int threshold, CancellationToken ct = default);
}

// Repositories/ProductRepository.cs
public class ProductRepository : RepositoryBase<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(p => p.Sku == sku, ct);

    public async Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default)
        => await DbSet.Where(p => p.CategoryId == categoryId).ToListAsync(ct);

    public async Task<IEnumerable<Product>> GetLowStockAsync(int threshold, CancellationToken ct = default)
        => await DbSet.Where(p => p.StockQuantity < threshold).ToListAsync(ct);
}

// Register
services.AddScoped<IProductRepository, ProductRepository>();
```

#### Option C: Source Generated (For Scale)

```csharp
// Only the interface - implementation is auto-generated
[GenerateRepository]
public interface IProductRepository : IRepository<Product>
{
    // Custom methods need partial class implementation
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);
}

// Partial class for custom implementations
public partial class ProductRepositoryGenerated
{
    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(p => p.Sku == sku, ct);
}
```

### Step 4: Add DbContext Configuration

```csharp
// Data/AppDbContext.cs
public class AppDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Sku).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Sku).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Price).HasPrecision(18, 2);

            entity.HasOne(e => e.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(e => e.CategoryId);
        });
    }
}
```

### Step 5: Create Migration (if using EF Core migrations)

```bash
dotnet ef migrations add AddProductTable -p src/Infrastructure -s src/Api
dotnet ef database update -p src/Infrastructure -s src/Api
```

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                         API / Application                        │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  Services, Handlers, Controllers                          │  │
│  │  - Inject IProductRepository                              │  │
│  │  - Use Specifications for complex queries                 │  │
│  └─────────────────────────┬─────────────────────────────────┘  │
│                            │                                     │
│                            ▼                                     │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  Repository Interfaces (Ports)                            │  │
│  │  - IRepository<T>      (Generic CRUD)                     │  │
│  │  - IProductRepository  (Domain-specific)                  │  │
│  │  - IUnitOfWork         (Transaction management)           │  │
│  └─────────────────────────┬─────────────────────────────────┘  │
│                            │                                     │
├────────────────────────────┼─────────────────────────────────────┤
│                            ▼                                     │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  Repository Implementations (Adapters)                    │  │
│  │                                                           │  │
│  │  Mystira.Shared (Base Classes)                           │  │
│  │  ├── RepositoryBase<T>          (Generic EF Core impl)   │  │
│  │  ├── RepositoryBase<T, TKey>    (Typed key support)      │  │
│  │  └── UnitOfWork                  (Transaction wrapper)   │  │
│  │                                                           │  │
│  │  Your Project (Concrete Implementations)                  │  │
│  │  ├── ProductRepository : RepositoryBase<Product>         │  │
│  │  ├── OrderRepository   : RepositoryBase<Order>           │  │
│  │  └── [Generated]       : RepositoryBase<T>               │  │
│  └─────────────────────────┬─────────────────────────────────┘  │
│                            │                                     │
│                            ▼                                     │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  DbContext                                                │  │
│  │  - Entity configurations                                  │  │
│  │  - DbSets                                                 │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

---

## Approach Comparison

### Source Generators vs Alternatives

| Approach | Pros | Cons | Best For |
|----------|------|------|----------|
| **Our Source Generators** | Compile-time, type-safe, no runtime reflection | Learning curve, debugging difficult | Teams comfortable with Roslyn |
| **Generic Repository Only** | Zero boilerplate, simple | No custom queries, limited flexibility | Simple CRUD apps |
| **Scaffolding CLI** | User-friendly, visual output | Not type-safe, one-time generation | Initial project setup |
| **Scrutor Convention Registration** | Auto-discovery, flexible | Runtime assembly scanning | Large projects with many repositories |
| **EF Core Power Tools** | Visual, database-first | Tied to EF Core, regeneration issues | Database-first projects |

### Modern Alternatives to Consider

#### 1. Convention-Based Registration (Scrutor)

```csharp
// Auto-register all repositories by convention
services.Scan(scan => scan
    .FromAssemblyOf<ProductRepository>()
    .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
    .AsImplementedInterfaces()
    .WithScopedLifetime());
```

#### 2. .NET 8 Interceptors (Compile-time AOP)

```csharp
// Intercept repository methods at compile time
[InterceptsLocation("ProductRepository.cs", line: 15, column: 5)]
public static async Task<Product?> InterceptGetById(Guid id, CancellationToken ct)
{
    // Add caching, logging, etc.
}
```

#### 3. Generic Repository with Specifications (Recommended for Simplicity)

```csharp
// No custom repository needed - use specifications
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

### Our Recommendation

**For most cases, use Generic Repository + Specifications:**

```csharp
// Register once
services.AddScoped(typeof(IRepository<>), typeof(RepositoryBase<>));

// Use specifications for all queries
public class ProductsByCategory : Specification<Product>
{
    public ProductsByCategory(Guid categoryId) =>
        Query.Where(p => p.CategoryId == categoryId);
}

// Clean, testable, no boilerplate
var products = await _repo.ListAsync(new ProductsByCategory(categoryId));
```

**Use Source Generators when:**
- You have 20+ similar repositories
- Custom methods can't be expressed as specifications
- You want compile-time validation of repository contracts

---

## Detailed Workflows

### Adding a Complete Feature

```
1. Entity Definition
   └── Domain/Entities/Product.cs

2. Repository Interface (optional for custom queries)
   └── Application/Interfaces/IProductRepository.cs

3. Repository Implementation
   └── Infrastructure/Repositories/ProductRepository.cs

4. DbContext Configuration
   └── Infrastructure/Data/AppDbContext.cs (add DbSet + configuration)

5. Migration
   └── dotnet ef migrations add AddProduct

6. Service Registration
   └── Infrastructure/DependencyInjection.cs

7. Specifications (for complex queries)
   └── Application/Specifications/Products/

8. Tests
   └── Tests/Repositories/ProductRepositoryTests.cs
```

### Entity Inheritance Patterns

```csharp
// Base entities from Mystira.Shared
public abstract class Entity<TId>
{
    public TId Id { get; set; } = default!;
}

public abstract class AuditableEntity<TId> : Entity<TId>
{
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public abstract class SoftDeletableEntity<TId> : AuditableEntity<TId>
{
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

// Your entities
public class Order : AuditableEntity<Guid>
{
    public required Guid CustomerId { get; set; }
    public OrderStatus Status { get; set; }
    // ...
}
```

---

## Reusable Patterns

### 1. Specification Pattern

```csharp
// Create reusable query specifications
public class ActiveProductsSpec : Specification<Product>
{
    public ActiveProductsSpec() =>
        Query.Where(p => !p.IsDeleted && p.StockQuantity > 0);
}

public class ProductsWithCategorySpec : Specification<Product>
{
    public ProductsWithCategorySpec() =>
        Query.Include(p => p.Category);
}

// Combine specifications
public class ActiveProductsWithCategorySpec : Specification<Product>
{
    public ActiveProductsWithCategorySpec()
    {
        Query.Where(p => !p.IsDeleted && p.StockQuantity > 0)
             .Include(p => p.Category);
    }
}
```

### 2. Decorator Pattern (Caching, Logging)

```csharp
// Decorator for caching
public class CachedProductRepository : IProductRepository
{
    private readonly IProductRepository _inner;
    private readonly ICacheService _cache;

    public CachedProductRepository(IProductRepository inner, ICacheService cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _cache.GetOrCreateAsync(
            $"product:{id}",
            async () => await _inner.GetByIdAsync(id, ct));
    }

    // Delegate other methods to _inner
}

// Register with Scrutor
services.Decorate<IProductRepository, CachedProductRepository>();
```

### 3. Unit of Work Pattern

```csharp
public class OrderService
{
    private readonly IRepository<Order> _orders;
    private readonly IRepository<OrderItem> _items;
    private readonly IRepository<Product> _products;
    private readonly IUnitOfWork _unitOfWork;

    public async Task CreateOrderAsync(CreateOrderDto dto, CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var order = new Order { CustomerId = dto.CustomerId };
            await _orders.AddAsync(order, ct);

            foreach (var item in dto.Items)
            {
                // Decrease stock
                var product = await _products.GetByIdAsync(item.ProductId, ct);
                product!.StockQuantity -= item.Quantity;
                await _products.UpdateAsync(product, ct);

                // Add order item
                await _items.AddAsync(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                }, ct);
            }

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
```

### 4. Streaming for Large Datasets

```csharp
public class ReportService
{
    private readonly IRepository<Order> _orders;

    public async IAsyncEnumerable<OrderSummary> StreamOrderReportAsync(
        DateOnly startDate,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var order in _orders.StreamAsync(
            new OrdersAfterDateSpec(startDate), ct))
        {
            yield return new OrderSummary
            {
                OrderId = order.Id,
                Total = order.Total,
                Date = order.CreatedAt
            };
        }
    }
}
```

---

## Best Practices

### DO

- Use generic `IRepository<T>` for simple CRUD
- Use specifications for complex/reusable queries
- Inherit from `AuditableEntity` for timestamp tracking
- Use `IAsyncEnumerable` for large result sets
- Register repositories as Scoped
- Use transactions via `IUnitOfWork` for multi-entity operations

### DON'T

- Create custom repositories just for single-use queries
- Expose `DbSet` or `DbContext` outside infrastructure layer
- Use repository pattern for simple projections (use CQRS/queries instead)
- Mix different DbContext instances in a transaction
- Create "god" repositories with dozens of methods

### Testing

```csharp
// Use InMemory provider for unit tests
public class ProductRepositoryTests
{
    private readonly DbContextOptions<AppDbContext> _options;

    public ProductRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetBySkuAsync_ReturnsProduct_WhenExists()
    {
        // Arrange
        await using var context = new AppDbContext(_options);
        var repo = new ProductRepository(context);
        var product = new Product { Name = "Test", Sku = "TEST-001", Price = 10m };
        await repo.AddAsync(product);
        await context.SaveChangesAsync();

        // Act
        var result = await repo.GetBySkuAsync("TEST-001");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
    }
}
```

---

## Related Documentation

- [Repository Architecture](../architecture/migrations/repository-architecture.md) - Dual-database strategy
- [Ardalis Specification Migration](../architecture/specifications/ardalis-specification-migration.md)
- [Mystira.Shared Migration Guide](./mystira-shared-migration.md)
