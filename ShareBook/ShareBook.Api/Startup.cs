using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Rollbar.NetCore.AspNet;
using ShareBook.Api.Configuration;
using ShareBook.Api.Filters;
using ShareBook.Api.Middleware;
using ShareBook.Api.Services;
using ShareBook.Repository;
using ShareBook.Service;
using ShareBook.Service.AwsSqs;
using ShareBook.Service.Muambator;
using ShareBook.Service.Notification;
using ShareBook.Service.Server;
using ShareBook.Service.Upload;
using System;
using System.Text.Json.Serialization;


namespace ShareBook.Api
{
    public class Startup
    {
        /// <summary>
        /// Only should be used for integration tests
        /// </summary>
        public static bool IgnoreMigrations = false;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDatabaseConfiguration(Configuration);

            services.RegisterRepositoryServices();
            services.AddAutoMapper(cfg => { }, typeof(Startup).Assembly);

            services
                .AddControllers(x =>
                {
                    x.Filters.Add(typeof(ValidateModelStateFilterAttribute));
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                }).AddNewtonsoftJson();

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddHttpContextAccessor();

            services.Configure<ImageSettings>(options => Configuration.GetSection("ImageSettings").Bind(options));

            services.Configure<EmailSettings>(options => Configuration.GetSection("EmailSettings").Bind(options));

            services.Configure<ServerSettings>(options => Configuration.GetSection("ServerSettings").Bind(options));

            services.Configure<PushNotificationSettings>(options => Configuration.GetSection("PushNotificationSettings").Bind(options));

            services.Configure<AwsSqsSettings>(options => Configuration.GetSection("AWSSQSSettings").Bind(options));

            services.Configure<MeetupSettings>(options => Configuration.GetSection("MeetupSettings").Bind(options));

            services.AddHttpContextAccessor();

            JWTConfig.RegisterJWT(services, Configuration);

            services.RegisterSwagger();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllHeaders",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });


            RollbarConfigurator
                .Configure(environment: Configuration.GetSection("Rollbar:Environment").Value,
                           isActive: Configuration.GetSection("Rollbar:IsActive").Value,
                           token: Configuration.GetSection("Rollbar:Token").Value,
                           logLevel: Configuration.GetSection("Rollbar:LogLevel").Value);

            services.AddRollbarLogger();

            MuambatorConfigurator.Configure(Configuration.GetSection("Muambator:Token").Value, Configuration.GetSection("Muambator:IsActive").Value);

            services.AddMemoryCache();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            bool rollbarActive = Configuration.GetSection("Rollbar:IsActive").Value == null ? false : Convert.ToBoolean(Configuration.GetSection("Rollbar:IsActive").Value.ToLower());
            if (rollbarActive)
            {
                app.UseRollbarMiddleware();
            }

            app.UseHealthChecks("/hc");
            app.UseExceptionHandlerMiddleware();

            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = (context) =>
                {
                    // Enable cors
                    context.Context.Response.Headers["Access-Control-Allow-Origin"] = "*";
                }
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SHAREBOOK API V1");
            });

            app.UseRouting();

            app.UseCors("AllowAllHeaders");
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Book}/{action=Index}/{id?}");

                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    AllowCachingResponses = false,
                    ResultStatusCodes =
                    {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status200OK,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                    }
                });
            });

            if (!IgnoreMigrations)
            {
                DatabaseConfiguration.EnsureDatabaseCreated(app.ApplicationServices, Configuration);

                if (env.IsDevelopment() || env.IsStaging())
                {
                    using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
                    var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                    var sharebookSeeder = new ShareBookSeeder(context);
                    sharebookSeeder.Seed();
                }
            }
        }

    }
}