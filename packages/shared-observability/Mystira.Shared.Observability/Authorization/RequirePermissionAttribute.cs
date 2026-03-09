using Microsoft.AspNetCore.Authorization;

namespace Mystira.Shared.Authorization;

/// <summary>
/// Authorization attribute that requires a specific permission.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Policy prefix for permission-based policies.
    /// </summary>
    public const string PolicyPrefix = "Permission:";

    /// <summary>
    /// Creates a new RequirePermissionAttribute for the specified permission.
    /// </summary>
    /// <param name="permission">The permission required (e.g., "scenarios.read")</param>
    public RequirePermissionAttribute(string permission)
        : base($"{PolicyPrefix}{permission}")
    {
        Permission = permission;
    }

    /// <summary>
    /// The permission required by this attribute.
    /// </summary>
    public string Permission { get; }
}

/// <summary>
/// Authorization attribute that requires a specific role.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireRoleAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Creates a new RequireRoleAttribute for the specified role.
    /// </summary>
    /// <param name="role">The role required (e.g., "Admin")</param>
    public RequireRoleAttribute(string role)
        : base()
    {
        Roles = role;
        Role = role;
    }

    /// <summary>
    /// The role required by this attribute.
    /// </summary>
    public string Role { get; }
}
