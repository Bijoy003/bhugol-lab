namespace BhugolLab.Web.Middleware;

/// <summary>
/// Adds the static, self-only security headers to every response.
/// The CSP allows no third-party scripts or styles; extend the allow-list only
/// when a third-party embed is actually added. HSTS is handled by <c>UseHsts()</c>.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    internal const string ContentSecurityPolicy =
        "default-src 'self'; img-src 'self' data: https:; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";

    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public Task Invoke(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers.TryAdd("X-Content-Type-Options", "nosniff");
        headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
        headers.TryAdd("Permissions-Policy", "geolocation=(), microphone=(), camera=(), payment=()");
        headers.TryAdd("Content-Security-Policy", ContentSecurityPolicy);
        return _next(context);
    }
}
