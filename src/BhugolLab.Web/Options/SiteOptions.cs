namespace BhugolLab.Web.Options;

/// <summary>
/// Non-secret site settings, bound from the <c>Site</c> configuration section.
/// Every value is documented in the README settings table.
/// </summary>
public sealed class SiteOptions
{
    public const string SectionName = "Site";

    /// <summary>Canonical base URL, e.g. <c>https://bhugol.dev</c>. Used for canonical links, OG tags and the sitemap.</summary>
    public string BaseUrl { get; set; } = "https://bhugol.dev";

    /// <summary>Display name of the consultant, used in the footer, hero and JSON-LD.</summary>
    public string AuthorName { get; set; } = string.Empty;

    /// <summary>Public GitHub profile URL.</summary>
    public string GitHubUrl { get; set; } = string.Empty;

    /// <summary>Public LinkedIn profile URL.</summary>
    public string LinkedInUrl { get; set; } = string.Empty;

    /// <summary>Booking link (Cal.com / Calendly) used by the "Book a call" CTAs.</summary>
    public string BookingUrl { get; set; } = string.Empty;

    /// <summary>City and timezone the consultant works from, shown in the footer and about section.</summary>
    public string Location { get; set; } = string.Empty;
}
