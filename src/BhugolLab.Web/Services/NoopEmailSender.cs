namespace BhugolLab.Web.Services;

/// <summary>
/// Default for local development and tests: logs instead of sending.
/// The contact form succeeds and leads are persisted regardless of email state.
/// </summary>
public sealed class NoopEmailSender(ILogger<NoopEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Email suppressed (noop provider): to {To}, subject {Subject}", to, subject);
        return Task.CompletedTask;
    }
}
