using PostSharp.Aspects;
using PostSharp.Serialization;
using ShareBook.Service.CustomExceptions;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace ShareBook.Service.Authorization
{
    [PSerializable]
    internal class AuthorizationInterceptorAttribute : MethodInterceptionAspect
    {
        public Permissions.Permission[] NecessaryPermissions { get; set; }

        public AuthorizationInterceptorAttribute(params Permissions.Permission[] permissions)
        {
            NecessaryPermissions = permissions;
        }

        public override void OnInvoke(MethodInterceptionArgs args)
        {
            var isAdministrator = false;
            var user = Thread.CurrentPrincipal?.Identity;
            var notAuthorizedException = new ShareBookException(ShareBookException.Error.NotAuthorized);

            if (user == null)
                throw notAuthorizedException;

            isAdministrator = ((ClaimsIdentity)Thread.CurrentPrincipal?.Identity).Claims
                .Any(x => x.Type == ClaimsIdentity.DefaultRoleClaimType.ToString() && x.Value == Domain.Enums.Profile.Administrator.ToString());

            if (NecessaryPermissions.Any(x => Permissions.AdminPermissions.Contains(x)) && !isAdministrator)
                throw notAuthorizedException;

            base.OnInvoke(args);
        }
    }
}
