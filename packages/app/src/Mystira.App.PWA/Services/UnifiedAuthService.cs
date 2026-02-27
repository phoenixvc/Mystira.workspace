using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Mystira.App.PWA.Models;

namespace Mystira.App.PWA.Services;

/// <summary>
/// Unified authentication service that provides dual-path authentication (Entra ID + Magic Link).
/// Manages authentication state, token handling, and provider switching.
/// Implements IDisposable to properly clean up event subscriptions.
/// </summary>
public class UnifiedAuthService : IAuthService, IDisposable
{
    private readonly EntraExternalIdAuthService _entraAuthService;
    private readonly IMagicAuthApiClient _magicAuthClient;
    private readonly ILogger<UnifiedAuthService> _logger;
    private readonly NavigationManager _navigationManager;
    private readonly SemaphoreSlim _stateLock = new(1, 1);

    private Account? _currentAccount;
    private string? _currentToken;
    private string? _currentAuthProvider;
    private DateTime? _tokenExpiryTime;
    private bool _isAuthenticated;

    /// <summary>
    /// Event raised when authentication state changes (sign in/out).
    /// </summary>
    public event EventHandler<bool>? AuthenticationStateChanged;

    /// <summary>
    /// Event raised when token is about to expire (within 5 minutes).
    /// </summary>
    public event EventHandler? TokenExpiryWarning;

    /// <summary>
    /// Initializes a new instance of the UnifiedAuthService.
    /// </summary>
    /// <param name="entraAuthService">The Entra External ID authentication service.</param>
    /// <param name="magicAuthClient">The magic link authentication API client.</param>
    /// <param name="logger">Logger for diagnostic information.</param>
    /// <param name="navigationManager">Navigation manager for redirect handling.</param>
    public UnifiedAuthService(
        EntraExternalIdAuthService entraAuthService,
        IMagicAuthApiClient magicAuthClient,
        ILogger<UnifiedAuthService> logger,
        NavigationManager navigationManager)
    {
        _entraAuthService = entraAuthService;
        _magicAuthClient = magicAuthClient;
        _logger = logger;
        _navigationManager = navigationManager;

        // Subscribe to underlying service events
        _entraAuthService.AuthenticationStateChanged += OnEntraAuthenticationStateChanged;
        _entraAuthService.TokenExpiryWarning += OnEntraTokenExpiryWarning;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        await _stateLock.WaitAsync();
        try
        {
            if (_currentAuthProvider == "entra")
            {
                _stateLock.Release();
                return await _entraAuthService.IsAuthenticatedAsync();
            }

            return _isAuthenticated && !string.IsNullOrWhiteSpace(_currentToken);
        }
        finally
        {
            if (_currentAuthProvider != "entra")
            {
                _stateLock.Release();
            }
        }
    }

    public async Task<Account?> GetCurrentAccountAsync()
    {
        await _stateLock.WaitAsync();
        try
        {
            if (_currentAuthProvider == "entra")
            {
                _stateLock.Release();
                return await _entraAuthService.GetCurrentAccountAsync();
            }

            return _currentAccount;
        }
        finally
        {
            if (_currentAuthProvider != "entra")
            {
                _stateLock.Release();
            }
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        await _stateLock.WaitAsync();
        try
        {
            if (_currentAuthProvider == "entra")
            {
                _stateLock.Release();
                return await _entraAuthService.GetTokenAsync();
            }

            return _currentToken;
        }
        finally
        {
            if (_currentAuthProvider != "entra")
            {
                _stateLock.Release();
            }
        }
    }

    public async Task<string?> GetCurrentTokenAsync()
    {
        return await GetTokenAsync();
    }

    public async Task LogoutAsync()
    {
        await _stateLock.WaitAsync();
        try
        {
            if (_currentAuthProvider == "entra")
            {
                _stateLock.Release();
                await _entraAuthService.LogoutAsync();
                await _stateLock.WaitAsync();
            }

            // Clear unified state
            _currentAccount = null;
            _currentToken = null;
            _currentAuthProvider = null;
            _tokenExpiryTime = null;
            _isAuthenticated = false;

            AuthenticationStateChanged?.Invoke(this, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
        }
        finally
        {
            _stateLock.Release();
        }
    }

    public void SetRememberMe(bool rememberMe)
    {
        if (_currentAuthProvider == "entra")
        {
            _entraAuthService.SetRememberMe(rememberMe);
        }
    }

    public Task<bool> GetRememberMeAsync()
    {
        if (_currentAuthProvider == "entra")
        {
            return _entraAuthService.GetRememberMeAsync();
        }

        return Task.FromResult(false); // Default for magic auth
    }

    public DateTime? GetTokenExpiryTime()
    {
        if (_currentAuthProvider == "entra")
        {
            return _entraAuthService.GetTokenExpiryTime();
        }

        return _tokenExpiryTime;
    }

    public async Task<bool> EnsureTokenValidAsync(CancellationToken cancellationToken = default)
    {
        await _stateLock.WaitAsync(cancellationToken);
        try
        {
            if (_currentAuthProvider == "entra")
            {
                _stateLock.Release();
                return await _entraAuthService.EnsureTokenValidAsync();
            }

            // For magic auth, check if token is expired
            if (_tokenExpiryTime.HasValue && _tokenExpiryTime.Value < DateTime.UtcNow.AddMinutes(5))
            {
                _stateLock.Release();
                await LogoutAsync();
                await _stateLock.WaitAsync(cancellationToken);
                return false;
            }

            return true;
        }
        finally
        {
            _stateLock.Release();
        }
    }

    /// <summary>
    /// Sets the authenticated session with the provided access token and account.
    /// Updates the current authentication provider and token expiry information.
    /// </summary>
    /// <param name="accessToken">The access token for the session.</param>
    /// <param name="account">The account information for the session.</param>
    public async Task SetAuthenticatedSessionAsync(string accessToken, Account account)
    {
        await SetAuthenticatedSessionAsync(accessToken, account, "magic", DateTime.UtcNow.AddHours(8));
    }

    /// <summary>
    /// Sets the authenticated session with the provided access token and account.
    /// Updates the current authentication provider and token expiry information.
    /// </summary>
    /// <param name="accessToken">The access token for the session.</param>
    /// <param name="account">The account information for the session.</param>
    /// <param name="authProvider">The authentication provider used ("entra" or "magic").</param>
    /// <param name="expiresAt">Optional token expiry time; defaults to 8 hours from now.</param>
    public async Task SetAuthenticatedSessionAsync(string accessToken, Account account, string authProvider = "magic", DateTime? expiresAt = null)
    {
        await _stateLock.WaitAsync();
        try
        {
            _currentToken = accessToken;
            _currentAccount = account;
            _currentAuthProvider = authProvider;
            _isAuthenticated = true;
            _tokenExpiryTime = expiresAt ?? DateTime.UtcNow.AddHours(8);

            AuthenticationStateChanged?.Invoke(this, true);
        }
        finally
        {
            _stateLock.Release();
        }
    }

    public async Task SetCurrentAccountAsync(Account account)
    {
        await _stateLock.WaitAsync();
        try
        {
            _currentAccount = account;
            if (_currentAuthProvider == "entra")
            {
                _stateLock.Release();
                await _entraAuthService.SetCurrentAccountAsync(account);
                await _stateLock.WaitAsync();
            }
        }
        finally
        {
            _stateLock.Release();
        }
    }

    // Dual-path authentication methods
    public async Task<AuthResult> SignInWithEntraAsync()
    {
        try
        {
            _currentAuthProvider = "entra";
            await _entraAuthService.LoginWithEntraAsync();

            // For Entra, the login flow is async and will complete via callback
            // We can't immediately get the result, so we return a pending result
            return new AuthResult(true, null, null, null, null, "entra");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Entra sign-in");
            return new AuthResult(false, null, ex.Message);
        }
    }

    public async Task<AuthResult> SignInWithMagicLinkAsync(string email, string? displayName = null)
    {
        try
        {
            var result = await RequestMagicLinkAsync(email, displayName);

            if (result != null)
            {
                return new AuthResult(true, null, null, null, null, "magic");
            }

            return new AuthResult(false, null, "Failed to request magic link");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during magic link request");
            return new AuthResult(false, null, ex.Message);
        }
    }

    public async Task<AuthResult> CompleteMagicLinkSignInAsync(string token)
    {
        try
        {
            var response = await _magicAuthClient.ConsumeMagicLinkAsync(token);

            if (response != null && response.Account != null && !string.IsNullOrWhiteSpace(response.AccessToken))
            {
                _currentAuthProvider = "magic";
                await SetAuthenticatedSessionAsync(response.AccessToken, response.Account);
                _tokenExpiryTime = response.ExpiresAtUtc;

                return new AuthResult(true, response.Account, null, response.AccessToken, response.ExpiresAtUtc, "magic");
            }

            return new AuthResult(false, null, response?.Message ?? "Failed to consume magic link");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing magic link sign-in");
            return new AuthResult(false, null, ex.Message);
        }
    }

    public async Task<MagicSignupResult?> RequestMagicLinkAsync(string email, string? displayName = null)
    {
        try
        {
            return await _magicAuthClient.RequestMagicLinkAsync(email, displayName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting magic link");
            return null;
        }
    }

    public async Task<MagicSignupResult?> ResendMagicLinkAsync(string email)
    {
        try
        {
            return await _magicAuthClient.ResendMagicLinkAsync(email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending magic link");
            return null;
        }
    }

    public async Task<VerifyMagicSignupResult?> VerifyMagicLinkAsync(string token)
    {
        try
        {
            return await _magicAuthClient.VerifyMagicLinkAsync(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying magic link");
            return null;
        }
    }

    public async Task<AuthResult> RefreshAuthAsync()
    {
        try
        {
            if (_currentAuthProvider == "entra")
            {
                var success = await _entraAuthService.EnsureTokenValidAsync();
                if (success)
                {
                    var account = await _entraAuthService.GetCurrentAccountAsync();
                    var token = await _entraAuthService.GetTokenAsync();

                    return new AuthResult(true, account, null, token, _entraAuthService.GetTokenExpiryTime(), "entra");
                }
            }

            // For magic auth, we need to re-authenticate
            if (_currentAuthProvider == "magic" && _currentAccount != null)
            {
                return new AuthResult(false, null, "Magic link tokens cannot be refreshed. Please sign in again.");
            }

            return new AuthResult(false, null, "No active authentication session to refresh");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing authentication");
            return new AuthResult(false, null, ex.Message);
        }
    }

    /// <summary>
    /// Handles authentication state changes from the Entra service.
    /// </summary>
    private void OnEntraAuthenticationStateChanged(object? sender, bool isAuthenticated)
    {
        if (_currentAuthProvider == "entra")
        {
            _isAuthenticated = isAuthenticated;
            AuthenticationStateChanged?.Invoke(this, isAuthenticated);
        }
    }

    /// <summary>
    /// Handles token expiry warnings from the Entra service.
    /// </summary>
    private void OnEntraTokenExpiryWarning(object? sender, EventArgs e)
    {
        if (_currentAuthProvider == "entra")
        {
            TokenExpiryWarning?.Invoke(this, e);
        }
    }

    /// <summary>
    /// Releases all resources and unsubscribes from events.
    /// </summary>
    public void Dispose()
    {
        try
        {
            _entraAuthService.AuthenticationStateChanged -= OnEntraAuthenticationStateChanged;
            _entraAuthService.TokenExpiryWarning -= OnEntraTokenExpiryWarning;
            _stateLock?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error unsubscribing from Entra service events during disposal");
        }
    }
}
