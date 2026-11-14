using System.Net;
using System.Net.Mail;
using BhugolLab.Web.Interfaces;
using BhugolLab.Web.Options;

namespace BhugolLab.Web.Infrastructure.Email;

/// <summary>
/// Sends via any SMTP provider (host/port/credentials from configuration; 587 + STARTTLS
/// by default). Always bounded by a timeout so a stuck mail server cannot hold a worker.
/// </summary>
public sealed class SmtpEmailSender : IEmailSender
{
    private static readonly TimeSpan SendTimeout = TimeSpan.FromSeconds(15);

    private readonly EmailOptions _options;
    private readonly string _fromAddress;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(EmailOptions options, ILogger<SmtpEmailSender> logger)
    {
        _options = options;
        _fromAddress = options.FromAddress;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(_fromAddress),
            Subject = subject,
            Body = body,
        };
        message.To.Add(to);

        using var client = new SmtpClient(_options.Smtp.Host, _options.Smtp.Port)
        {
            EnableSsl = true,
            Timeout = (int)SendTimeout.TotalMilliseconds,
        };

        if (!string.IsNullOrEmpty(_options.Smtp.Username))
        {
            client.Credentials = new NetworkCredential(_options.Smtp.Username, _options.Smtp.Password);
        }

        try
        {
            await client.SendMailAsync(message, cancellationToken);
        }
        catch (Exception ex) when (ex is SmtpException or InvalidOperationException)
        {
            _logger.LogError(ex, "SMTP send failed to {To} with subject {Subject}", to, subject);
            throw;
        }
    }
}
