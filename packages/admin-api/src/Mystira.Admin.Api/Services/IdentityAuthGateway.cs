using System.Net.Http.Json;

namespace Mystira.Admin.Api.Services;

public class IdentityAuthGateway : IIdentityAuthGateway
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IdentityAuthGateway(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public Task<HttpResponseMessage> PostAsync<TRequest>(string path, TRequest payload, string? bearerToken = null, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("IdentityAuth");
        AttachBearer(client, bearerToken);
        return client.PostAsJsonAsync(path, payload, ct);
    }

    public Task<HttpResponseMessage> PostAsync(string path, string? bearerToken = null, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("IdentityAuth");
        AttachBearer(client, bearerToken);
        return client.PostAsync(path, content: null, ct);
    }

    public Task<HttpResponseMessage> GetAsync(string path, string? bearerToken = null, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("IdentityAuth");
        AttachBearer(client, bearerToken);

        return client.GetAsync(path, ct);
    }

    private static void AttachBearer(HttpClient client, string? bearerToken)
    {
        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
        }
    }
}
