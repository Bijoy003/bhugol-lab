using BhugolLab.Web.Middleware;
using BhugolLab.Web.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
});

builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection(SiteOptions.SectionName));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));

// Do not advertise the server technology.
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

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

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthorization();

app.MapGet("/healthz", () => "OK");

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

// Exposed for WebApplicationFactory<Program> in the test project.
public partial class Program
{
}
