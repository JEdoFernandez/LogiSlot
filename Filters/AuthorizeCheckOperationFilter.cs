using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace LogiSlot.Filters
{
    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Verificar si el método o el controlador tiene el atributo [Authorize]
            var hasAuthorize = context.MethodInfo.DeclaringType != null && (
                context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ||
                context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
            );

            if (hasAuthorize)
            {
                if (operation.Security == null)
                    operation.Security = new List<OpenApiSecurityRequirement>();

                var oAuthScheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                };

                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [oAuthScheme] = new List<string>()
                });
            }
        }
    }
}
