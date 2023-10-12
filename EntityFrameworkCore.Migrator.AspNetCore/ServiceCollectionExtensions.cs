using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EntityFrameworkCore.Migrator.AspNetCore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkCoreMigrator<TContext>(this IServiceCollection services, Action<EntityFrameworkCoreMigratorOptions>? optionsConfiguration = null) where TContext : DbContext
    {
        return services.AddEntityFrameworkCoreMigrator(typeof(TContext), optionsConfiguration);
    }

    public static IServiceCollection AddEntityFrameworkCoreMigrator(this IServiceCollection services, Type dbContextType, Action<EntityFrameworkCoreMigratorOptions>? optionsConfiguration = null)
    {
        if (!dbContextType.IsAssignableTo(typeof(DbContext)))
        {
            throw new ArgumentException("AddEntityFrameworkCoreMigrator expects a type that inherits DbContext.", nameof(dbContextType));
        }

        var options = new EntityFrameworkCoreMigratorOptions(dbContextType);

        optionsConfiguration?.Invoke(options);

        services.AddSingleton(options);

        return services;
    }
}
