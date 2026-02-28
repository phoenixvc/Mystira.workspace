using System.Net.Http.Headers;

namespace Mystira.App.PWA.Services;

/// <summary>
/// HTTP delegating handler that automatically adds authorization headers to requests.
/// Eliminates the need for repeated SetAuthorizationHeaderAsync() calls in API clients.
/// </summary>
public class AuthorizationHandler : DelegatingHandler
{
    private readonly ITokenProvider _tokenProvider;
    private readonly ILogger<AuthorizationHandler> _logger;

    public AuthorizationHandler(ITokenProvider tokenProvider, ILogger<AuthorizationHandler> logger)
    {
        _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Skip authorization for specific endpoints that don't require it
        var path = request.RequestUri?.AbsolutePath ?? string.Empty;
        var isPublicEndpoint = path.StartsWith("/api/auth", StringComparison.OrdinalIgnoreCase) ||
                             path.Equals("/api/discord/status", StringComparison.OrdinalIgnoreCase) ||
                             path.Equals("/ping", StringComparison.OrdinalIgnoreCase);

        if (!isPublicEndpoint)
        {
            try
            {
                var isAuthenticated = await _tokenProvider.IsAuthenticatedAsync();
                if (isAuthenticated)
                {
                    var token = await _tokenProvider.GetCurrentTokenAsync();
                    if (!string.IsNullOrEmpty(token))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        _logger.LogDebug("Added Bearer token to request: {Uri}", request.RequestUri);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error adding authorization header to request: {Uri}", request.RequestUri);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
