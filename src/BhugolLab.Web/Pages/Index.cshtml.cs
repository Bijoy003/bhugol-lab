using BhugolLab.Web.Models;
using BhugolLab.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BhugolLab.Web.Pages;

public class IndexModel : PageModel
{
    private readonly MarkdownService _markdown;

    public IndexModel(MarkdownService markdown) => _markdown = markdown;

    /// <summary>Up to three newest published case studies, shown as proof points.</summary>
    public IReadOnlyList<ContentItem> CaseStudies { get; private set; } = [];

    public void OnGet() => CaseStudies = _markdown.LoadPublished("work").Take(3).ToList();
}
