using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Acme.SaaS.API.Filters;

public class SwaggerDefaultValues : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= [];

        if (!operation.Parameters.Any(p => p.Name?.Equals("X-Tenant-Id", StringComparison.OrdinalIgnoreCase) == true))
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Tenant-Id",
                In = ParameterLocation.Header,
                Description = "Tenant identifier (required for all requests)",
                Required = false,
                Schema = new OpenApiSchema { Type = JsonSchemaType.String }
            });
        }
    }
}
