using BhugolLab.Web.Data.Entities;
using BhugolLab.Web.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BhugolLab.Web.Pages.Admin;

[Authorize]
public class LeadsModel(ILeadRepository leads) : PageModel
{
    public IReadOnlyList<Lead> Leads { get; private set; } = [];

    public IReadOnlyList<string> Statuses { get; } = LeadStatus.All;

    public async Task OnGetAsync() => Leads = await leads.ListAsync();

    public async Task<IActionResult> OnPostStatusAsync(Guid id, string status)
    {
        if (!LeadStatus.All.Contains(status, StringComparer.Ordinal))
        {
            return BadRequest();
        }

        var updated = await leads.UpdateStatusAsync(id, status);
        return updated ? RedirectToPage("/Admin/Leads") : NotFound();
    }
}
