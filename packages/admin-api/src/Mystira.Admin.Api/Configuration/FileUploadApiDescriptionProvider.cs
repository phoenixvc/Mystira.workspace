using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Mystira.Admin.Api.Configuration;

/// <summary>
/// API description provider that modifies IFormFile parameters to prevent Swashbuckle errors
/// </summary>
public class FileUploadApiDescriptionProvider : IApiDescriptionProvider
{
    public int Order => -100; // Run early

    public void OnProvidersExecuting(ApiDescriptionProviderContext context)
    {
        // No-op - we modify in OnProvidersExecuted
    }

    public void OnProvidersExecuted(ApiDescriptionProviderContext context)
    {
        foreach (var apiDescription in context.Results)
        {
            // Find IFormFile parameters with [FromForm]
            var formFileParameters = apiDescription.ParameterDescriptions
                .Where(p => (p.Type == typeof(IFormFile) || p.Type == typeof(IFormFile[])) &&
                           p.Source == BindingSource.Form)
                .ToList();

            if (formFileParameters.Any())
            {
                // Remove IFormFile parameters from the API description
                // They will be handled by the operation filter as part of the request body
                foreach (var param in formFileParameters)
                {
                    apiDescription.ParameterDescriptions.Remove(param);
                }
            }
        }
    }
}
