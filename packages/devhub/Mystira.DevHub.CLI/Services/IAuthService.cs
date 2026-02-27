using System.Security.Claims;

namespace Mystira.DevHub.CLI.Services;

/// <summary>
/// Provides authentication services for the DevHub CLI.
/// Handles JWT token validation, principal extraction, and role-based authorization.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Validates a JWT token using configured validation parameters.
    /// </summary>
    /// <param name="token">The JWT token to validate.</param>
    /// <returns>True if the token is valid; false otherwise.</returns>
    Task<bool> ValidateTokenAsync(string token);

    /// <summary>
    /// Extracts the claims principal from a valid JWT token.
    /// </summary>
    /// <param name="token">The JWT token to extract claims from.</param>
    /// <returns>The claims principal if valid; null otherwise.</returns>
    Task<ClaimsPrincipal?> GetPrincipalAsync(string token);

    /// <summary>
    /// Checks if a token is valid and has the required role.
    /// </summary>
    /// <param name="token">The JWT token to authorize.</param>
    /// <param name="requiredRole">The role required for authorization.</param>
    /// <returns>True if authorized; false otherwise.</returns>
    Task<bool> IsAuthorizedAsync(string token, string requiredRole = "admin");
}

/// <summary>
/// Represents the result of a JWT token validation operation.
/// </summary>
/// <param name="IsValid">True if the token is valid; false otherwise.</param>
/// <param name="Principal">The claims principal extracted from the token, if valid.</param>
/// <param name="ErrorMessage">Error message if validation failed.</param>
public record AuthResult(
    bool IsValid,
    ClaimsPrincipal? Principal = null,
    string? ErrorMessage = null
);
