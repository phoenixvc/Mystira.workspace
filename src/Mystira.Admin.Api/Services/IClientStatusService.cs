using Mystira.Contracts.App.Requests.Client;
using Mystira.Contracts.App.Responses.Client;

namespace Mystira.Admin.Api.Services;

public interface IClientStatusService
{
    Task<ClientStatusResponse> GetClientStatusAsync(ClientStatusRequest request);
}
