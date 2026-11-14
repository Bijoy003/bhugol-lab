namespace BhugolLab.Web.Models;

/// <summary>A rendered Markdown document (blog post or case study) with its front matter.</summary>
public sealed class ContentItem
{
    public required string Slug { get; init; }

    public required string Title { get; init; }

    public string Description { get; init; } = string.Empty;

    public DateOnly Date { get; init; }

    public IReadOnlyList<string> Tags { get; init; } = [];

    public bool Draft { get; init; }

    /// <summary>Rendered HTML body.</summary>
    public string Html { get; init; } = string.Empty;

    /// <summary>Headings (levels 2–3) with the anchor ids Markdig will emit.</summary>
    public IReadOnlyList<TocEntry> Toc { get; init; } = [];
}

public sealed record TocEntry(int Level, string Id, string Text);
