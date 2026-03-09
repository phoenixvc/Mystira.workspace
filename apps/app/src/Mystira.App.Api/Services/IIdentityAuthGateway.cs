using System.Net.Http;

namespace Mystira.App.Api.Services;

public interface IIdentityAuthGateway
{
    Task<HttpResponseMessage> PostAsync<TRequest>(string path, TRequest payload, string? bearerToken = null, CancellationToken ct = default);
    Task<HttpResponseMessage> PostAsync(string path, string? bearerToken = null, CancellationToken ct = default);
    Task<HttpResponseMessage> GetAsync(string path, string? bearerToken = null, CancellationToken ct = default);
}
