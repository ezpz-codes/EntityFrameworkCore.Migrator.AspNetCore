using EntityFrameworkCore.Migrator.AspNetCore.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EntityFrameworkCore.Migrator.AspNetCore.Pages;

#pragma warning disable EF1001 // Internal EF Core API usage.

[MigratorAuth]
public class LiveModel : PageModel
{
    private readonly EntityFrameworkCoreMigratorOptions _options;
    private readonly IServiceProvider _serviceProvider;

    public LiveModel(EntityFrameworkCoreMigratorOptions options, IServiceProvider serviceProvider)
    {
        _options = options;
        _serviceProvider = serviceProvider;
    }

    public string? SuccessMessage { get; set; }

    public string? ErrorMessage { get; set; }

    public string? InfoMessage { get; set; }

    public string? WarningMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!_options.ShowLiveMigration)
        {
            return NotFound();
        }

        var context = _serviceProvider.GetService(_options.DbContextType) as DbContext;

        if (context is null)
        {
            throw new Exception($"Required context type is not registered ({_options.DbContextType.FullName})");
        }

        var designTimeServiceBuilder = new DesignTimeServicesBuilder(
            Assembly.GetExecutingAssembly(),
            Assembly.GetExecutingAssembly(),
            new EmptyReporter(),
            Array.Empty<string>());

        var serviceProvider = designTimeServiceBuilder.Build(context);

        var databaseModelFactory = serviceProvider.GetService<IDatabaseModelFactory>()!;
        var scaffoldingModelFactory = serviceProvider.GetService<IScaffoldingModelFactory>()!;
        var migrationsModelDiffer = serviceProvider.GetService<IMigrationsModelDiffer>()!;
        var migrationsSqlGenerator = serviceProvider.GetService<IMigrationsSqlGenerator>()!;

        var databaseModel = databaseModelFactory.Create(context.Database.GetConnectionString()!, new DatabaseModelFactoryOptions());

        var liveModel = scaffoldingModelFactory.Create(databaseModel, new ModelReverseEngineerOptions());
        var codeModel = context.GetService<IDesignTimeModel>().Model;

        if (_options.ShouldIgnoreColumnOrderForLiveMigrations)
        {
            var canIgnoreOrder = TryIgnoreOrder(liveModel, codeModel);
            if (!canIgnoreOrder)
            {
                WarningMessage = "Could not ignore column order. This might happen if your context does not use standard Model type.";
            }
            else
            {
                WarningMessage = null;
            }
            InfoMessage = "Live migrations are set to ignore Order of the properties. You can change this behavior by setting ShouldIgnoreColumnOrderForLiveMigrations = false in the configuration options";
        }
        else
        {
            WarningMessage = null;
            InfoMessage = "Live migrations can generate commands that change column even if they are same. This is because of the Order property. You can either define Order on properties in the model, or set ShouldIgnoreColumnOrderForLiveMigrations = true in the configuration options (or leaving it blank as it is the default value).";
        }

        var migrationOperations = migrationsModelDiffer.GetDifferences(
            liveModel.GetRelationalModel(),
            codeModel.GetRelationalModel());

        var modifiedMigrationOperations = _options.GetMigrationOperations is null
            ? migrationOperations
            : await _options.GetMigrationOperations(_serviceProvider, migrationOperations, liveModel, codeModel);

        var migrationCommands = migrationsSqlGenerator.Generate(modifiedMigrationOperations);

        ViewData["script"] = string.Join("\n", migrationCommands.Select(x => x.CommandText));

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string script)
    {
        if (!_options.ShowLiveMigration)
        {
            return NotFound();
        }

        var context = _serviceProvider.GetService(_options.DbContextType) as DbContext;

        if (context is null)
        {
            throw new Exception($"Required context type is not registered ({_options.DbContextType.FullName})");
        }

        try
        {
            using var connection = context.Database.GetDbConnection();
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            var command = connection.CreateCommand();
            command.CommandText = script;
            command.Transaction = transaction;

            try
            {
                await command.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error occured while trying to update the database:\n\n{ex.Message}\n\n{ex.StackTrace}";
                SuccessMessage = null;
                return Page();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error occured while trying to open a connection to the database:\n\n{ex.Message}\n\n{ex.StackTrace}";
            SuccessMessage = null;
            return Page();
        }

        SuccessMessage = "Database updated successfully";
        ErrorMessage = null;
        return Page();
    }

    private static bool TryIgnoreOrder(IModel liveModel, IModel codeModel)
    {
        try
        {
            var liveMutableModel = MakeMutableModel(liveModel);
            var codeMutableModel = MakeMutableModel(codeModel);

            if (liveMutableModel is null || codeMutableModel is null)
            {
                return false;
            }

            foreach (var entityType in liveMutableModel.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    property.SetColumnOrder(null);
                }
            }

            foreach (var entityType in codeMutableModel.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    property.SetColumnOrder(null);
                }
            }

            liveMutableModel.FinalizeModel();
            codeMutableModel.FinalizeModel();

            return true;
        }
        catch
        { 
            return false; 
        }
    }

    private static IMutableModel? MakeMutableModel(IModel model)
    {
        if (model is not Model)
        {
            return null;
        }

        var helperModel = new Model();

        var conventionDispatcherField = typeof(Model).GetField("_conventionDispatcher", BindingFlags.Instance | BindingFlags.NonPublic);

        if (conventionDispatcherField is null) 
        {
            return null;
        }

        conventionDispatcherField.SetValue(model, conventionDispatcherField.GetValue(helperModel));

        return model as IMutableModel;
    }
}

#pragma warning restore EF1001 // Internal EF Core API usage.