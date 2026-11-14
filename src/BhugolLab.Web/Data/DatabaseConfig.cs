namespace BhugolLab.Web.Data;

/// <summary>
/// Chooses the database provider from <c>ConnectionStrings:Default</c>:
/// empty → SQLite file under <c>App_Data</c> (local dev, zero setup);
/// contains "Data Source" → SQLite with that connection string;
/// anything else → SQL Server (including Azure AD auth connection strings).
/// </summary>
public static class DatabaseConfig
{
    public static bool IsSqlite(string? connectionString) =>
        string.IsNullOrWhiteSpace(connectionString)
        || connectionString.Contains("Data Source", StringComparison.OrdinalIgnoreCase);

    public static string ResolveSqliteConnectionString(string? connectionString, string contentRootPath)
    {
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        var directory = Path.Combine(contentRootPath, "App_Data");
        Directory.CreateDirectory(directory);
        return $"Data Source={Path.Combine(directory, "bhugollab.db")}";
    }
}
