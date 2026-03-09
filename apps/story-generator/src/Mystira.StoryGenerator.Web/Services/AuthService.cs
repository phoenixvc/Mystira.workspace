using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;

namespace Mystira.StoryGenerator.Web.Services;

public class AuthService : IAuthService
{
    private readonly ILocalStorageService _localStorage;
    private readonly NavigationManager _navigationManager;
    private readonly ILogger<AuthService> _logger;
    private readonly HttpClient _httpClient;

    private const string TokenStorageKey = "storygen_auth_token";
    private const string UserStorageKey = "storygen_user_info";

    private string? _currentToken;
    private bool _isAuthenticated;

    public event EventHandler<bool>? AuthenticationStateChanged;

    public AuthService(
        ILocalStorageService localStorage,
        NavigationManager navigationManager,
        ILogger<AuthService> logger,
        HttpClient httpClient)
    {
        _localStorage = localStorage;
        _navigationManager = navigationManager;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        if (_isAuthenticated && !string.IsNullOrWhiteSpace(_currentToken))
        {
            return true;
        }

        await LoadStoredAuthDataAsync();
        return _isAuthenticated;
    }

    public async Task<string?> GetTokenAsync()
    {
        if (!string.IsNullOrWhiteSpace(_currentToken))
        {
            return _currentToken;
        }

        await LoadStoredAuthDataAsync();
        return _currentToken;
    }

    public async Task LoginAsync(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Token is required", nameof(token));
            }

            // Validate token format
            var jwtHandler = new JwtSecurityTokenHandler();
            if (!jwtHandler.CanReadToken(token))
            {
                throw new ArgumentException("Invalid token format", nameof(token));
            }

            var jwtToken = jwtHandler.ReadJwtToken(token);

            // Check if token is expired
            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                throw new ArgumentException("Token has expired", nameof(token));
            }

            // Store token
            _currentToken = token;
            _isAuthenticated = true;
            await _localStorage.SetItemAsStringAsync(TokenStorageKey, token);

            // Store user info
            var userInfo = new
            {
                Name = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "Unknown",
                Email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "",
                ExpiresAt = jwtToken.ValidTo
            };

            await _localStorage.SetItemAsStringAsync(UserStorageKey, JsonSerializer.Serialize(userInfo));

            _logger.LogInformation("User successfully authenticated");
            AuthenticationStateChanged?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            await LogoutAsync();
            throw;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            _currentToken = null;
            _isAuthenticated = false;

            await _localStorage.RemoveItemAsync(TokenStorageKey);
            await _localStorage.RemoveItemAsync(UserStorageKey);

            _logger.LogInformation("User logged out");
            AuthenticationStateChanged?.Invoke(this, false);

            // Redirect to home page
            _navigationManager.NavigateTo("/", forceLoad: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
        }
    }

    public async Task<bool> EnsureTokenValidAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_currentToken))
            {
                await LoadStoredAuthDataAsync();
            }

            if (string.IsNullOrWhiteSpace(_currentToken))
            {
                return false;
            }

            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadJwtToken(_currentToken);

            // Check if token expires within next 5 minutes
            if (jwtToken.ValidTo < DateTime.UtcNow.AddMinutes(5))
            {
                await LogoutAsync();
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            await LogoutAsync();
            return false;
        }
    }

    private async Task LoadStoredAuthDataAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsStringAsync(TokenStorageKey);
            var hasStoredToken = !string.IsNullOrWhiteSpace(token);

            if (hasStoredToken)
            {
                var jwtHandler = new JwtSecurityTokenHandler();
                if (jwtHandler.CanReadToken(token))
                {
                    var jwtToken = jwtHandler.ReadJwtToken(token);

                    // Check if token is still valid
                    if (jwtToken.ValidTo > DateTime.UtcNow)
                    {
                        _currentToken = token;
                        _isAuthenticated = true;
                        return;
                    }
                }
            }

            // Token is invalid or missing, clear storage only if we had something stored
            if (hasStoredToken)
            {
                await LogoutAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading stored auth data");
            // Only call logout if we had previously stored data
            var hasStoredData = !string.IsNullOrWhiteSpace(await _localStorage.GetItemAsStringAsync(TokenStorageKey));
            if (hasStoredData)
            {
                await LogoutAsync();
            }
        }
    }
}
