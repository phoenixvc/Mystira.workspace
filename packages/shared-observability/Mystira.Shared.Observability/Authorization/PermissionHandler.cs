using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Mystira.Shared.Authorization;

/// <summary>
/// Authorization requirement for a specific permission.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionRequirement"/> class.
    /// </summary>
    /// <param name="permission">The required permission string.</param>
    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }

    /// <summary>
    /// Gets the required permission.
    /// </summary>
    public string Permission { get; }
}

/// <summary>
/// Handles permission-based authorization by checking user claims.
/// </summary>
public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ILogger<PermissionHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionHandler"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public PermissionHandler(ILogger<PermissionHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var user = context.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogDebug("User is not authenticated, permission {Permission} denied", requirement.Permission);
            return Task.CompletedTask;
        }

        // Check if user has the required permission in their claims
        if (HasPermission(user, requirement.Permission))
        {
            _logger.LogDebug("User has permission {Permission}", requirement.Permission);
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Check if user has admin role (admins have all permissions)
        if (IsAdmin(user))
        {
            _logger.LogDebug("User is admin, granting permission {Permission}", requirement.Permission);
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        _logger.LogDebug("User lacks permission {Permission}", requirement.Permission);
        return Task.CompletedTask;
    }

    private static bool HasPermission(ClaimsPrincipal user, string permission)
    {
        // Check standard permission claims
        var hasPermissionClaim = user.HasClaim(c =>
            (c.Type == "permissions" || c.Type == "scp" || c.Type == "scope") &&
            c.Value.Split(' ').Contains(permission));

        if (hasPermissionClaim)
            return true;

        // Check Entra ID app roles
        var hasRoleClaim = user.HasClaim(c =>
            c.Type == "roles" &&
            c.Value == permission);

        return hasRoleClaim;
    }

    private static bool IsAdmin(ClaimsPrincipal user)
    {
        return user.IsInRole(Roles.Admin) ||
               user.HasClaim(c => c.Type == "roles" && c.Value == Roles.Admin);
    }
}
