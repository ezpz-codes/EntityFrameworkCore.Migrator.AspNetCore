using Microsoft.EntityFrameworkCore.Design.Internal;

namespace EntityFrameworkCore.Migrator.AspNetCore;

#pragma warning disable EF1001 // Internal EF Core API usage.

public class EmptyReporter : IOperationReporter
{
    public void WriteError(string message) { }

    public void WriteInformation(string message) { }

    public void WriteVerbose(string message) { }

    public void WriteWarning(string message) { }
}

#pragma warning restore EF1001 // Internal EF Core API usage.
