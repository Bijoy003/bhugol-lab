using System.Net;

namespace BhugolLab.Web.Middleware;

/// <summary>
/// The app runs behind Cloudflare. <c>CF-Connecting-IP</c> is set and overwritten at the
/// Cloudflare edge, so — provided the origin only accepts traffic from Cloudflare (a
/// firewall rule, see docs/deployment.md) — it is authoritative for the real client
/// address, which is what rate limiting is partitioned by. The edge's
/// <c>X-Forwarded-Proto</c> fixes scheme detection behind TLS termination.
/// </summary>
public sealed class CloudflareHeadersMiddleware(RequestDelegate next)
{
    public Task Invoke(HttpContext context)
    {
        var clientIp = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
        if (clientIp is not null && IPAddress.TryParse(clientIp, out var address))
        {
            context.Connection.RemoteIpAddress = address;

            var proto = context.Request.Headers["X-Forwarded-Proto"].FirstOrDefault();
            if (string.Equals(proto, "https", StringComparison.OrdinalIgnoreCase))
            {
                context.Request.Scheme = "https";
            }
            else if (string.Equals(proto, "http", StringComparison.OrdinalIgnoreCase))
            {
                context.Request.Scheme = "http";
            }
        }

        return next(context);
    }
}
