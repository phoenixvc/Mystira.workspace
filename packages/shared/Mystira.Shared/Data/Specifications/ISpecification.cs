using System.Linq.Expressions;

namespace Mystira.Shared.Data.Specifications;

/// <summary>
/// Specification pattern for encapsulating query logic.
///
/// DEPRECATED: Use Ardalis.Specification.ISpecification instead.
/// This interface is kept for backward compatibility only.
/// New code should use Ardalis.Specification.Specification{T} as the base class.
///
/// Example using Ardalis.Specification:
/// <code>
/// public class ActiveUsersSpec : Specification{User}
/// {
///     public ActiveUsersSpec()
///     {
///         Query.Where(u => u.IsActive)
///              .OrderBy(u => u.Name);
///     }
/// }
/// </code>
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
[Obsolete("Use Ardalis.Specification.ISpecification<T> instead. This interface will be removed in a future version.")]
public interface ISpecification<T>
{
    /// <summary>
    /// Criteria expression for filtering entities (WHERE clause).
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Include expressions for eager loading related entities.
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// Include strings for eager loading related entities using ThenInclude.
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// Order by expression for ascending sort.
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// Order by expression for descending sort.
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>
    /// Group by expression.
    /// </summary>
    Expression<Func<T, object>>? GroupBy { get; }

    /// <summary>
    /// Number of records to skip (for pagination).
    /// </summary>
    int Skip { get; }

    /// <summary>
    /// Number of records to take (for pagination).
    /// </summary>
    int Take { get; }

    /// <summary>
    /// Enable pagination.
    /// </summary>
    bool IsPagingEnabled { get; }
}

/// <summary>
/// Base implementation of the Specification pattern.
///
/// DEPRECATED: Use Ardalis.Specification.Specification{T} instead.
/// This class is kept for backward compatibility only.
///
/// Example migration to Ardalis.Specification:
/// <code>
/// // Old way (deprecated):
/// public class MySpec : BaseSpecification{Entity}
/// {
///     public MySpec() { ApplyCriteria(e => e.IsActive); }
/// }
///
/// // New way (recommended):
/// public class MySpec : Specification{Entity}
/// {
///     public MySpec() { Query.Where(e => e.IsActive); }
/// }
/// </code>
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
[Obsolete("Use Ardalis.Specification.Specification<T> instead. This class will be removed in a future version.")]
public abstract class BaseSpecification<T> : ISpecification<T>
{
    protected BaseSpecification()
    {
    }

    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    public Expression<Func<T, bool>>? Criteria { get; private set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; private set; }
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }
    public Expression<Func<T, object>>? GroupBy { get; private set; }
    public int Skip { get; private set; }
    public int Take { get; private set; }
    public bool IsPagingEnabled { get; private set; }

    /// <summary>
    /// Set the criteria expression.
    /// </summary>
    protected virtual void ApplyCriteria(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    /// <summary>
    /// Add an include expression for eager loading.
    /// </summary>
    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    /// <summary>
    /// Add an include string for eager loading with ThenInclude.
    /// </summary>
    protected virtual void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    /// <summary>
    /// Apply ascending ordering.
    /// </summary>
    protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    /// <summary>
    /// Apply descending ordering.
    /// </summary>
    protected virtual void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
    {
        OrderByDescending = orderByDescExpression;
    }

    /// <summary>
    /// Apply grouping.
    /// </summary>
    protected virtual void ApplyGroupBy(Expression<Func<T, object>> groupByExpression)
    {
        GroupBy = groupByExpression;
    }

    /// <summary>
    /// Apply pagination.
    /// </summary>
    protected virtual void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
}
