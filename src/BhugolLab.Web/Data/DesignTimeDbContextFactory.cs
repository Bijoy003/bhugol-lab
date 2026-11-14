using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BhugolLab.Web.Data;

/// <summary>
/// Pins design-time tooling (<c>dotnet ef migrations add</c>) to SQL Server so the committed
/// migrations always carry production column types. The connection string is never opened —
/// only the model is needed. At runtime the provider is chosen from configuration instead.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=localhost;Database=BhugolLab-DesignTimeOnly;Trusted_Connection=True;")
            .Options);
}
