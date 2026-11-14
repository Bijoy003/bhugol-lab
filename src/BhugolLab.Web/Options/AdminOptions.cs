namespace BhugolLab.Web.Options;

/// <summary>
/// Single admin user for <c>/admin/leads</c>, seeded from configuration.
/// The password hash is a secret — set it via user-secrets or environment variables.
/// Generate one with: <c>dotnet run --project src/BhugolLab.Web -- hash-password &lt;password&gt;</c>
/// </summary>
public sealed class AdminOptions
{
    public const string SectionName = "Admin";

    public string Username { get; set; } = string.Empty;

    /// <summary>PBKDF2 hash in the form <c>v1.{iterations}.{salt}.{key}</c> (base64 parts).</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(PasswordHash);
}
