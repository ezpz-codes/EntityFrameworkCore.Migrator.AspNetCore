using EntityFrameworkCore.Migrator.AspNetCore.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Migrator.AspNetCore.Pages;

[MigratorAuth]
public class MigrationsModel : PageModel
{
    private readonly EntityFrameworkCoreMigratorOptions _options;
    private readonly IServiceProvider _serviceProvider;

    public MigrationsModel(EntityFrameworkCoreMigratorOptions options, IServiceProvider serviceProvider)
    {
        _options = options;
        _serviceProvider = serviceProvider;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!_options.ShowMigrations)
        {
            return NotFound();
        }

        var context = _serviceProvider.GetService(_options.DbContextType) as DbContext;

        if (context is null)
        {
            throw new Exception($"Required context type is not registered ({_options.DbContextType.FullName})");
        }

        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

        ViewData["appliedMigrations"] = appliedMigrations;
        ViewData["pendingMigrations"] = pendingMigrations;

        return Page();
    }
}
