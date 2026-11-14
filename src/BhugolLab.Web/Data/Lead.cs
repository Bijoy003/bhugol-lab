namespace BhugolLab.Web.Data;

/// <summary>A contact-form submission. The only table in the database.</summary>
public class Lead
{
    /// <summary>Application-generated key so the same migrations apply on SQL Server and SQLite.</summary>
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Company { get; set; }

    public string Message { get; set; } = string.Empty;

    /// <summary>Combined UTM parameters of the visit, e.g. <c>utm_source=newsletter&amp;utm_campaign=spring</c>.</summary>
    public string? Source { get; set; }

    public string? Referrer { get; set; }

    /// <summary>UTC timestamp of the submission.</summary>
    public DateTime CreatedAt { get; set; }

    public string Status { get; set; } = LeadStatus.New;
}

public static class LeadStatus
{
    public const string New = "New";
    public const string Contacted = "Contacted";
    public const string Qualified = "Qualified";
    public const string Won = "Won";
    public const string Lost = "Lost";
    public const string Spam = "Spam";

    public static readonly string[] All = [New, Contacted, Qualified, Won, Lost, Spam];
}
