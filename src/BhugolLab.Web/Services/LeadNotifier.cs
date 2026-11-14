using BhugolLab.Web.Options;
using Microsoft.Extensions.Options;

namespace BhugolLab.Web.Services;

/// <summary>
/// Background worker that sends lead notification emails off the request thread,
/// bounded by a per-send timeout. Failures are logged; the lead is already persisted.
/// </summary>
public sealed class LeadNotifier(
    LeadNotificationQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<LeadNotifier> logger) : BackgroundService
{
    private static readonly TimeSpan SendBudget = TimeSpan.FromSeconds(20);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var leadId in queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await NotifyAsync(leadId, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lead notification failed {LeadId}", leadId);
            }
        }
    }

    private async Task NotifyAsync(Guid leadId, CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<ILeadStore>();
        var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<EmailOptions>>().Value;

        if (string.IsNullOrWhiteSpace(options.ToAddress))
        {
            logger.LogWarning("Lead {LeadId} not notified: Email:ToAddress is not configured", leadId);
            return;
        }

        var lead = await store.FindAsync(leadId, stoppingToken);
        if (lead is null)
        {
            logger.LogWarning("Lead {LeadId} not found for notification", leadId);
            return;
        }

        var body = $"""
            New lead via bhugol.dev contact form

            Name:    {lead.Name}
            Email:   {lead.Email}
            Company: {lead.Company ?? "-"}
            Source:  {lead.Source ?? "-"}
            Referrer:{lead.Referrer ?? "-"}
            Received:{lead.CreatedAt:yyyy-MM-dd HH:mm} UTC

            Message:
            {lead.Message}
            """;

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        timeout.CancelAfter(SendBudget);

        await sender.SendAsync(options.ToAddress, $"New lead: {lead.Name}", body, timeout.Token);
        logger.LogInformation("Lead notification sent {LeadId}", leadId);
    }
}
