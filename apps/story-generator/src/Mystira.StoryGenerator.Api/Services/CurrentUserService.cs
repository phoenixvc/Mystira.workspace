using Mystira.Core.Ports.Services;
using Mystira.Shared.Extensions;

namespace Mystira.StoryGenerator.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private System.Security.Claims.ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public string? GetAccountId()
    {
        return User?.GetAccountId();
    }

    public string GetRequiredAccountId()
    {
        var accountId = GetAccountId();
        if (string.IsNullOrEmpty(accountId))
        {
            throw new UnauthorizedAccessException("User is not authenticated or account ID not found");
        }
        return accountId;
    }

    public string? GetClaim(string claimType)
    {
        return User?.GetClaimValue(claimType);
    }

    public string? GetEmail()
    {
        return User?.GetEmail();
    }

    public string? GetDisplayName()
    {
        return User?.GetDisplayName();
    }
}
