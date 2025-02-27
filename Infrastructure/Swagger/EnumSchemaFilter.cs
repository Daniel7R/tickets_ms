using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TicketsMS.Infrastructure.Swagger
{
    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                schema.Enum = Enum.GetNames(context.Type)
                    .Select(n => (IOpenApiAny)new OpenApiString(n))
                    .ToList();
                schema.Type = "string";
                schema.Format = null;
            }
        }
    }
}
