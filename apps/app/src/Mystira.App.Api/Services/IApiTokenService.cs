using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Api.Services;

public interface IApiTokenService
{
    (string AccessToken, DateTime ExpiresAtUtc) CreateAccessToken(Account account, string authProvider);
}
