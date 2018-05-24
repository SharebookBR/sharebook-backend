using ShareBook.Domain;
using ShareBook.Repository.Infra.CrossCutting.Identity.Configurations;

namespace ShareBook.Repository.Infra.CrossCutting.Identity.Interfaces
{
    public interface IApplicationSignInManager
    {
        object GenerateTokenAndSetIdentity(User user, SigningConfigurations signingConfigurations, TokenConfigurations tokenConfigurations);
    }
}
