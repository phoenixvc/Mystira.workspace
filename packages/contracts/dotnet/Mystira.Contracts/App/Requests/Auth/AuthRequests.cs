namespace Mystira.Contracts.App.Requests.Auth;

/// <summary>
/// Request to initiate passwordless signup or login.
/// </summary>
public record PasswordlessSignupRequest
{
    /// <summary>
    /// The email address to send the verification code to.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Optional display name for new accounts.
    /// </summary>
    public string? DisplayName { get; set; }
}

/// <summary>
/// Request to verify a passwordless authentication code.
/// </summary>
public record PasswordlessVerifyRequest
{
    /// <summary>
    /// The email address that received the verification code.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The verification code sent to the email.
    /// </summary>
    public string Code { get; set; } = string.Empty;
}

/// <summary>
/// Request to refresh an authentication token.
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// The refresh token to exchange for a new access token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Request to sign in with passwordless authentication.
/// </summary>
public record PasswordlessSigninRequest
{
    /// <summary>
    /// The email address for signin.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Optional device identifier for multi-device tracking.
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// Optional platform identifier (e.g., "ios", "android", "web").
    /// </summary>
    public string? Platform { get; set; }
}
