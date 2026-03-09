namespace Mystira.App.PWA.Services;

public interface ICoppaApiClient
{
    Task<ConsentStatusResponse?> GetConsentStatusAsync(string childProfileId);
    Task<RevokeConsentResponse?> RevokeConsentAsync(string childProfileId, string parentEmail);
    Task<AgeCheckResponse?> CheckAgeAsync(int age);
}

public class CoppaApiClient : BaseApiClient, ICoppaApiClient
{
    public CoppaApiClient(HttpClient httpClient, ILogger<CoppaApiClient> logger, ITokenProvider tokenProvider)
        : base(httpClient, logger, tokenProvider) { }

    public async Task<ConsentStatusResponse?> GetConsentStatusAsync(string childProfileId)
    {
        if (string.IsNullOrWhiteSpace(childProfileId))
        {
            return null;
        }

        var encoded = Uri.EscapeDataString(childProfileId);
        return await SendGetAsync<ConsentStatusResponse>(
            $"api/coppa/consent/status/{encoded}", "GetConsentStatus", requireAuth: true);
    }

    public async Task<RevokeConsentResponse?> RevokeConsentAsync(string childProfileId, string parentEmail)
    {
        if (string.IsNullOrWhiteSpace(childProfileId) || string.IsNullOrWhiteSpace(parentEmail))
        {
            return null;
        }

        return await SendPostAsync<RevokeConsentRequest, RevokeConsentResponse>(
            "api/coppa/consent/revoke",
            new RevokeConsentRequest(childProfileId, parentEmail),
            "RevokeConsent", requireAuth: true);
    }

    public async Task<AgeCheckResponse?> CheckAgeAsync(int age)
    {
        return await SendPostAsync<AgeCheckRequest, AgeCheckResponse>(
            "api/coppa/age-check", new AgeCheckRequest(age), "CheckAge");
    }
}

// DTOs
public record ConsentStatusResponse(
    string ConsentId,
    string Status,
    string Message,
    string? ChildProfileId = null);

public record RevokeConsentRequest(string ChildProfileId, string ParentEmail);

public record RevokeConsentResponse(string ConsentId, string Status, string Message);

public record AgeCheckRequest(int Age);

public record AgeCheckResponse(bool RequiresParentalConsent, string AgeGroup, string Message);
