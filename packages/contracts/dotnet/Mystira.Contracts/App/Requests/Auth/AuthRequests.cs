namespace Mystira.Contracts.App.Requests.Auth;

public record PasswordlessSignupRequest
{
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
}

public record PasswordlessVerifyRequest
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
