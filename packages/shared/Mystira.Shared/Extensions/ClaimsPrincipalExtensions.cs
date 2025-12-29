using System.Security.Claims;
using Mystira.Shared.Exceptions;

namespace Mystira.Shared.Extensions;

/// <summary>
/// Extension methods for <see cref="ClaimsPrincipal"/>.
/// Provides standard claims extraction patterns for any API consuming JWTs.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Standard claim types used for account identification across different identity providers.
    /// </summary>
    private static readonly string[] AccountIdClaimTypes =
    [
        "account_id",                   // Custom Mystira claim
        "accountId",                    // Alternative casing
        ClaimTypes.NameIdentifier,      // Standard .NET claim type
        "sub",                          // Standard JWT subject claim
        "oid",                          // Microsoft Entra ID object ID
        "http://schemas.microsoft.com/identity/claims/objectidentifier" // Full Entra ID claim URI
    ];

    /// <summary>
    /// Gets the account ID from the claims principal.
    /// Searches through common claim types used by different identity providers.
    /// </summary>
    /// <param name="principal">The claims principal to extract the account ID from.</param>
    /// <returns>The account ID if found; otherwise, null.</returns>
    /// <remarks>
    /// This method searches for the account ID in the following claim types (in order):
    /// <list type="bullet">
    ///   <item><description>account_id - Custom Mystira claim</description></item>
    ///   <item><description>accountId - Alternative casing</description></item>
    ///   <item><description>nameidentifier - Standard .NET claim</description></item>
    ///   <item><description>sub - Standard JWT subject claim</description></item>
    ///   <item><description>oid - Microsoft Entra ID object ID</description></item>
    /// </list>
    /// </remarks>
    public static string? GetAccountId(this ClaimsPrincipal? principal)
    {
        if (principal is null)
        {
            return null;
        }

        foreach (var claimType in AccountIdClaimTypes)
        {
            var value = principal.FindFirst(claimType)?.Value;
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the required account ID from the claims principal.
    /// Throws an exception if the account ID is not found.
    /// </summary>
    /// <param name="principal">The claims principal to extract the account ID from.</param>
    /// <returns>The account ID.</returns>
    /// <exception cref="UnauthorizedException">Thrown when no account ID claim is found.</exception>
    /// <remarks>
    /// Use this method when the account ID is required for the operation.
    /// For optional account ID scenarios, use <see cref="GetAccountId"/> instead.
    /// </remarks>
    public static string GetRequiredAccountId(this ClaimsPrincipal? principal)
    {
        var accountId = principal.GetAccountId();

        if (string.IsNullOrWhiteSpace(accountId))
        {
            throw new UnauthorizedException("No account ID claim found in the authentication token.");
        }

        return accountId;
    }

    /// <summary>
    /// Gets the user ID from the claims principal.
    /// This is an alias for <see cref="GetAccountId"/> for semantic clarity.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>The user ID if found; otherwise, null.</returns>
    public static string? GetUserId(this ClaimsPrincipal? principal)
    {
        return principal.GetAccountId();
    }

    /// <summary>
    /// Gets the required user ID from the claims principal.
    /// This is an alias for <see cref="GetRequiredAccountId"/> for semantic clarity.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>The user ID.</returns>
    /// <exception cref="UnauthorizedException">Thrown when no user ID claim is found.</exception>
    public static string GetRequiredUserId(this ClaimsPrincipal? principal)
    {
        return principal.GetRequiredAccountId();
    }

    /// <summary>
    /// Gets the email address from the claims principal.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>The email address if found; otherwise, null.</returns>
    public static string? GetEmail(this ClaimsPrincipal? principal)
    {
        if (principal is null)
        {
            return null;
        }

        return principal.FindFirst(ClaimTypes.Email)?.Value
            ?? principal.FindFirst("email")?.Value
            ?? principal.FindFirst("preferred_username")?.Value; // Entra ID uses this for email
    }

    /// <summary>
    /// Gets the display name from the claims principal.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>The display name if found; otherwise, null.</returns>
    public static string? GetDisplayName(this ClaimsPrincipal? principal)
    {
        if (principal is null)
        {
            return null;
        }

        return principal.FindFirst(ClaimTypes.Name)?.Value
            ?? principal.FindFirst("name")?.Value
            ?? principal.FindFirst(ClaimTypes.GivenName)?.Value;
    }

    /// <summary>
    /// Gets all roles assigned to the claims principal.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>A collection of role names.</returns>
    public static IEnumerable<string> GetRoles(this ClaimsPrincipal? principal)
    {
        if (principal is null)
        {
            return [];
        }

        return principal.FindAll(ClaimTypes.Role)
            .Concat(principal.FindAll("roles"))     // Entra ID app roles
            .Concat(principal.FindAll("role"))      // Some providers use singular
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the claims principal has the specified role.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <param name="role">The role to check for.</param>
    /// <returns>True if the principal has the role; otherwise, false.</returns>
    public static bool HasRole(this ClaimsPrincipal? principal, string role)
    {
        if (principal is null || string.IsNullOrWhiteSpace(role))
        {
            return false;
        }

        return principal.GetRoles().Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets a specific claim value by type.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <param name="claimType">The claim type to retrieve.</param>
    /// <returns>The claim value if found; otherwise, null.</returns>
    public static string? GetClaimValue(this ClaimsPrincipal? principal, string claimType)
    {
        if (principal is null || string.IsNullOrWhiteSpace(claimType))
        {
            return null;
        }

        return principal.FindFirst(claimType)?.Value;
    }

    /// <summary>
    /// Gets all values for a specific claim type.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <param name="claimType">The claim type to retrieve.</param>
    /// <returns>A collection of claim values.</returns>
    public static IEnumerable<string> GetClaimValues(this ClaimsPrincipal? principal, string claimType)
    {
        if (principal is null || string.IsNullOrWhiteSpace(claimType))
        {
            return [];
        }

        return principal.FindAll(claimType)
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v));
    }
}
