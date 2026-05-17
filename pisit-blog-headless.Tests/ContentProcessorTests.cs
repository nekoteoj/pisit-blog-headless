using PisitBlog.Application;
using PisitBlog.Domain;
using Xunit;

namespace PisitBlog.Tests;

public class ContentProcessorTests
{
    [Fact]
    public void ProcessPost_ValidMarkdown_ReturnsPostContent()
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
        var result = processor.ProcessPost(markdown);

        // Assert
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
    public void ProcessPost_WithImageRewriter_RewritesImagePaths()
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
        var result = processor.ProcessPost(markdown, url => "/assets/hashed-image.webp");

        // Assert
        Assert.Contains("<img src=\"/assets/hashed-image.webp\" alt=\"Test Image\" />", result.HtmlContent);
    }

    [Fact]
    public void ProcessPost_MissingSlug_ThrowsException()
    {
        var processor = new ContentProcessor();
        var markdown = @"---
title: Title
date: 2026-05-16
---
Content";
        Assert.Throws<InvalidOperationException>(() => processor.ProcessPost(markdown));
    }

    [Fact]
    public void ProcessPost_MissingDate_ThrowsException()
    {
        var processor = new ContentProcessor();
        var markdown = @"---
title: Title
slug: slug
---
Content";
        Assert.Throws<InvalidOperationException>(() => processor.ProcessPost(markdown));
    }

    [Fact]
    public void ProcessPost_MissingYaml_ThrowsException()
    {
        var processor = new ContentProcessor();
        var markdown = "# No YAML here";
        Assert.Throws<InvalidOperationException>(() => processor.ProcessPost(markdown));
    }

    [Fact]
    public void ProcessPost_ExtractsTocWithCorrectIds()
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
        var result = processor.ProcessPost(markdown);

        // Assert
        Assert.Equal(2, result.Toc.Length);
        Assert.Equal(1, result.Toc[0].Level);
        Assert.Equal("Heading 1", result.Toc[0].Title);
        Assert.Equal("heading-1", result.Toc[0].Id);
        Assert.Equal(2, result.Toc[1].Level);
        Assert.Equal("Heading 2", result.Toc[1].Title);
        Assert.Equal("heading-2", result.Toc[1].Id);
    }
}
