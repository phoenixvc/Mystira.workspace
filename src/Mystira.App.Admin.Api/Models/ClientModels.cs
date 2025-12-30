namespace Mystira.App.Admin.Api.Models;

/// <summary>
/// Request for checking client status
/// </summary>
public class ClientStatusRequest
{
    public string ClientVersion { get; set; } = string.Empty;
    public string? ContentVersion { get; set; }
}
