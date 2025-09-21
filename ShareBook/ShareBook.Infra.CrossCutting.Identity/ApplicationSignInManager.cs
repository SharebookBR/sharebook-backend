﻿using Microsoft.IdentityModel.Tokens;
using ShareBook.Domain;
using ShareBook.Infra.CrossCutting.Identity.Interfaces;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ShareBook.Infra.CrossCutting.Identity
{
    public class ApplicationSignInManager : IApplicationSignInManager
    {
        public object GenerateTokenAndSetIdentity(User user, SigningConfigurations signingConfigurations, TokenConfigurations tokenConfigurations)
        {
            ClaimsIdentity identity = new ClaimsIdentity(
                    new[] {
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                        new Claim(JwtRegisteredClaimNames.UniqueName, user.Id.ToString()),
                        new Claim(ClaimTypes.Role, user.Profile.ToString(), ClaimValueTypes.String, tokenConfigurations.Issuer)
                    }
                );

            DateTime creationDate = DateTime.UtcNow;
            DateTime expireDate = creationDate + TimeSpan.FromSeconds(tokenConfigurations.Seconds);

            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = tokenConfigurations.Issuer,
                Audience = tokenConfigurations.Audience,
                SigningCredentials = signingConfigurations.SigningCredentials,
                Subject = identity,
                NotBefore = creationDate,
                Expires = expireDate
            });
            var token = handler.WriteToken(securityToken);

            return new
            {
                authenticated = true,
                created = creationDate,
                expiration = expireDate,
                accessToken = token,
                name = user.Name,
                email = user.Email,
                userId = user.Id,
                profile = user.Profile.ToString(),
                message = "OK"
            };
        }
    }
}
