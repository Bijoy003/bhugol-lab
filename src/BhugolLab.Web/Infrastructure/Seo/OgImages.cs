namespace BhugolLab.Web.Infrastructure.Seo;

/// <summary>
/// Per-content social images live in <c>wwwroot/img/og/{folder}/{slug}.png</c>.
/// Pages that have one reference it; everything else falls back to the default image.
/// </summary>
public static class OgImages
{
    public static string? For(IWebHostEnvironment environment, string folder, string slug)
    {
        var relative = $"img/og/{folder}/{slug}.png";
        return File.Exists(Path.Combine(environment.WebRootPath, relative)) ? "/" + relative : null;
    }
}
