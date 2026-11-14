using BhugolLab.Web.Models;
using BhugolLab.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BhugolLab.Web.Pages.Blog;

public class IndexModel : PageModel
{
    private readonly MarkdownService _markdown;

    public IndexModel(MarkdownService markdown) => _markdown = markdown;

    public IReadOnlyList<ContentItem> Items { get; private set; } = [];

    public IReadOnlyList<string> Tags { get; private set; } = [];

    public void OnGet()
    {
        Items = _markdown.LoadPublished("posts");
        Tags = Items.SelectMany(item => item.Tags)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(tag => tag, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
