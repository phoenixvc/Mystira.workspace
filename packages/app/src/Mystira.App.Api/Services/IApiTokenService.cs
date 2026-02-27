using Mystira.App.Domain.Models;

namespace Mystira.App.Api.Services;

public interface IApiTokenService
{
    (string AccessToken, DateTime ExpiresAtUtc) CreateAccessToken(Account account, string authProvider);
}
