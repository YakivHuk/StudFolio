using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Security.Claims;

namespace StudFolio.Filters
{
    public class AuthorizeOrNotFoundAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public AuthorizeOrNotFoundAttribute(params string[] roles)
        {
            _allowedRoles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                SetNotFoundResult(context);
                return;
            }

            if (_allowedRoles != null && _allowedRoles.Length > 0)
            {
                var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(roleClaim) || !_allowedRoles.Contains(roleClaim))
                {
                    SetNotFoundResult(context);
                }
            }
        }

        private void SetNotFoundResult(AuthorizationFilterContext context)
        {
            var viewResult = new ViewResult
            {
                ViewName = "NotFound"
            };
            viewResult.StatusCode = 404;
            context.Result = viewResult;
        }
    }
}