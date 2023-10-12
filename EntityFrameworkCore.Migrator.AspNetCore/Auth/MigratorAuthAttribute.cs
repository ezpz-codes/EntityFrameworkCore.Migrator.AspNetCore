using Microsoft.AspNetCore.Mvc;

namespace EntityFrameworkCore.Migrator.AspNetCore.Auth;

public class MigratorAuthAttribute : TypeFilterAttribute
{
    public MigratorAuthAttribute() : base(typeof(MigratorFilter)) { }
}
