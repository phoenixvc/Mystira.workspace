namespace Mystira.Contracts.App.Requests.Accounts;

public record CreateAccountRequest
{
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
}

public record UpdateAccountRequest
{
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
}

public record UpdateSubscriptionRequest
{
    public string Type { get; set; } = string.Empty;
    public string? ProductId { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string? PurchaseToken { get; set; }
    public List<string>? PurchasedScenarios { get; set; }
}
