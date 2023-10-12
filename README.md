![Nuget](https://img.shields.io/nuget/v/EntityFrameworkCore.Migrator.AspNetCore) 

# UI for managing EF Core migrations

Tool that provides you a UI to make your migration management a little bit easier.

## Migration management

You can manage the migration created through CLI with the provided UI. The tool generates SQL that would executed on the database before running the migration so you can make sure everything is as expected.

![Migrations](https://raw.githubusercontent.com/ezpz-codes/EntityFrameworkCore.Migrator.AspNetCore/main/Screenshots/Migrations.png)
![Migration](https://raw.githubusercontent.com/ezpz-codes/EntityFrameworkCore.Migrator.AspNetCore/main/Screenshots/Migration.png)

## Live migrations

You can also generate and run SQL script that automatically migrate your database from the current state to the state you have in your code, without the need for any migrations. You can also modify all the migration operations generated by EF Core before generating the commands. 

![LiveMigrations](https://raw.githubusercontent.com/ezpz-codes/EntityFrameworkCore.Migrator.AspNetCore/main/Screenshots/LiveMigration.png)

## Setup

In `Program.cs` file, the following lines of code are needed:

```csharp
using EntityFrameworkCore.Migrator.AspNetCore;
using Microsoft.EntityFrameworkCore;

// ...

builder.Services.AddDbContext<TestDbContext>(...);
builder.Services.AddEntityFrameworkCoreMigrator<TestDbContext>();
builder.Services.AddRazorPages();

// ...

var app = builder.Build();

// ...

app.MapRazorPages();

// ...

app.Run();
```

### Authorization

In development mode, you do not need to setup any authorization, but in production it is mandatory for security purposes. You can either set up authorization by restricting by role, or writing a custom method for authorization.

```csharp
builder.Services.AddEntityFrameworkCoreMigrator<TestDbContext>(options =>
{
    options.RestrictToRoles = new [] { "Admin" };
});
```

```csharp
builder.Services.AddEntityFrameworkCoreMigrator<TestDbContext>(options =>
{
    options.AuthorizationMethod = async (httpContext) =>
    {
        // Custom authorization logic

        return true;
    };
});
```

## Versioning

The versioning of the package follows the versioning of EntityFrameworkCore package.