namespace PisitBlog.Domain;

public record PostMetadata
{
    public required string Title { get; init; }
    public required string Slug { get; init; }
    public required DateTimeOffset Date { get; init; }
    public string[] Tags { get; init; } = [];
    public bool Draft { get; init; }
    public string Summary { get; init; } = string.Empty;
    public string CoverImage { get; init; } = string.Empty;
}

public record PostContent(
    PostMetadata Metadata,
    string HtmlContent,
    TableOfContentsItem[] Toc
);

public record TableOfContentsItem(
    int Level,
    string Title,
    string Id
);

public record PageResponse<T>(
    int Page,
    int TotalPages,
    T[] Items
);

public record TagResponse(
    string Tag,
    PostMetadata[] Posts
);
