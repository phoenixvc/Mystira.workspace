namespace Mystira.Contracts.App.Responses.Auth;

/// <summary>
/// Response from passwordless signup initiation.
/// </summary>
public record PasswordlessSignupResponse
{
    /// <summary>
    /// Whether the signup initiation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message describing the result.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// The email address the verification was sent to.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Expiration time of the verification code.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Number of seconds until retry is allowed.
    /// </summary>
    public int? RetryAfterSeconds { get; set; }
}

/// <summary>
/// Response from passwordless verification.
/// </summary>
public record PasswordlessVerifyResponse
{
    /// <summary>
    /// Whether the verification was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The access token for authenticated requests.
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// The refresh token for obtaining new access tokens.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Token type (typically "Bearer").
    /// </summary>
    public string? TokenType { get; set; }

    /// <summary>
    /// Expiration time of the access token in seconds.
    /// </summary>
    public int? ExpiresIn { get; set; }

    /// <summary>
    /// The account identifier for the authenticated user.
    /// </summary>
    public string? AccountId { get; set; }

    /// <summary>
    /// Whether this is a new account (first-time signup).
    /// </summary>
    public bool IsNewAccount { get; set; }

    /// <summary>
    /// Error message if verification failed.
    /// </summary>
    public string? Error { get; set; }
}

