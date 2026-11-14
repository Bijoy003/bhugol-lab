using BhugolLab.Web.Data;
using BhugolLab.Web.Models;
using BhugolLab.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace BhugolLab.Web.Pages;

[EnableRateLimiting("contact")]
public class ContactModel(
    ILeadStore leads,
    LeadNotificationQueue queue,
    ILogger<ContactModel> logger) : PageModel
{
    [BindProperty]
    public ContactInput Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!string.IsNullOrWhiteSpace(Input.Website))
        {
            // Bot: pretend success, persist nothing, notify nobody.
            logger.LogInformation("Contact submission discarded by honeypot");
            return RedirectToPage("/ContactThanks");
        }

        // Persist first — a failed or slow notification must never lose a lead.
        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            Name = Input.Name.Trim(),
            Email = Input.Email.Trim(),
            Company = string.IsNullOrWhiteSpace(Input.Company) ? null : Input.Company.Trim(),
            Message = Input.Message.Trim(),
            Source = Input.BuildSource(),
            Referrer = string.IsNullOrWhiteSpace(Input.Referrer) ? null : Input.Referrer.Trim(),
            CreatedAt = DateTime.UtcNow,
            Status = LeadStatus.New,
        };

        await leads.AddAsync(lead);
        logger.LogInformation("Lead received {LeadId}", lead.Id);
        queue.Enqueue(lead.Id);

        return RedirectToPage("/ContactThanks");
    }
}
