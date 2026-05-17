using PisitBlog.Application;
using PisitBlog.Domain;
using Xunit;

namespace PisitBlog.Tests;

public class ContentProcessorTests
{
    [Fact]
    public async Task ProcessPostAsync_ValidMarkdown_ReturnsPostContent()
    {
        // Arrange
        var processor = new ContentProcessor();
        var markdown = @"---
title: Hello World
slug: hello-world
date: 2026-05-16
tags:
  - tech
  - web
draft: false
summary: Introduction to the blog system
coverImage: ./cover.webp
---

# Hello World

This is the article content.";

        // Act
        var result = await processor.ProcessPostAsync(markdown);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Hello World", result.Metadata.Title);
        Assert.Equal("hello-world", result.Metadata.Slug);
        Assert.Equal(DateTimeOffset.Parse("2026-05-16"), result.Metadata.Date);
        Assert.Contains("tech", result.Metadata.Tags);
        Assert.Contains("web", result.Metadata.Tags);
        Assert.False(result.Metadata.Draft);
        Assert.Contains("<h1 id=\"hello-world\">Hello World</h1>", result.HtmlContent);
        Assert.Contains("This is the article content.", result.HtmlContent);
    }

    [Fact]
    public async Task ProcessPostAsync_WithImageRewriter_RewritesImagePaths()
    {
        // Arrange
        var processor = new ContentProcessor();
        var markdown = @"---
title: Post with Image
slug: post-with-image
date: 2026-05-16
---
![Test Image](./cover.png)";

        // Act
        var result = await processor.ProcessPostAsync(markdown, url => Task.FromResult("/assets/hashed-image.webp"));

        // Assert
        Assert.NotNull(result);
        Assert.Contains("<img src=\"/assets/hashed-image.webp\" alt=\"Test Image\" />", result.HtmlContent);
    }

    [Fact]
    public async Task ProcessPostAsync_DraftIsTrue_ReturnsNull()
    {
        // Arrange
        var processor = new ContentProcessor();
        var markdown = @"---
title: Draft Post
slug: draft-post
date: 2026-05-16
draft: true
---
This is draft content.";

        // Act
        var result = await processor.ProcessPostAsync(markdown);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ProcessPostAsync_MissingSlug_ThrowsException()
    {
        var processor = new ContentProcessor();
        var markdown = @"---
title: Title
date: 2026-05-16
---
Content";
        await Assert.ThrowsAsync<InvalidOperationException>(() => processor.ProcessPostAsync(markdown));
    }

    [Fact]
    public async Task ProcessPostAsync_MissingDate_ThrowsException()
    {
        var processor = new ContentProcessor();
        var markdown = @"---
title: Title
slug: slug
---
Content";
        await Assert.ThrowsAsync<InvalidOperationException>(() => processor.ProcessPostAsync(markdown));
    }

    [Fact]
    public async Task ProcessPostAsync_MissingYaml_ThrowsException()
    {
        var processor = new ContentProcessor();
        var markdown = "# No YAML here";
        await Assert.ThrowsAsync<InvalidOperationException>(() => processor.ProcessPostAsync(markdown));
    }

    [Fact]
    public async Task ProcessPostAsync_ExtractsTocWithCorrectIds()
    {
        // Arrange
        var processor = new ContentProcessor();
        var markdown = @"---
title: TOC Test
slug: toc-test
date: 2026-05-16
---
# Heading 1
## Heading 2";

        // Act
        var result = await processor.ProcessPostAsync(markdown);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Toc.Length);
        Assert.Equal(1, result.Toc[0].Level);
        Assert.Equal("Heading 1", result.Toc[0].Title);
        Assert.Equal("heading-1", result.Toc[0].Id);
        Assert.Equal(2, result.Toc[1].Level);
        Assert.Equal("Heading 2", result.Toc[1].Title);
        Assert.Equal("heading-2", result.Toc[1].Id);
    }
}
