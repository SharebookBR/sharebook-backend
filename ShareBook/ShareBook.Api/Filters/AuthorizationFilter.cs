using Microsoft.AspNetCore.Mvc.Filters;
using ShareBook.Domain.Exceptions;
using ShareBook.Service.Authorization;
using System.Linq;
using System.Security.Claims;

namespace ShareBook.Api.Filters;

public class AuthorizationFilter(params Permissions.Permission[] permissions) : ActionFilterAttribute
{
    public Permissions.Permission[] NecessaryPermissions { get; set; } = permissions;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User;

        if (user?.Identity == null)
            throw new ShareBookException(ShareBookException.Error.NotAuthorized);

        var isAdministrator = ((ClaimsIdentity)user.Identity).Claims
            .Any(x => x.Type == ClaimsIdentity.DefaultRoleClaimType.ToString() && x.Value == Domain.Enums.Profile.Administrator.ToString());

        if (NecessaryPermissions.Any(x => Permissions.AdminPermissions.Contains(x)) && !isAdministrator)
            throw new ShareBookException(ShareBookException.Error.Forbidden);

        base.OnActionExecuting(context);
    }
}
