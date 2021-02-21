using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rollbar.NetPlatformExtensions;
using ShareBook.Api.Configuration;
using ShareBook.Api.Middleware;
using ShareBook.Api.Services;
using ShareBook.Repository;
using ShareBook.Service;
using ShareBook.Service.AWSSQS;
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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            
            var isDocker = Environment.GetEnvironmentVariable("IS_DOCKER");
            var connectionStringKey = isDocker == "1" ? "DefaultConnectionDocker" : "DefaultConnection";

            RegisterHealthChecks(services, Configuration.GetConnectionString(connectionStringKey));

            services.RegisterRepositoryServices();
            services.AddAutoMapper(typeof(Startup));

            services
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                }).AddNewtonsoftJson();

            services
                .AddHttpContextAccessor()
                .Configure<RollbarOptions>(options => Configuration.GetSection("Rollbar").Bind(options))
                .AddRollbarLogger(loggerOptions => loggerOptions.Filter = (loggerName, loglevel) => loglevel >= LogLevel.Trace);

            services.Configure<ImageSettings>(options => Configuration.GetSection("ImageSettings").Bind(options));

            services.Configure<EmailSettings>(options => Configuration.GetSection("EmailSettings").Bind(options));

            services.Configure<ServerSettings>(options => Configuration.GetSection("ServerSettings").Bind(options));

            services.Configure<NotificationSettings>(options => Configuration.GetSection("NotificationSettings").Bind(options));

            services.Configure<AWSSQSSettings>(options => Configuration.GetSection("AWSSQSSettings").Bind(options));

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

            services
                .AddDbContext<ApplicationDbContext>(options =>
                    options
                        .UseSqlServer(Configuration.GetConnectionString(connectionStringKey)));

            RollbarConfigurator
                .Configure(environment: Configuration.GetSection("Rollbar:Environment").Value,
                           isActive: Configuration.GetSection("Rollbar:IsActive").Value,
                           token: Configuration.GetSection("Rollbar:Token").Value,
                           logLevel: Configuration.GetSection("Rollbar:LogLevel").Value);

            MuambatorConfigurator.Configure(Configuration.GetSection("Muambator:Token").Value, Configuration.GetSection("Muambator:IsActive").Value);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            bool rollbarActive = Convert.ToBoolean(Configuration.GetSection("Rollbar:IsActive").Value.ToLower());
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

                endpoints.MapFallbackToController("Index", "ClientSpa");

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

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var scopeServiceProvider = serviceScope.ServiceProvider;
                var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                context.Database.Migrate();
                if (env.IsDevelopment() || env.IsStaging())
                {
                    var sharebookSeeder = new ShareBookSeeder(context);
                    sharebookSeeder.Seed();
                }
            }
        }

        private void RegisterHealthChecks(IServiceCollection services, string connectionString)
        {
            services.AddHealthChecks()
                .AddSqlServer(connectionString);
        }
    }
}