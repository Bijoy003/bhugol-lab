namespace BhugolLab.Web.Data.Entities;

/// <summary>The lifecycle states a lead can be in.</summary>
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
