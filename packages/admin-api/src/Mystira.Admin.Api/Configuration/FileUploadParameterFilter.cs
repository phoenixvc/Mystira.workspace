using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Mystira.Admin.Api.Configuration;

/// <summary>
/// Parameter filter to prevent Swashbuckle from generating parameters for IFormFile
/// These will be handled by FileUploadOperationFilter instead
/// </summary>
public class FileUploadParameterFilter : IParameterFilter
{
    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
    {
        // Skip parameter generation for IFormFile - it will be handled by the operation filter
        if (context.ApiParameterDescription.Type == typeof(IFormFile) ||
            context.ApiParameterDescription.Type == typeof(IFormFile[]))
        {
            // Set the parameter to null or mark it as ignored
            // This prevents Swashbuckle from trying to generate a schema for it
            parameter.Schema = null;
        }
    }
}
