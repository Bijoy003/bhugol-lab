using BhugolLab.Web.Models;
using BhugolLab.Web.Options;
using BhugolLab.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace BhugolLab.Web.Pages.Blog;

public class PostModel(
    MarkdownService markdown,
    IWebHostEnvironment environment,
    IOptions<SiteOptions> siteOptions) : PageModel
{
    private readonly SiteOptions _site = siteOptions.Value;

    public ContentItem Item { get; private set; } = null!;

    public IActionResult OnGet(string slug)
    {
        var item = markdown.FindPublished("posts", slug);
        if (item is null)
        {
            return NotFound();
        }

        Item = item;
        ViewData["Title"] = item.Title;
        ViewData["Description"] = item.Description;
        ViewData["OgType"] = "article";

        var baseUrl = _site.BaseUrl.TrimEnd('/');
        var url = $"{baseUrl}/blog/{item.Slug}";
        var ogImage = OgImages.For(environment, "posts", item.Slug) ?? "/img/og/default.png";
        ViewData["OgImage"] = ogImage;

        ViewData["JsonLd"] = StructuredData.Combine(
            StructuredData.Article(item.Title, item.Description, url, baseUrl + ogImage, item.Date, _site.AuthorName, baseUrl + "/"),
            StructuredData.BreadcrumbList(
                ("Home", baseUrl + "/"),
                ("Blog", baseUrl + "/blog"),
                (item.Title, url)));
        return Page();
    }
}
