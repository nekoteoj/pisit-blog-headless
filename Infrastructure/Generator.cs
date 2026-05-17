using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using PisitBlog.Application;
using PisitBlog.Configuration;
using PisitBlog.Domain;

namespace PisitBlog.Infrastructure;

public class Generator
{
    private readonly BlogConfiguration _config;
    private readonly ContentProcessor _contentProcessor;
    private readonly ImageProcessor _imageProcessor;
    private readonly JsonSerializerOptions _jsonOptions;

    public Generator(BlogConfiguration config)
    {
        _config = config;
        _contentProcessor = new ContentProcessor();
        _imageProcessor = new ImageProcessor(
            Path.Combine(_config.OutputDirectory, "assets"), 
            _config.MaxImageWidth);
        
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
        
        Directory.CreateDirectory(_config.OutputDirectory);
        Directory.CreateDirectory(Path.Combine(_config.OutputDirectory, "posts", "page"));
        Directory.CreateDirectory(Path.Combine(_config.OutputDirectory, "tags"));
    }

    public async Task GenerateAsync()
    {
        var postDirectories = Directory.EnumerateDirectories(_config.ContentDirectory);
        List<PostContent> allPosts = [];

        foreach (var postDir in postDirectories)
        {
            var markdownFile = Path.Combine(postDir, "index.md");
            if (!File.Exists(markdownFile)) continue;

            var markdown = await File.ReadAllTextAsync(markdownFile);
            
            // Collect relative images first to process them async
            var pipeline = new MarkdownPipelineBuilder().UseYamlFrontMatter().Build();
            var document = Markdown.Parse(markdown, pipeline);
            
            var relativeImages = document.Descendants<LinkInline>()
                .Where(l => l.IsImage && l.Url != null && !l.Url.StartsWith("http"))
                .Select(l => l.Url!)
                .Distinct()
                .ToList();

            Dictionary<string, string> rewrittenMap = [];
            foreach (var relPath in relativeImages)
            {
                rewrittenMap[relPath] = await _imageProcessor.ProcessImageAsync(relPath, postDir);
            }

            var postContent = _contentProcessor.ProcessPost(markdown, (url) => rewrittenMap.GetValueOrDefault(url, url));
            
            if (postContent.Metadata.Draft) continue;

            // Rewrite metadata image path
            if (!string.IsNullOrEmpty(postContent.Metadata.CoverImage) && !postContent.Metadata.CoverImage.StartsWith("http"))
            {
                var rewrittenPath = await _imageProcessor.ProcessImageAsync(postContent.Metadata.CoverImage, postDir);
                postContent = postContent with { Metadata = postContent.Metadata with { CoverImage = rewrittenPath } };
            }

            allPosts.Add(postContent);

            var postJson = JsonSerializer.Serialize(postContent, _jsonOptions);
            await File.WriteAllTextAsync(Path.Combine(_config.OutputDirectory, "posts", $"{postContent.Metadata.Slug}.json"), postJson);
        }

        var sortedPosts = allPosts.OrderByDescending(p => p.Metadata.Date).ToList();

        await GeneratePaginationAsync(sortedPosts);
        await GenerateTagsAsync(sortedPosts);
        await GenerateSearchIndexAsync(sortedPosts);
        await GenerateRssAsync(sortedPosts);
        await GenerateSitemapAsync(sortedPosts);
        
        Console.WriteLine($"Successfully generated {allPosts.Count} posts.");
    }

    private async Task GeneratePaginationAsync(List<PostContent> sortedPosts)
    {
        int totalPages = (int)Math.Ceiling(sortedPosts.Count / (double)_config.PageSize);
        for (int i = 0; i < totalPages; i++)
        {
            var pageItems = sortedPosts.Skip(i * _config.PageSize).Take(_config.PageSize).Select(p => p.Metadata).ToArray();
            var pageResponse = new PageResponse<PostMetadata>(i + 1, totalPages, pageItems);
            var pageJson = JsonSerializer.Serialize(pageResponse, _jsonOptions);
            await File.WriteAllTextAsync(Path.Combine(_config.OutputDirectory, "posts", "page", $"{i + 1}.json"), pageJson);
        }
    }

    private async Task GenerateTagsAsync(List<PostContent> sortedPosts)
    {
        var tags = sortedPosts.SelectMany(p => p.Metadata.Tags).Distinct();
        foreach (var tag in tags)
        {
            var tagPosts = sortedPosts.Where(p => p.Metadata.Tags.Contains(tag)).Select(p => p.Metadata).ToArray();
            var tagJson = JsonSerializer.Serialize(tagPosts, _jsonOptions);
            await File.WriteAllTextAsync(Path.Combine(_config.OutputDirectory, "tags", $"{tag}.json"), tagJson);
        }
    }

    private async Task GenerateSearchIndexAsync(List<PostContent> sortedPosts)
    {
        var searchIndex = sortedPosts.Select(p => new { p.Metadata.Title, p.Metadata.Slug, p.Metadata.Summary, p.Metadata.Date }).ToArray();
        await File.WriteAllTextAsync(Path.Combine(_config.OutputDirectory, "search-index.json"), JsonSerializer.Serialize(searchIndex, _jsonOptions));
    }

    private async Task GenerateRssAsync(List<PostContent> sortedPosts)
    {
        XNamespace content = "http://purl.org/rss/1.0/modules/content/";
        var rss = new XDocument(
            new XElement("rss", 
                new XAttribute("version", "2.0"),
                new XAttribute(XNamespace.Xmlns + "content", content.NamespaceName),
                new XElement("channel",
                    new XElement("title", _config.SiteName),
                    new XElement("link", _config.BaseUrl),
                    new XElement("description", _config.SiteDescription),
                    sortedPosts.Take(20).Select(post => 
                        new XElement("item",
                            new XElement("title", post.Metadata.Title),
                            new XElement("link", $"{_config.BaseUrl}/posts/{post.Metadata.Slug}"),
                            new XElement("description", post.Metadata.Summary),
                            new XElement("pubDate", post.Metadata.Date.ToString("R")),
                            new XElement("guid", $"{_config.BaseUrl}/posts/{post.Metadata.Slug}")
                        )
                    )
                )
            )
        );

        using var stream = new FileStream(Path.Combine(_config.OutputDirectory, "rss.xml"), FileMode.Create);
        using var writer = XmlWriter.Create(stream, new XmlWriterSettings { Async = true, Indent = true });
        await rss.SaveAsync(writer, default);
    }

    private async Task GenerateSitemapAsync(List<PostContent> sortedPosts)
    {
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var sitemap = new XDocument(
            new XElement(ns + "urlset",
                sortedPosts.Select(post =>
                    new XElement(ns + "url",
                        new XElement(ns + "loc", $"{_config.BaseUrl}/posts/{post.Metadata.Slug}"),
                        new XElement(ns + "lastmod", post.Metadata.Date.ToString("yyyy-MM-dd"))
                    )
                )
            )
        );

        using var stream = new FileStream(Path.Combine(_config.OutputDirectory, "sitemap.xml"), FileMode.Create);
        using var writer = XmlWriter.Create(stream, new XmlWriterSettings { Async = true, Indent = true });
        await sitemap.SaveAsync(writer, default);
    }
}
