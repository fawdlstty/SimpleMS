using Fawdlstty.SimpleMS.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fawdlstty.SimpleMS.Security {
	public class SecurityOperationFilter: IOperationFilter {
		public void Apply (OpenApiOperation operation, OperationFilterContext context) {
			if (operation == null || context == null)
				return;
			var requiredScopes = context.MethodInfo.GetCustomAttributes (true).OfType<MethodAuthAttribute> ().Select (attr => attr.Policy).Distinct ();

			if (requiredScopes.Any ()) {
				operation.Responses.Add ("401", new OpenApiResponse { Description = "Unauthorized" });
				operation.Responses.Add ("403", new OpenApiResponse { Description = "Forbidden" });
				operation.Description = $"{requiredScopes.First ()}<br />{operation.Description}";
				operation.Security = new List<OpenApiSecurityRequirement> {
					new OpenApiSecurityRequirement {
						[new OpenApiSecurityScheme {
							Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
						}] = requiredScopes.ToList ()
					}
				};
			}
		}
	}
}
