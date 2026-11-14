using BhugolLab.Web.Infrastructure.Seo;
using BhugolLab.Web.Models;
using BhugolLab.Web.Options;
using BhugolLab.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace BhugolLab.Web.Pages;

public class IndexModel(MarkdownService markdown, IOptions<SiteOptions> siteOptions) : PageModel
{
    private readonly SiteOptions _site = siteOptions.Value;

    /// <summary>Up to three newest published case studies, shown as proof points.</summary>
    public IReadOnlyList<ContentItem> CaseStudies { get; private set; } = [];

    public void OnGet()
    {
        CaseStudies = markdown.LoadPublished("work").Take(3).ToList();

        var sameAs = new List<string>();
        if (!SiteOptions.IsPlaceholder(_site.GitHubUrl))
        {
            sameAs.Add(_site.GitHubUrl);
        }

        if (!SiteOptions.IsPlaceholder(_site.LinkedInUrl))
        {
            sameAs.Add(_site.LinkedInUrl);
        }

        ViewData["JsonLd"] = StructuredData.Person(_site.AuthorName, _site.BaseUrl.TrimEnd('/') + "/", sameAs);
    }
}
