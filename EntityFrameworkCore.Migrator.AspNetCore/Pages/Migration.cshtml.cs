using EntityFrameworkCore.Migrator.AspNetCore.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EntityFrameworkCore.Migrator.AspNetCore.Pages;

[MigratorAuth]
public class MigrationsScriptModel : PageModel
{
    private readonly EntityFrameworkCoreMigratorOptions _options;
    private readonly IServiceProvider _serviceProvider;

    public MigrationsScriptModel(EntityFrameworkCoreMigratorOptions options, IServiceProvider serviceProvider)
    {
        _options = options;
        _serviceProvider = serviceProvider;
    }

    public string? SuccessMessage { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string id)
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

        var allMigrations = context.Database.GetMigrations();
        if (id != "0" && !allMigrations.Contains(id))
        {
            ErrorMessage = "Migration not found.";
            SuccessMessage = null;
            return Page();
        }

        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();

        var currentMigration = appliedMigrations.LastOrDefault();

        var migrator = context.GetService<IMigrator>();

        var script = migrator.GenerateScript(currentMigration, id);

        ViewData["script"] = script;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id)
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

        var migrator = context.GetService<IMigrator>();

        try
        {
            await migrator.MigrateAsync(id);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error occured while trying to apply the migration:\n\n{ex.Message}\n\n{ex.StackTrace}";
            SuccessMessage = null;
            return Page();
        }

        SuccessMessage = "Migration applied successfully.";
        ErrorMessage = null;
        return Page();
    }
}
