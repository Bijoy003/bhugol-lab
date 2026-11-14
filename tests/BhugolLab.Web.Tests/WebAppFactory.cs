using BhugolLab.Web.Infrastructure.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BhugolLab.Web.Tests;

/// <summary>
/// Boots the real application against a throw-away SQLite file with the no-op email
/// sender and a configured admin user. Each test class gets its own factory, so
/// database and rate-limiter state never leak between classes.
/// </summary>
public sealed class WebAppFactory : WebApplicationFactory<Program>
{
    public const string AdminUsername = "admin";
    public const string AdminPassword = "test-password-123";

    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"bhugollab-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={_dbPath}");
        builder.UseSetting("Email:Provider", "noop");
        builder.UseSetting("Email:FromAddress", "site@example.com");
        builder.UseSetting("Email:ToAddress", "leads@example.com");
        builder.UseSetting("Admin:Username", AdminUsername);
        builder.UseSetting("Admin:PasswordHash", PasswordHasher.Hash(AdminPassword));
        builder.UseSetting("Site:BaseUrl", "https://test.example");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        foreach (var suffix in new[] { string.Empty, "-shm", "-wal" })
        {
            try
            {
                File.Delete(_dbPath + suffix);
            }
            catch (IOException)
            {
                // Best effort cleanup.
            }
        }
    }
}
