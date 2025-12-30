using Mystira.App.Admin.Api.Models;
using Mystira.Contracts.App.Responses.Client;

namespace Mystira.App.Admin.Api.Services;

public interface IClientStatusService
{
    Task<ClientStatusResponse> GetClientStatusAsync(ClientStatusRequest request);
}
