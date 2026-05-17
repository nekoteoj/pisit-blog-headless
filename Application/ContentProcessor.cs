using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using PisitBlog.Domain;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PisitBlog.Application;

public class ContentProcessor : IContentProcessor
{
    private readonly MarkdownPipeline _pipeline;
    private readonly IDeserializer _yamlDeserializer;

    public ContentProcessor()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseYamlFrontMatter()
            .UseAdvancedExtensions()
            .Build();

        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }

    public async Task<PostContent?> ProcessPostAsync(string markdownContent, Func<string, Task<string>>? imageRewriter = null)
    {
        var document = Markdown.Parse(markdownContent, _pipeline);
        
        // Extract Frontmatter
        var yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
        if (yamlBlock == null)
        {
            throw new InvalidOperationException("Markdown file is missing YAML frontmatter.");
        }

        var yaml = string.Join('\n', yamlBlock.Lines.Lines.Select(l => l.ToString()));

        var metadata = _yamlDeserializer.Deserialize<PostMetadata>(yaml);

        // Validation
        if (string.IsNullOrWhiteSpace(metadata.Title)) throw new InvalidOperationException("Metadata 'title' is required.");
        if (string.IsNullOrWhiteSpace(metadata.Slug)) throw new InvalidOperationException("Metadata 'slug' is required.");
        if (metadata.Date == default) throw new InvalidOperationException("Metadata 'date' is required.");

        if (metadata.Draft) return null;

        // Rewrite image paths if rewriter is provided
        if (imageRewriter != null)
        {
            foreach (var link in document.Descendants<Markdig.Syntax.Inlines.LinkInline>())
            {
                if (link.IsImage && link.Url != null && !UrlHelper.IsExternalUrl(link.Url))
                {
                    link.Url = await imageRewriter(link.Url);
                }
            }
        }

        // Convert to HTML
        using var writer = new StringWriter();
        var renderer = new Markdig.Renderers.HtmlRenderer(writer);
        _pipeline.Setup(renderer);
        renderer.Render(document);
        writer.Flush();
        var htmlContent = writer.ToString();

        // Extract TOC
        var toc = document.Descendants<HeadingBlock>()
            .Select(h => {
                var id = Markdig.Renderers.Html.HtmlAttributesExtensions.GetAttributes(h).Id ?? "";
                var title = h.Inline != null ? string.Join("", h.Inline.Select(i => i.ToString())) : "";
                return new TableOfContentsItem(h.Level, title, id);
            })
            .ToArray();

        return new PostContent(metadata, htmlContent, toc);
    }
}
