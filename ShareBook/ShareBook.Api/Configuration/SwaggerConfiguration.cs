using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Linq;

namespace ShareBook.Api.Configuration
{
    public static class SwaggerConfiguration
    {
        public static IServiceCollection RegisterSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(swagger =>
            {
                swagger.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SHAREBOOK API",
                    Version = "v1",
                    Description = "Open Source project",
                    Contact = new OpenApiContact
                    {
                        Name = "",
                        Email = "",
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    }
                });
                swagger.ResolveConflictingActions(x => x.First());
                swagger.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey
                    }
                );
                swagger.AddSecurityRequirement(
                    new OpenApiSecurityRequirement {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] { }
                        }
                    }
                );
            });
            return services;
        }
    }
}