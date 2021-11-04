using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace ShareBook.Infra.CrossCutting.Identity
{
    public class SigningConfigurations
    {
        public SecurityKey Key { get; }
        public SigningCredentials SigningCredentials { get; }

        public SigningConfigurations(string secretJwtKey)
        {
            Key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretJwtKey));            

            SigningCredentials = new SigningCredentials(
                Key, SecurityAlgorithms.HmacSha256Signature);
        }
    }
}
