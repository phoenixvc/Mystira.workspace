using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Mystira.Admin.Api.Configuration;

/// <summary>
/// Parameter filter to prevent Swashbuckle from generating parameters for IFormFile
/// These will be handled by FileUploadOperationFilter instead
/// </summary>
public class FileUploadParameterFilter : IParameterFilter
{
    public void Apply(IOpenApiParameter parameter, ParameterFilterContext context)
    {
        // No-op: file/form handling is performed by FileUploadOperationFilter.
        _ = parameter;
        _ = context;
    }
}
