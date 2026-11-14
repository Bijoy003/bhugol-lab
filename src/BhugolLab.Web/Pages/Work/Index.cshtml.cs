using BhugolLab.Web.Models;
using BhugolLab.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BhugolLab.Web.Pages.Work;

public class IndexModel : PageModel
{
    private readonly MarkdownService _markdown;

    public IndexModel(MarkdownService markdown) => _markdown = markdown;

    public IReadOnlyList<ContentItem> Items { get; private set; } = [];

    public void OnGet() => Items = _markdown.LoadPublished("work");
}
