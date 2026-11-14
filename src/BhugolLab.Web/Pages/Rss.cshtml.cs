using System.Xml.Linq;
using BhugolLab.Web.Options;
using BhugolLab.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace BhugolLab.Web.Pages;

public class RssModel : PageModel
{
    private const int MaxItems = 20;

    private readonly MarkdownService _markdown;
    private readonly SiteOptions _site;

    public RssModel(MarkdownService markdown, IOptions<SiteOptions> site)
    {
        _markdown = markdown;
        _site = site.Value;
    }

    public string Xml { get; private set; } = string.Empty;

    public void OnGet()
    {
        var baseUrl = _site.BaseUrl.TrimEnd('/');
        var posts = _markdown.LoadPublished("posts").Take(MaxItems).ToList();

        var channel = new XElement("channel",
            new XElement("title", "Bhugol Lab"),
            new XElement("link", baseUrl + "/blog"),
            new XElement("description", "Writing from Bhugol Lab on .NET, web architecture and consulting."),
            new XElement("language", "en-us"),
            new XElement("lastBuildDate", DateTimeOffset.UtcNow.ToString("R")),
            new XElement(XNamespace.Get("http://www.w3.org/2005/Atom") + "link",
                new XAttribute("href", baseUrl + "/rss.xml"),
                new XAttribute("rel", "self"),
                new XAttribute("type", "application/rss+xml")));

        foreach (var post in posts)
        {
            var url = $"{baseUrl}/blog/{post.Slug}";
            channel.Add(new XElement("item",
                new XElement("title", post.Title),
                new XElement("link", url),
                new XElement("guid", new XAttribute("isPermaLink", "true"), url),
                new XElement("pubDate", post.Date.ToDateTime(TimeOnly.MinValue).ToString("R")),
                new XElement("description", post.Description)));
        }

        var document = new XDocument(
            new XElement("rss",
                new XAttribute("version", "2.0"),
                new XAttribute(XNamespace.Xmlns + "atom", "http://www.w3.org/2005/Atom"),
                channel));

        Xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + document;
    }
}
