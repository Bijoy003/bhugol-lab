using System.Text.RegularExpressions;

namespace BhugolLab.Web.Tests;

internal static partial class TestHelpers
{
    /// <summary>Fetches a page and extracts its antiforgery token.</summary>
    public static async Task<string> GetAntiforgeryTokenAsync(HttpClient client, string url)
    {
        var html = await client.GetStringAsync(url);
        var match = AntiforgeryTokenRegex().Match(html);
        Assert.True(match.Success, $"No antiforgery token found on {url}");
        return match.Groups[1].Value;
    }

    /// <summary>Returns the first regex group-1 match, or an empty string.</summary>
    public static string ExtractFirst(string html, string pattern) =>
        Regex.Match(html, pattern).Groups[1].Value;

    [GeneratedRegex("name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"")]
    private static partial Regex AntiforgeryTokenRegex();
}
