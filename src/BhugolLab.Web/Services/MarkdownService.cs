using BhugolLab.Web.Models;
using Markdig;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace BhugolLab.Web.Services;

/// <summary>
/// Loads Markdown documents with YAML front matter from <c>Content/{folder}</c> and renders
/// them to HTML with Markdig. Content lives in git; nothing here touches the database.
/// </summary>
public sealed class MarkdownService
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    private static readonly IDeserializer FrontMatterDeserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<MarkdownService> _logger;

    public MarkdownService(IWebHostEnvironment environment, ILogger<MarkdownService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    /// <summary>All published (non-draft) items in a folder, newest first.</summary>
    public IReadOnlyList<ContentItem> LoadPublished(string folder)
    {
        var items = LoadAll(folder).Where(item => !item.Draft).ToList();
        items.Sort((a, b) => b.Date.CompareTo(a.Date));
        return items;
    }

    /// <summary>A single published item, or null when missing or still a draft.</summary>
    public ContentItem? FindPublished(string folder, string slug) =>
        LoadPublished(folder).FirstOrDefault(item =>
            string.Equals(item.Slug, slug, StringComparison.OrdinalIgnoreCase));

    private IReadOnlyList<ContentItem> LoadAll(string folder)
    {
        var directory = Path.Combine(_environment.ContentRootPath, "Content", folder);
        if (!Directory.Exists(directory))
        {
            return [];
        }

        var items = new List<ContentItem>();
        foreach (var path in Directory.EnumerateFiles(directory, "*.md", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var item = Parse(Path.GetFileNameWithoutExtension(path), File.ReadAllText(path));
                items.Add(item);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or YamlDotNet.Core.YamlException)
            {
                _logger.LogError(ex, "Skipping unreadable Markdown file {Path}", path);
            }
        }

        return items;
    }

    internal ContentItem Parse(string slug, string markdown)
    {
        var frontMatter = new FrontMatter();
        var body = markdown;

        var (yaml, rest) = SplitFrontMatter(markdown);
        if (yaml is not null)
        {
            frontMatter = FrontMatterDeserializer.Deserialize<FrontMatter>(yaml) ?? new FrontMatter();
            body = rest;
        }
        else
        {
            _logger.LogWarning("Markdown document {Slug} has no YAML front matter", slug);
        }

        var document = Markdown.Parse(body, Pipeline);
        var html = document.ToHtml(Pipeline);

        return new ContentItem
        {
            Slug = slug,
            Title = string.IsNullOrWhiteSpace(frontMatter.Title) ? slug : frontMatter.Title,
            Description = frontMatter.Description ?? string.Empty,
            Date = frontMatter.Date is null ? DateOnly.MinValue : DateOnly.FromDateTime(frontMatter.Date.Value),
            Tags = frontMatter.Tags ?? [],
            Draft = frontMatter.Draft,
            Html = html,
            Toc = BuildToc(document),
        };
    }

    private static (string? Yaml, string Body) SplitFrontMatter(string markdown)
    {
        // Front matter must start on the very first line: "---" ... "---"
        var normalized = markdown.Replace("\r\n", "\n").Replace('\r', '\n');
        if (!normalized.StartsWith("---\n", StringComparison.Ordinal))
        {
            return (null, markdown);
        }

        var end = normalized.IndexOf("\n---\n", 4, StringComparison.Ordinal);
        if (end < 0)
        {
            return (null, markdown);
        }

        var yaml = normalized[4..end];
        var body = normalized[(end + 5)..];
        return (yaml, body);
    }

    /// <summary>
    /// Collects level 2–3 headings. The anchor ids come from Markdig's auto-identifier
    /// extension, so they match exactly what the renderer emits.
    /// </summary>
    private static IReadOnlyList<TocEntry> BuildToc(MarkdownDocument document)
    {
        var entries = new List<TocEntry>();
        foreach (var heading in document.Descendants().OfType<HeadingBlock>())
        {
            if (heading.Level is < 2 or > 3)
            {
                continue;
            }

            var id = heading.GetAttributes().Id;
            if (string.IsNullOrEmpty(id))
            {
                continue;
            }

            entries.Add(new TocEntry(heading.Level, id, ExtractText(heading.Inline)));
        }

        return entries;
    }

    private static string ExtractText(ContainerInline? inline)
    {
        if (inline is null)
        {
            return string.Empty;
        }

        var builder = new System.Text.StringBuilder();
        Append(inline);
        return builder.ToString().Trim();

        void Append(Inline node)
        {
            switch (node)
            {
                case LiteralInline literal:
                    builder.Append(literal.Content.ToString());
                    break;
                case CodeInline code:
                    builder.Append(code.Content);
                    break;
                case ContainerInline container:
                    foreach (var child in container)
                    {
                        Append(child);
                    }

                    break;
            }
        }
    }

    private sealed class FrontMatter
    {
        public string? Title { get; set; }

        public string? Description { get; set; }

        public DateTime? Date { get; set; }

        public List<string>? Tags { get; set; }

        public bool Draft { get; set; }
    }
}
