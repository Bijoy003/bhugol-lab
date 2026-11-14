namespace BhugolLab.Web.Services;

/// <summary>
/// Sends notification email. Implementations: SMTP and a no-op logger. Switching providers
/// (e.g. to Resend or Brevo later) is a configuration and implementation change only —
/// callers depend on this interface.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
