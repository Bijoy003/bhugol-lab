using System.ComponentModel.DataAnnotations;

namespace BhugolLab.Web.Models;

/// <summary>Contact form payload. Server-side validation only; no client-side framework.</summary>
public sealed class ContactInput
{
    [Required(ErrorMessage = "Please enter your name.")]
    [StringLength(100, ErrorMessage = "The name must be at most 100 characters.")]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "That does not look like a valid email address.")]
    [StringLength(254, ErrorMessage = "The email address must be at most 254 characters.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "The company name must be at most 100 characters.")]
    [Display(Name = "Company (optional)")]
    public string? Company { get; set; }

    [Required(ErrorMessage = "Please tell me a little about the project.")]
    [StringLength(4000, MinimumLength = 10, ErrorMessage = "The message must be between 10 and 4000 characters.")]
    [Display(Name = "Message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Honeypot. Hidden from humans via CSS and unreachable for assistive technology;
    /// a submission that fills it is discarded silently (but logged).
    /// </summary>
    [StringLength(100)]
    [Display(Name = "Website")]
    public string? Website { get; set; }

    // Hidden lead-source fields, filled by /js/lead-source.js from the URL and referrer.
    [StringLength(200)]
    public string? UtmSource { get; set; }

    [StringLength(200)]
    public string? UtmMedium { get; set; }

    [StringLength(200)]
    public string? UtmCampaign { get; set; }

    [StringLength(500)]
    public string? Referrer { get; set; }

    /// <summary>Combines the UTM parameters into the lead's Source value.</summary>
    public string? BuildSource()
    {
        var parts = new List<string>(3);
        if (!string.IsNullOrWhiteSpace(UtmSource))
        {
            parts.Add($"utm_source={UtmSource.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(UtmMedium))
        {
            parts.Add($"utm_medium={UtmMedium.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(UtmCampaign))
        {
            parts.Add($"utm_campaign={UtmCampaign.Trim()}");
        }

        return parts.Count == 0 ? null : string.Join("&", parts);
    }
}
