using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;

namespace EntityFrameworkCore.Migrator.AspNetCore.Auth;

public class MigratorFilter : IAuthorizationFilter
{
    private readonly IWebHostEnvironment _environment;
    private readonly EntityFrameworkCoreMigratorOptions _options;

    public MigratorFilter(IWebHostEnvironment environment, EntityFrameworkCoreMigratorOptions options)
    {
        _environment = environment;
        _options = options;
    }

    public async void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!_options.IsSecuritySet)
        {
            if (_environment.IsDevelopment())
            {
                return;
            }

            context.Result = new UnauthorizedResult();
            return;
        }

        if (_options.AuthorizationMethod is not null)
        {
            var isAuthorizedTask = _options.AuthorizationMethod.Invoke(context.HttpContext);
            var isAuthorized = await isAuthorizedTask;

            if (!isAuthorized)
            {
                context.Result = new UnauthorizedResult();
            }

            return;
        }

        if (_options.RestrictToRoles is not null && _options.RestrictToRoles.Any())
        {
            if (!_options.RestrictToRoles.Any(context.HttpContext.User.IsInRole))
            {
                context.Result = new UnauthorizedResult();
            }

            return;
        }
    }
}
