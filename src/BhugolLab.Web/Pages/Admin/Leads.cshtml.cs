using BhugolLab.Web.Data;
using BhugolLab.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BhugolLab.Web.Pages.Admin;

[Authorize]
public class LeadsModel(ILeadStore store) : PageModel
{
    public IReadOnlyList<Lead> Leads { get; private set; } = [];

    public IReadOnlyList<string> Statuses { get; } = LeadStatus.All;

    public async Task OnGetAsync() => Leads = await store.ListAsync();

    public async Task<IActionResult> OnPostStatusAsync(Guid id, string status)
    {
        if (!LeadStatus.All.Contains(status, StringComparer.Ordinal))
        {
            return BadRequest();
        }

        var updated = await store.UpdateStatusAsync(id, status);
        return updated ? RedirectToPage("/Admin/Leads") : NotFound();
    }
}
