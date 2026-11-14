using BhugolLab.Web.Models;
using BhugolLab.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BhugolLab.Web.Pages.Blog;

public class TagModel : PageModel
{
    private readonly MarkdownService _markdown;

    public TagModel(MarkdownService markdown) => _markdown = markdown;

    public string Tag { get; private set; } = string.Empty;

    public IReadOnlyList<ContentItem> Items { get; private set; } = [];

    public void OnGet(string tag)
    {
        Tag = tag;
        Items = _markdown.LoadPublished("posts")
            .Where(item => item.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            .ToList();
        ViewData["Title"] = $"posts tagged {tag}";
        ViewData["Description"] = $"All Bhugol Lab blog posts tagged “{tag}”.";
    }
}
