using Microsoft.AspNetCore.Mvc.Filters;
using ShareBook.Domain.Exceptions;
using ShareBook.Service.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ShareBook.Api.Filters
{
    public class AuthorizationFilter : ActionFilterAttribute
    {
        public Permissions.Permission[] NecessaryPermissions { get; set; }

        public AuthorizationFilter(params Permissions.Permission[] permissions)
        {
            NecessaryPermissions = permissions;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;

            if (user == null)
                throw new ShareBookException(ShareBookException.Error.NotAuthorized);

            var isAdministrator = ((ClaimsIdentity)user.Identity).Claims
                .Any(x => x.Type == ClaimsIdentity.DefaultRoleClaimType.ToString() && x.Value == Domain.Enums.Profile.Administrator.ToString());

            if (NecessaryPermissions.Any(x => Permissions.AdminPermissions.Contains(x)) && !isAdministrator)
                throw new ShareBookException(ShareBookException.Error.Forbidden);

            base.OnActionExecuting(context);
        }
    }
}
