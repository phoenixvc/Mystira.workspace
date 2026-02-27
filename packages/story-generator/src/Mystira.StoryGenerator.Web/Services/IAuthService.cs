namespace Mystira.StoryGenerator.Web.Services;

public interface IAuthService
{
    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetTokenAsync();
    Task LoginAsync(string token);
    Task LogoutAsync();
    Task<bool> EnsureTokenValidAsync();
    
    event EventHandler<bool>? AuthenticationStateChanged;
}

public record AuthResult(
    bool IsSuccess,
    string? Token = null,
    string? ErrorMessage = null
);
