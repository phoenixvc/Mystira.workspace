using Mystira.App.PWA.Models;

namespace Mystira.App.PWA.Services;

/// <summary>
/// Provides authentication services for dual-path authentication (Entra ID + Magic Link).
/// Supports both Microsoft Entra ID SSO and email-based magic link authentication.
/// </summary>
public interface IAuthService
{
    Task<bool> IsAuthenticatedAsync();
    Task<Account?> GetCurrentAccountAsync();
    Task<string?> GetTokenAsync();
    Task LogoutAsync();
    Task<string?> GetCurrentTokenAsync();
    void SetRememberMe(bool rememberMe);
    Task<bool> GetRememberMeAsync();

    /// <summary>
    /// Gets the current UTC expiry time of the access token.
    /// </summary>
    /// <returns>The token expiry time in UTC, or null if no token is available.</returns>
    DateTime? GetTokenExpiryTime();

    /// <summary>
    /// Ensures the current token is valid, refreshing if necessary.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>True if the token is valid or was successfully refreshed; false otherwise.</returns>
    Task<bool> EnsureTokenValidAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Event raised when token is about to expire (within 5 minutes).
    /// </summary>
    event EventHandler? TokenExpiryWarning;

    /// <summary>
    /// Sets the authenticated session with the provided access token and account.
    /// </summary>
    /// <param name="accessToken">The access token for the session.</param>
    /// <param name="account">The account information for the session.</param>
    Task SetAuthenticatedSessionAsync(string accessToken, Account account);
    Task SetCurrentAccountAsync(Account account);

    event EventHandler<bool>? AuthenticationStateChanged;

    // Dual-path authentication methods
    /// <summary>
    /// Initiates sign-in with Microsoft Entra ID using OAuth 2.0 flow.
    /// </summary>
    /// <returns>An AuthResult indicating success or failure with relevant details.</returns>
    /// <exception cref="InvalidOperationException">Thrown when Entra configuration is invalid.</exception>
    /// <exception cref="System.Net.Http.HttpRequestException">Thrown when network requests fail.</exception>
    Task<AuthResult> SignInWithEntraAsync();
    /// <summary>
    /// Initiates sign-in using a magic link sent to the provided email.
    /// </summary>
    /// <param name="email">The email address to send the magic link to.</param>
    /// <param name="displayName">Optional display name for the user account.</param>
    /// <returns>An AuthResult indicating success or failure with relevant details.</returns>
    /// <exception cref="ArgumentException">Thrown when email is invalid or empty.</exception>
    /// <exception cref="System.Net.Http.HttpRequestException">Thrown when email service fails.</exception>
    Task<AuthResult> SignInWithMagicLinkAsync(string email, string? displayName = null);
    /// <summary>
    /// Completes the magic link sign-in process using the token from the email link.
    /// </summary>
    /// <param name="token">The magic link token from the email URL.</param>
    /// <returns>An AuthResult indicating success or failure with authentication details.</returns>
    /// <exception cref="ArgumentException">Thrown when token is invalid or expired.</exception>
    /// <exception cref="System.Net.Http.HttpRequestException">Thrown when token validation fails.</exception>
    Task<AuthResult> CompleteMagicLinkSignInAsync(string token);
    /// <summary>
    /// Requests a magic link to be sent to the specified email address.
    /// </summary>
    /// <param name="email">The email address to send the magic link to.</param>
    /// <param name="displayName">Optional display name for account creation.</param>
    /// <returns>A MagicSignupResult indicating if the email was sent successfully.</returns>
    /// <exception cref="ArgumentException">Thrown when email format is invalid.</exception>
    /// <exception cref="System.Net.Http.HttpRequestException">Thrown when email service fails.</exception>
    Task<MagicSignupResult?> RequestMagicLinkAsync(string email, string? displayName = null);
    /// <summary>
    /// Resends a magic link to an existing email address.
    /// </summary>
    /// <param name="email">The email address to resend the magic link to.</param>
    /// <returns>A MagicSignupResult indicating if the resend was successful.</returns>
    /// <exception cref="ArgumentException">Thrown when email is not found or invalid.</exception>
    /// <exception cref="System.Net.Http.HttpRequestException">Thrown when email service fails.</exception>
    Task<MagicSignupResult?> ResendMagicLinkAsync(string email);
    /// <summary>
    /// Verifies a magic link token without completing sign-in.
    /// </summary>
    /// <param name="token">The magic link token to verify.</param>
    /// <returns>A VerifyMagicSignupResult with token validity and user details if valid.</returns>
    /// <exception cref="ArgumentException">Thrown when token format is invalid.</exception>
    /// <exception cref="System.Net.Http.HttpRequestException">Thrown when token verification fails.</exception>
    Task<VerifyMagicSignupResult?> VerifyMagicLinkAsync(string token);
    /// <summary>
    /// Refreshes the current authentication token if needed.
    /// </summary>
    /// <returns>An AuthResult with the refreshed token or failure details.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown when no active session exists.</exception>
    /// <exception cref="System.Net.Http.HttpRequestException">Thrown when token refresh fails.</exception>
    Task<AuthResult> RefreshAuthAsync();
}

/// <summary>
/// Represents the result of an authentication operation with dual-path support.
/// Contains success status, account information, tokens, and error details.
/// </summary>
/// <param name="IsSuccess">True if the operation succeeded; false otherwise.</param>
/// <param name="Account">The authenticated account, if successful.</param>
/// <param name="ErrorMessage">Error message describing the failure, if any.</param>
/// <param name="AccessToken">JWT access token for authenticated sessions.</param>
/// <param name="ExpiresAt">Token expiration time in UTC.</param>
/// <param name="AuthProvider">The authentication provider used ("entra" or "magic").</param>
public record AuthResult(
    bool IsSuccess,
    Account? Account = null,
    string? ErrorMessage = null,
    string? AccessToken = null,
    DateTime? ExpiresAt = null,
    string? AuthProvider = null
);

/// <summary>
/// Represents the result of a magic link request operation.
/// Used for email delivery operations without completing authentication.
/// </summary>
/// <param name="IsSuccess">True if the magic link was sent successfully; false otherwise.</param>
/// <param name="ErrorMessage">Error message if the request failed.</param>
/// <param name="Email">The email address the link was sent to.</param>
public record MagicSignupResult(
    bool IsSuccess,
    string? ErrorMessage = null,
    string? Email = null
);

/// <summary>
/// Represents the result of a magic link token verification operation.
/// Contains user details and token information for completing sign-in.
/// </summary>
/// <param name="IsSuccess">True if the token is valid; false otherwise.</param>
/// <param name="Account">The associated account if token is valid.</param>
/// <param name="ErrorMessage">Error message if verification failed.</param>
/// <param name="AccessToken">JWT access token if verification succeeded.</param>
/// <param name="ExpiresAt">Token expiration time in UTC.</param>
public record VerifyMagicSignupResult(
    string Status,
    string Message,
    bool CanContinueWithEmail,
    bool CanContinueWithEntra,
    bool IsSuccess = false,
    Account? Account = null,
    string? AccessToken = null,
    DateTime? ExpiresAt = null
);
