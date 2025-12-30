using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Mystira.Contracts.App.Responses;

namespace Mystira.App.Admin.Api.Controllers;

/// <summary>
/// Provides API version and compatibility information.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ApiInfoController : ControllerBase
{
    /// <summary>
    /// Gets the current API version and compatibility information.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiVersionInfo), StatusCodes.Status200OK)]
    public ActionResult<ApiVersionInfo> GetApiInfo()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version?.ToString() ?? "1.0.0";

        return Ok(new ApiVersionInfo
        {
            ApiVersion = "v1",
            BuildVersion = version,
            MasterDataVersion = "2.0.0",
            SupportsLegacyEnums = false,
            MasterDataEntities = new[]
            {
                new MasterDataEntityInfo { Name = "CompassAxis", Endpoint = "/api/compassaxes", Count = null },
                new MasterDataEntityInfo { Name = "Archetype", Endpoint = "/api/archetypes", Count = null },
                new MasterDataEntityInfo { Name = "EchoType", Endpoint = "/api/echotypes", Count = null },
                new MasterDataEntityInfo { Name = "FantasyTheme", Endpoint = "/api/fantasythemes", Count = null },
                new MasterDataEntityInfo { Name = "AgeGroup", Endpoint = "/api/agegroups", Count = null }
            },
            DeprecatedApis = Array.Empty<DeprecatedApiInfo>()
        });
    }
}
