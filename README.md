![Nuget](https://img.shields.io/nuget/v/EntityFrameworkCore.Migrator.AspNetCore)

# UI for managing EF Core migrations

Tool that provides you a UI to make your migration management a little bit easier.

## Migration management

You can manage the migration created through CLI with the provided UI. The tool generates SQL that would executed on the database before running the migration so you can make sure everything is as expected.

## Live migrations

You can also generate and run SQL script that automatically migrate your database from the current state to the state you have in your code, without the need for any migrations. You can also modify all the migration operations generated by EF Core before generating the commands.

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

## Versioning

The versioning of the package follows the versioning of EntityFrameworkCore package.