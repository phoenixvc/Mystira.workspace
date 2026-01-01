using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Security.Claims;

namespace Mystira.Infrastructure.Data;

/// <summary>
/// EF Core interceptor that sets the CurrentUserId on MystiraAppDbContext
/// before SaveChanges is called, enabling audit field population.
/// </summary>
/// <remarks>
/// This interceptor reads the user ID from the current HTTP context's claims.
/// It should be registered as a scoped service and added to the DbContext via DI.
///
/// Usage in Program.cs:
/// <code>
/// services.AddHttpContextAccessor();
/// services.AddScoped&lt;AuditInterceptor&gt;();
/// services.AddDbContext&lt;MystiraAppDbContext&gt;((sp, options) =>
/// {
///     options.AddInterceptors(sp.GetRequiredService&lt;AuditInterceptor&gt;());
///     // ... other configuration
/// });
/// </code>
/// </remarks>
public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditInterceptor"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor for reading user claims.</param>
    public AuditInterceptor(IHttpContextAccessor? httpContextAccessor = null)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        SetCurrentUserId(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        SetCurrentUserId(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void SetCurrentUserId(DbContext? context)
    {
        if (context is not MystiraAppDbContext dbContext)
        {
            return;
        }

        // Only set if not already set (allows manual override)
        if (dbContext.CurrentUserId != null)
        {
            return;
        }

        var userId = GetCurrentUserIdFromHttpContext();
        dbContext.CurrentUserId = userId;
    }

    private string? GetCurrentUserIdFromHttpContext()
    {
        var httpContext = _httpContextAccessor?.HttpContext;
        if (httpContext == null)
        {
            return null;
        }

        var user = httpContext.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        // Try common claim types for user ID
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value  // JWT subject claim
            ?? user.FindFirst("oid")?.Value  // Azure AD object ID
            ?? user.FindFirst("user_id")?.Value;

        return userId;
    }
}
