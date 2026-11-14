using System.Xml.Linq;
using BhugolLab.Web.Options;
using BhugolLab.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace BhugolLab.Web.Pages;

public class SitemapModel(MarkdownService markdown, IOptions<SiteOptions> site) : PageModel
{
    private static readonly XNamespace SitemapNs = "http://www.sitemaps.org/schemas/sitemap/0.9";

    public string Xml { get; private set; } = string.Empty;

    public void OnGet()
    {
        var baseUrl = site.Value.BaseUrl.TrimEnd('/');
        var posts = markdown.LoadPublished("posts");
        var work = markdown.LoadPublished("work");

        var urls = new List<(string Loc, DateOnly? LastMod)>
        {
            (baseUrl + "/", null),
            (baseUrl + "/hire", null),
            (baseUrl + "/services", null),
            (baseUrl + "/work", null),
            (baseUrl + "/blog", null),
            (baseUrl + "/contact", null),
            (baseUrl + "/privacy", null),
        };

        urls.AddRange(work.Select(item => ($"{baseUrl}/work/{item.Slug}", (DateOnly?)item.Date)));
        urls.AddRange(posts.Select(item => ($"{baseUrl}/blog/{item.Slug}", (DateOnly?)item.Date)));
        urls.AddRange(posts
            .SelectMany(item => item.Tags)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(tag => ($"{baseUrl}/blog/tag/{tag}", (DateOnly?)null)));

        var urlset = new XElement(SitemapNs + "urlset",
            urls.Select(url =>
            {
                var element = new XElement(SitemapNs + "url", new XElement(SitemapNs + "loc", url.Loc));
                if (url.LastMod is { } lastMod)
                {
                    element.Add(new XElement(SitemapNs + "lastmod", lastMod.ToString("yyyy-MM-dd")));
                }

                return element;
            }));

        Xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + new XDocument(urlset);
    }
}
