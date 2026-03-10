using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Identity.Api.Services;

public interface IIdentityTokenService
{
    (string AccessToken, DateTime ExpiresAtUtc) CreateAccountToken(Account account, string authProvider);
    (string AccessToken, DateTime ExpiresAtUtc) CreateAdminToken(string username, IReadOnlyCollection<string> roles);
}
