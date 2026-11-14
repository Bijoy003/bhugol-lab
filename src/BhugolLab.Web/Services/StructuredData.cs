using System.Text.Json;

namespace BhugolLab.Web.Services;

/// <summary>Builds schema.org JSON-LD graphs. Serialized with System.Text.Json, which escapes
/// unsafe characters, so the output is safe to embed in a script block.</summary>
public static class StructuredData
{
    public static string Person(string name, string url, IReadOnlyList<string> sameAs) =>
        Serialize(new Dictionary<string, object>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "Person",
            ["name"] = name,
            ["url"] = url,
            ["sameAs"] = sameAs,
            ["worksFor"] = Organization("Bhugol Lab", url),
        });

    public static string ProfessionalService(string name, string url, string description, IReadOnlyList<string> sameAs) =>
        Serialize(new Dictionary<string, object>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "ProfessionalService",
            ["name"] = name,
            ["url"] = url,
            ["description"] = description,
            ["sameAs"] = sameAs,
        });

    public static string Article(
        string headline, string description, string url, string imageUrl, DateOnly date, string authorName, string authorUrl) =>
        Serialize(new Dictionary<string, object>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "Article",
            ["headline"] = headline,
            ["description"] = description,
            ["url"] = url,
            ["image"] = imageUrl,
            ["datePublished"] = date.ToString("yyyy-MM-dd"),
            ["dateModified"] = date.ToString("yyyy-MM-dd"),
            ["author"] = new Dictionary<string, object>
            {
                ["@type"] = "Person",
                ["name"] = authorName,
                ["url"] = authorUrl,
            },
        });

    public static string BreadcrumbList(params (string Name, string Url)[] crumbs) =>
        Serialize(new Dictionary<string, object>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "BreadcrumbList",
            ["itemListElement"] = crumbs.Select((crumb, index) => new Dictionary<string, object>
            {
                ["@type"] = "ListItem",
                ["position"] = index + 1,
                ["name"] = crumb.Name,
                ["item"] = crumb.Url,
            }).ToList(),
        });

    /// <summary>Combines several graphs into one JSON-LD array for a single script block.</summary>
    public static string Combine(params string[] graphs) =>
        graphs.Length == 1 ? graphs[0] : "[" + string.Join(",", graphs) + "]";

    private static Dictionary<string, object> Organization(string name, string url) =>
        new()
        {
            ["@type"] = "Organization",
            ["name"] = name,
            ["url"] = url,
        };

    private static string Serialize(object graph) => JsonSerializer.Serialize(graph);
}
