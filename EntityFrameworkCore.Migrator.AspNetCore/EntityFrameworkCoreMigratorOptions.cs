using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EntityFrameworkCore.Migrator.AspNetCore;

public class EntityFrameworkCoreMigratorOptions
{
    /// <summary>
    /// Creates a new instance on EntityFrameworkCoreMigratorOptions.
    /// </summary>
    /// <param name="dbContextType">Type of database context for which the migration management is needed (needs to be a subclass of DbContext).</param>
    public EntityFrameworkCoreMigratorOptions(Type dbContextType)
    {
        if (!dbContextType.IsAssignableTo(typeof(DbContext)))
        {
            throw new ArgumentException("EntityFrameworkCoreMigratorOptions expects a type that inherits DbContext.", nameof(dbContextType));
        }

        DbContextType = dbContextType;
    }

    /// <summary>
    /// Type of database context for which the migration management is needed.
    /// </summary>
    public Type DbContextType { get; }

    /// <summary>
    /// Title that appears in the navbar as a link to the homepage of the migrator tool. Default value: "Migrator".
    /// </summary>
    public string Title { get; set; } = "Migrator";

    /// <summary>
    /// Title that appears on the home page. Default value: "Home".
    /// </summary>
    public string HomeTitle { get; set; } = "Home";

    /// <summary>
    /// Text that appears on the home page below the title. Default value: "This tool simplifies management of EF Core migrations by providing you a UI. You can either manage your code-first migrations (created previously from CLI), or by finding the difference between live database model and the model you have in your code.".
    /// </summary>
    public string HomeText { get; set; } = "This tool simplifies management of EF Core migrations by providing you a UI. You can either manage your code-first migrations (created previously from CLI), or by finding the difference between live database model and the model you have in your code.";

    /// <summary>
    /// <see langword="true"/> if either <see cref="RestrictToRoles"/> or <see cref="AuthorizationMethod"/> has been set, <see langword="false"/> otherwise.
    /// </summary>
    public bool IsSecuritySet => RestrictToRoles is not null && RestrictToRoles.Any() || AuthorizationMethod is not null;

    /// <summary>
    /// <see langword="true"/> if the tool should ignore column order when generating live migrations, <see langword="false"/> otherwise. 
    /// The reason for this parameter is that in case you haven't specified Order on you model's properties, EntityFrameworkCore generates unneccessary ALTER TABLE commands. Default value: <see langword="true"/>.
    /// </summary>
    public bool ShouldIgnoreColumnOrderForLiveMigrations { get; set; } = true;

    /// <summary>
    /// <see langword="true"/> if the tool should display the page for managing migrations generated with CLI, <see langword="false"/> otherwise. Default value: <see langword="true"/>.
    /// </summary>
    public bool ShowMigrations { get; set; } = true;

    /// <summary>
    /// <see langword="true"/> if the tool should display the page for generating and running live migrations, <see langword="false"/> otherwise. Default value: <see langword="true"/>.
    /// </summary>
    public bool ShowLiveMigration { get; set; } = true;

    /// <summary>
    /// Roles that are allowed to access the UI for migrations management. Ignored if <see cref="AuthorizationMethod"/> is specified. 
    /// Either <see cref="RestrictToRoles"/> or <see cref="AuthorizationMethod"/> need to be specified in order to access the migrations management UI in non-development environment.
    /// </summary>
    public IEnumerable<string>? RestrictToRoles { get; set; }

    /// <summary>
    /// Custom method for authorizing the migrations management UI. 
    /// The method is async and needs to have one parameter of type <see cref="HttpContext"/> (current HTTP context), and needs to return <see langword="true"/> if the request should be authorized, or <see langword="false"/> otherwise.
    /// Either <see cref="RestrictToRoles"/> or <see cref="AuthorizationMethod"/> need to be specified in order to access the migrations management UI in non-development environment.
    /// </summary>
    public Func<HttpContext, Task<bool>>? AuthorizationMethod { get; set; }

    /// <summary>
    /// Method that is applied before the collection of <see cref="MigrationOperation"/> is converted to the collection of <see cref="MigrationCommand"/>.
    /// The method is async and needs to have for parameters of type <see cref="IServiceProvider"/> (service provider containing all the registered services), <see cref="IReadOnlyList{MigrationOperation}"/> (list of initial migration operations), <see cref="IModel"/> (database model) an <see cref="IModel"/> (code model), and needs to return new <see cref="IReadOnlyList{MigrationOperation}"/> that the tool will then convert to commands.
    /// This parameter cold be useful if there is a need for filter certain operations or adding new ones.
    /// </summary>
    public Func<IServiceProvider, IReadOnlyList<MigrationOperation>, IModel, IModel, Task<IReadOnlyList<MigrationOperation>>>? GetMigrationOperations { get; set; }
}
