using Microsoft.AspNetCore.Mvc.Testing;
namespace BhugolLab.Web.Tests;

public class PageSmokeTests(WebAppFactory factory) : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [Theory]
    [InlineData("/")]
    [InlineData("/hire")]
    [InlineData("/services")]
    [InlineData("/work")]
    [InlineData("/work/legacy-modernization")]
    [InlineData("/blog")]
    [InlineData("/blog/why-razor-pages-for-a-marketing-site")]
    [InlineData("/blog/tag/dotnet")]
    [InlineData("/privacy")]
    [InlineData("/contact")]
    [InlineData("/healthz")]
    [InlineData("/robots.txt")]
    [InlineData("/rss.xml")]
    public async Task Public_pages_return_200(string url)
    {
        var response = await _client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Homepage_contains_brand_and_headline()
    {
        var html = await _client.GetStringAsync("/");
        Assert.Contains("Bhugol Lab", html);
        Assert.Contains("<h1", html);
    }

    [Fact]
    public async Task Unknown_path_returns_404_page()
    {
        var response = await _client.GetAsync("/definitely-not-a-page");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Page not found", html);
    }

    [Fact]
    public async Task Robots_disallows_admin()
    {
        var robots = await _client.GetStringAsync("/robots.txt");
        Assert.Contains("Disallow: /admin", robots);
    }

    [Fact]
    public async Task Rss_feed_is_rss_xml()
    {
        var response = await _client.GetAsync("/rss.xml");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("rss+xml", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("<rss", body);
        Assert.Contains("why-razor-pages-for-a-marketing-site", body);
    }

    [Fact]
    public async Task Sitemap_lists_routes_and_content_with_lastmod()
    {
        var response = await _client.GetAsync("/sitemap.xml");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("<urlset", body);
        Assert.Contains("https://test.example/", body);
        Assert.Contains("https://test.example/blog/why-razor-pages-for-a-marketing-site", body);
        Assert.Contains("https://test.example/work/legacy-modernization", body);
        Assert.Contains("<lastmod>", body);
        Assert.DoesNotContain("/admin", body);
    }

    [Fact]
    public async Task Homepage_carries_person_json_ld()
    {
        var html = await _client.GetStringAsync("/");
        Assert.Contains("application/ld+json", html);
        Assert.Contains("\"@type\":\"Person\"", html);
    }

    [Fact]
    public async Task Blog_post_carries_article_json_ld_and_og_image()
    {
        var html = await _client.GetStringAsync("/blog/output-caching-gotchas");
        Assert.Contains("\"@type\":\"Article\"", html);
        Assert.Contains("\"@type\":\"BreadcrumbList\"", html);
        Assert.Contains("/img/og/posts/output-caching-gotchas.png", html);
    }

    [Fact]
    public async Task Security_headers_are_present()
    {
        var response = await _client.GetAsync("/");
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Contains("default-src 'self'", response.Headers.GetValues("Content-Security-Policy").Single());
        Assert.False(response.Headers.Contains("Server"));
    }
}
