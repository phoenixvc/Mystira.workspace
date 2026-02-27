using Mystira.App.PWA.Models;

namespace Mystira.App.PWA.Services;

public interface IMagicAuthApiClient
{
    Task<MagicSignupResult?> RequestMagicLinkAsync(string email, string? displayName);
    Task<MagicSignupResult?> ResendMagicLinkAsync(string email);
    Task<VerifyMagicSignupResult?> VerifyMagicLinkAsync(string token);
    Task<MagicConsumeResponse?> ConsumeMagicLinkAsync(string token);
    Task<Account?> BootstrapAccountAsync();
}

public class MagicAuthApiClient : BaseApiClient, IMagicAuthApiClient
{
    public MagicAuthApiClient(HttpClient httpClient, ILogger<MagicAuthApiClient> logger, ITokenProvider tokenProvider)
        : base(httpClient, logger, tokenProvider)
    {
    }

    public Task<MagicSignupResult?> RequestMagicLinkAsync(string email, string? displayName)
    {
        return SendPostAsync<object, MagicSignupResult>(
            "api/auth/magic/request",
            new { email, displayName },
            "RequestMagicLink");
    }

    public Task<MagicSignupResult?> ResendMagicLinkAsync(string email)
    {
        return SendPostAsync<object, MagicSignupResult>(
            "api/auth/magic/resend",
            new { email },
            "ResendMagicLink");
    }

    public Task<VerifyMagicSignupResult?> VerifyMagicLinkAsync(string token)
    {
        return SendPostAsync<object, VerifyMagicSignupResult>(
            "api/auth/magic/verify",
            new { token },
            "VerifyMagicLink");
    }

    public Task<MagicConsumeResponse?> ConsumeMagicLinkAsync(string token)
    {
        return SendPostAsync<object, MagicConsumeResponse>(
            "api/auth/magic/consume",
            new { token },
            "ConsumeMagicLink");
    }

    public async Task<Account?> BootstrapAccountAsync()
    {
        return await SendPostAsync<object, Account>(
            "api/auth/bootstrap-account",
            new { },
            "BootstrapAccount",
            requireAuth: true);
    }
}

public record MagicSignupResult(string PendingSignupId, string Status, string Message);

public record VerifyMagicSignupResult(
    string Status,
    string Message,
    bool CanContinueWithEmail,
    bool CanContinueWithEntra);

public record MagicConsumeResponse(
    string? AccessToken,
    DateTime? ExpiresAtUtc,
    Account? Account,
    string Status,
    string Message);
