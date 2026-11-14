using BhugolLab.Web.Models;
using BhugolLab.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BhugolLab.Web.Pages.Work;

public class DetailModel : PageModel
{
    private readonly MarkdownService _markdown;

    public DetailModel(MarkdownService markdown) => _markdown = markdown;

    public ContentItem Item { get; private set; } = null!;

    public IActionResult OnGet(string slug)
    {
        var item = _markdown.FindPublished("work", slug);
        if (item is null)
        {
            return NotFound();
        }

        Item = item;
        ViewData["Title"] = item.Title;
        ViewData["Description"] = item.Description;
        ViewData["OgType"] = "article";
        return Page();
    }
}
