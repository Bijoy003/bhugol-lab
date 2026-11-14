namespace BhugolLab.Web.Options;

/// <summary>
/// Email settings, bound from the <c>Email</c> configuration section.
/// Credentials come from user-secrets or environment variables, never from committed files.
/// </summary>
public sealed class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary><c>noop</c> (log only, default for local dev) or <c>smtp</c>.</summary>
    public string Provider { get; set; } = "noop";

    public SmtpOptions Smtp { get; set; } = new();

    /// <summary>Sender address for lead notifications.</summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>Recipient of lead notifications; also the public contact address.</summary>
    public string ToAddress { get; set; } = string.Empty;
}

public sealed class SmtpOptions
{
    public string Host { get; set; } = string.Empty;

    /// <summary>587 with STARTTLS is the usual default.</summary>
    public int Port { get; set; } = 587;

    public string Username { get; set; } = string.Empty;

    /// <summary>Secret — set via user-secrets or environment variable.</summary>
    public string Password { get; set; } = string.Empty;
}
