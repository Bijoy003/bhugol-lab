using System.Threading.RateLimiting;
using BhugolLab.Web.Data;
using BhugolLab.Web.Middleware;
using BhugolLab.Web.Options;
using BhugolLab.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;

if (args.Length == 2 && string.Equals(args[0], "hash-password", StringComparison.OrdinalIgnoreCase))
{
    // Utility for generating Admin:PasswordHash values: dotnet run -- hash-password <password>
    Console.WriteLine(PasswordHasher.Hash(args[1]));
    return;
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSingleton<MarkdownService>();
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
});

builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection(SiteOptions.SectionName));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));

// Database: SQL Server in every deployment; SQLite file fallback so a clean machine
// with only the SDK installed can run the site.
var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (DatabaseConfig.IsSqlite(connectionString))
    {
        options.UseSqlite(DatabaseConfig.ResolveSqliteConnectionString(
            connectionString, builder.Environment.ContentRootPath));

        // The committed migrations and model snapshot are SQL Server's; on SQLite the
        // provider-computed column types legitimately differ from the snapshot, which
        // trips PendingModelChangesWarning on every start. Forgotten model changes are
        // still caught on SQL Server and by `dotnet ef migrations has-pending-model-changes`.
        options.ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }
    else
    {
        options.UseSqlServer(connectionString, sql =>
            sql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(2), null));
    }
});
builder.Services.AddScoped<ILeadStore, EfLeadStore>();

// Email: provider is configuration only. Unknown values fail fast at startup.
var emailProvider = builder.Configuration[$"{EmailOptions.SectionName}:Provider"];
if (string.Equals(emailProvider, "smtp", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IEmailSender>(sp => new SmtpEmailSender(
        sp.GetRequiredService<IOptions<EmailOptions>>().Value,
        sp.GetRequiredService<ILogger<SmtpEmailSender>>()));
}
else if (string.IsNullOrWhiteSpace(emailProvider)
         || string.Equals(emailProvider, "noop", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IEmailSender, NoopEmailSender>();
}
else
{
    throw new InvalidOperationException(
        $"Unknown {EmailOptions.SectionName}:Provider '{emailProvider}'. Supported values: noop, smtp.");
}

builder.Services.AddSingleton<LeadNotificationQueue>();
builder.Services.AddHostedService<LeadNotifier>();

// Cookie authentication for the single configured admin user.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/admin/login";
        options.AccessDeniedPath = "/admin/login";
        options.Cookie.Name = "bhugol.admin";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
builder.Services.AddAuthorization();
builder.Services.Configure<AdminOptions>(builder.Configuration.GetSection(AdminOptions.SectionName));

// Rate limiting: contact POSTs 3/10min per IP, admin login POSTs 5/10min per IP,
// plus a global per-IP limiter. Partitioned by the (forwarded) client IP.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(ClientId(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 120,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        }));
    options.AddPolicy("contact", context => PostOnlyLimiter(context, 3, TimeSpan.FromMinutes(10)));
    options.AddPolicy("login", context => PostOnlyLimiter(context, 5, TimeSpan.FromMinutes(10)));
});

// Output caching for anonymous GETs of content pages. Form pages are excluded because
// they embed per-visitor antiforgery tokens, which must never be shared.
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(policy => policy
        .Expire(TimeSpan.FromMinutes(5))
        .With(context => IsCacheablePage(context.HttpContext)));
});

// Do not advertise the server technology.
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

// Honor proxy headers from trusted hops (loopback by default; Cloudflare is handled
// by CloudflareHeadersMiddleware below).
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

app.UseMiddleware<SecurityHeadersMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Render the friendly 404 page while keeping the 404 status code.
app.UseStatusCodePagesWithReExecute("/NotFound");

// Forwarded headers before HTTPS redirection, rate limiting and any IP-based logic.
app.UseForwardedHeaders();
app.UseMiddleware<CloudflareHeadersMiddleware>();

app.UseHttpsRedirection();

app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();

// Migrations are applied explicitly; EnsureCreated() is never used.
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

app.MapGet("/healthz", () => "OK");

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets()
   .CacheOutput();

app.Run();

static string ClientId(HttpContext context) =>
    context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

static RateLimitPartition<string> PostOnlyLimiter(HttpContext context, int permitLimit, TimeSpan window) =>
    HttpMethods.IsPost(context.Request.Method)
        ? RateLimitPartition.GetFixedWindowLimiter(ClientId(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = window,
            QueueLimit = 0,
        })
        : RateLimitPartition.GetNoLimiter("get");

static bool IsCacheablePage(HttpContext context) =>
    HttpMethods.IsGet(context.Request.Method)
    && context.User.Identity?.IsAuthenticated != true
    && !context.Request.Path.StartsWithSegments("/admin")
    && !context.Request.Path.StartsWithSegments("/contact")
    && !context.Request.Path.StartsWithSegments("/hire");

// Exposed for WebApplicationFactory<Program> in the test project.
public partial class Program
{
}
