using EntityFrameworkCore.Migrator.AspNetCore.Auth;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EntityFrameworkCore.Migrator.AspNetCore.Pages;

[MigratorAuth]
public class HomeModel : PageModel
{
    public void OnGet()
    {
    }
}
