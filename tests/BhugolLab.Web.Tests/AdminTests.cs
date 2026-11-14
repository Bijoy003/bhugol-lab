using Microsoft.AspNetCore.Mvc.Testing;
namespace BhugolLab.Web.Tests;

public class AdminTests(WebAppFactory factory) : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient(NoRedirect);
    private static WebApplicationFactoryClientOptions NoRedirect => new() { AllowAutoRedirect = false };

    [Fact]
    public async Task Leads_page_requires_authentication()
    {
        var response = await _client.GetAsync("/admin/leads");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/admin/login", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Wrong_password_is_rejected()
    {
        var token = await TestHelpers.GetAntiforgeryTokenAsync(_client, "/admin/login");
        var response = await _client.PostAsync("/admin/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["username"] = WebAppFactory.AdminUsername,
            ["password"] = "wrong-password",
            ["__RequestVerificationToken"] = token,
        }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid username or password.", html);
    }

    [Fact]
    public async Task Valid_credentials_sign_in_and_logout_signs_out()
    {
        var token = await TestHelpers.GetAntiforgeryTokenAsync(_client, "/admin/login");
        var login = await _client.PostAsync("/admin/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["username"] = WebAppFactory.AdminUsername,
            ["password"] = WebAppFactory.AdminPassword,
            ["__RequestVerificationToken"] = token,
        }));

        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        Assert.Contains("/admin/leads", login.Headers.Location?.OriginalString);

        var leads = await _client.GetAsync("/admin/leads");
        Assert.Equal(HttpStatusCode.OK, leads.StatusCode);
        var html = await leads.Content.ReadAsStringAsync();
        Assert.Contains("Leads", html);

        var logoutToken = TestHelpers.ExtractFirst(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        var logout = await _client.PostAsync("/admin/logout", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = logoutToken,
        }));
        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);

        var afterLogout = await _client.GetAsync("/admin/leads");
        Assert.Equal(HttpStatusCode.Redirect, afterLogout.StatusCode);
    }
}
