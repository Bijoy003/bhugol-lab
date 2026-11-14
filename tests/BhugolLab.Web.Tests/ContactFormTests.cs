using BhugolLab.Web.Data.Entities;
using BhugolLab.Web.Interfaces;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BhugolLab.Web.Tests;

public class ContactFormTests(WebAppFactory factory) : IClassFixture<WebAppFactory>
{
    private readonly WebAppFactory _factory = factory;
    private readonly HttpClient _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [Fact]
    public async Task Invalid_submission_returns_validation_errors()
    {
        var token = await TestHelpers.GetAntiforgeryTokenAsync(_client, "/contact");
        var response = await _client.PostAsync("/contact", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Name"] = string.Empty,
            ["Email"] = "not-an-email",
            ["Message"] = "short",
            ["__RequestVerificationToken"] = token,
        }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("field-validation-error", html);
        Assert.DoesNotContain(await LeadsAsync(), lead => lead.Email == "not-an-email");
    }

    [Fact]
    public async Task Valid_submission_persists_lead_and_redirects_to_thanks()
    {
        var token = await TestHelpers.GetAntiforgeryTokenAsync(_client, "/contact?utm_source=newsletter&utm_campaign=fall");
        var response = await _client.PostAsync("/contact", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Name"] = "Test Person",
            ["Email"] = "test@example.com",
            ["Company"] = "ACME",
            ["Message"] = "We need help migrating a legacy service.",
            ["UtmSource"] = "newsletter",
            ["UtmCampaign"] = "fall",
            ["Referrer"] = "https://referrer.example/",
            ["__RequestVerificationToken"] = token,
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/contact/thanks", response.Headers.Location?.OriginalString);

        var thanks = await _client.GetAsync("/contact/thanks");
        Assert.Equal(HttpStatusCode.OK, thanks.StatusCode);

        var leads = await LeadsAsync();
        var lead = Assert.Single(leads);
        Assert.Equal("Test Person", lead.Name);
        Assert.Equal("test@example.com", lead.Email);
        Assert.Equal("ACME", lead.Company);
        Assert.Equal("utm_source=newsletter&utm_campaign=fall", lead.Source);
        Assert.Equal("https://referrer.example/", lead.Referrer);
        Assert.Equal(LeadStatus.New, lead.Status);
    }

    [Fact]
    public async Task Honeypot_submission_is_discarded_silently()
    {
        var token = await TestHelpers.GetAntiforgeryTokenAsync(_client, "/contact");
        var response = await _client.PostAsync("/contact", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Name"] = "Bot",
            ["Email"] = "bot@example.com",
            ["Message"] = "Spam spam spam spam spam spam.",
            ["Website"] = "http://spam.example",
            ["__RequestVerificationToken"] = token,
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.DoesNotContain(await LeadsAsync(), lead => lead.Email == "bot@example.com");
    }

    private async Task<IReadOnlyList<Lead>> LeadsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILeadRepository>();
        return await repository.ListAsync();
    }
}
