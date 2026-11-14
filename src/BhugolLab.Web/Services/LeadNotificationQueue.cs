using System.Threading.Channels;

namespace BhugolLab.Web.Services;

/// <summary>
/// In-process queue of lead ids awaiting a notification email. The contact request
/// persists the lead first and only enqueues afterwards, so a failed or slow email
/// can never lose a lead.
/// </summary>
public sealed class LeadNotificationQueue
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>();

    public void Enqueue(Guid leadId) => _channel.Writer.TryWrite(leadId);

    public ChannelReader<Guid> Reader => _channel.Reader;
}
