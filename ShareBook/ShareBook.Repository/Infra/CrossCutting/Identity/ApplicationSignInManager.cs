using Microsoft.IdentityModel.Tokens;
using ShareBook.Domain;
using ShareBook.Repository.Infra.CrossCutting.Identity.Interfaces;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;

namespace ShareBook.Repository.Infra.CrossCutting.Identity.Configurations
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

            DateTime creationDate = DateTime.Now;
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
                created = creationDate.ToString("yyyy-MM-dd HH:mm:ss"),
                expiration = expireDate.ToString("yyyy-MM-dd HH:mm:ss"),
                accessToken = token,
                name = user.Name,
                email = user.Email,
                userId = user.Id,
                message = "OK"
            };
        }
    }
}
