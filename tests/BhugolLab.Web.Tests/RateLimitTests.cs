using Microsoft.AspNetCore.Mvc.Testing;
namespace BhugolLab.Web.Tests;

public class RateLimitTests(WebAppFactory factory) : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [Fact]
    public async Task Contact_post_returns_429_after_three_requests()
    {
        var token = await TestHelpers.GetAntiforgeryTokenAsync(_client, "/contact");

        for (var i = 1; i <= 3; i++)
        {
            var response = await PostContactAsync(token);
            Assert.True(
                response.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.OK,
                $"request {i} should be permitted, got {(int)response.StatusCode}");
        }

        var limited = await PostContactAsync(token);
        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
    }

    [Fact]
    public async Task Login_post_returns_429_after_five_requests()
    {
        var token = await TestHelpers.GetAntiforgeryTokenAsync(_client, "/admin/login");

        for (var i = 1; i <= 5; i++)
        {
            var response = await PostLoginAsync(token);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        var limited = await PostLoginAsync(token);
        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
    }

    private Task<HttpResponseMessage> PostContactAsync(string token) =>
        _client.PostAsync("/contact", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Name"] = "Rate Limit",
            ["Email"] = "rate@example.com",
            ["Message"] = "Checking the limiter, nothing personal.",
            ["__RequestVerificationToken"] = token,
        }));

    private Task<HttpResponseMessage> PostLoginAsync(string token) =>
        _client.PostAsync("/admin/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["username"] = WebAppFactory.AdminUsername,
            ["password"] = "wrong-password",
            ["__RequestVerificationToken"] = token,
        }));
}
