using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ShareBook.Repository.Infra.CrossCutting.Identity;
using ShareBook.Repository.Infra.CrossCutting.Identity.Configurations;
using System;

namespace ShareBook.Api.Configuration
{
    public class JWTConfig
    {

        public void RegisterJWT(IServiceCollection services, IConfiguration configuration)
        {
            var tokenConfigurations = configureToken(services, configuration);
            var signingConfigurations = configureSigning(services);
            configureAuth(services, signingConfigurations, tokenConfigurations);

        }

        private TokenConfigurations configureToken(IServiceCollection services, IConfiguration configuration)
        {
            var tokenConfigurations = new TokenConfigurations();
            new ConfigureFromConfigurationOptions<TokenConfigurations>(
                configuration.GetSection("TokenConfigurations"))
                    .Configure(tokenConfigurations);
            services.AddSingleton(tokenConfigurations);

            return tokenConfigurations;
        }

        private SigningConfigurations configureSigning(IServiceCollection services)
        {
            var signingConfigurations = new SigningConfigurations();
            services.AddSingleton(signingConfigurations);

            return signingConfigurations;
        }

        private void configureAuth(IServiceCollection services, SigningConfigurations signingConfigurations, TokenConfigurations tokenConfigurations)
        {
            services.AddAuthentication(authOptions =>
            {
                authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(bearerOptions =>
            {
                var paramsValidation = bearerOptions.TokenValidationParameters;
                paramsValidation.IssuerSigningKey = signingConfigurations.Key;


                // Valida a assinatura de um token recebido
                paramsValidation.ValidateIssuerSigningKey = true;

                paramsValidation.ValidAudience = tokenConfigurations.Audience;
                paramsValidation.ValidIssuer = tokenConfigurations.Issuer;

                // Verifica se um token recebido ainda é válido
                paramsValidation.ValidateLifetime = true;

                // Tempo de tolerância para a expiração de um token (utilizado
                // caso haja problemas de sincronismo de horário entre diferentes
                // computadores envolvidos no processo de comunicação)
                paramsValidation.ClockSkew = TimeSpan.Zero;
            });

            // Ativa o uso do token como forma de autorizar o acesso
            // a recursos deste projeto
            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌​)
                    .RequireAuthenticatedUser().Build());
            });

        }
    }
}
