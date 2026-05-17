using PisitBlog.Configuration;
using PisitBlog.Infrastructure;
using Xunit;

namespace PisitBlog.Tests;

public class GeneratorIntegrationTests : IAsyncLifetime
{
    private readonly string _testContentDir = $"test-content-{Guid.NewGuid()}";
    private readonly string _testDistDir = $"test-dist-{Guid.NewGuid()}";

    public Task InitializeAsync()
    {
        Directory.CreateDirectory(_testContentDir);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        if (Directory.Exists(_testContentDir)) Directory.Delete(_testContentDir, true);
        if (Directory.Exists(_testDistDir)) Directory.Delete(_testDistDir, true);
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Generate_ValidContent_CreatesAllArtifacts()
    {
        // Arrange
        var postDir = Path.Combine(_testContentDir, "test-post");
        Directory.CreateDirectory(postDir);
        await File.WriteAllTextAsync(Path.Combine(postDir, "index.md"), @"---
title: Test Post & Special Characters
slug: test-post
date: 2026-05-16
tags: [test]
summary: summary <with> XML & stuff
---
# Test");

        var config = new BlogConfiguration
        {
            ContentDirectory = _testContentDir,
            OutputDirectory = _testDistDir,
            BaseUrl = "https://test.com",
            PageSize = 5
        };

        var contentProcessor = new PisitBlog.Application.ContentProcessor();
        var imageProcessor = new PisitBlog.Application.ImageProcessor(Path.Combine(config.OutputDirectory, "assets"), config.MaxImageWidth);
        var generator = new Generator(config, contentProcessor, imageProcessor);

        // Act
        await generator.GenerateAsync();

        // Assert
        Assert.True(File.Exists(Path.Combine(_testDistDir, "posts", "test-post.json")));
        Assert.True(File.Exists(Path.Combine(_testDistDir, "posts", "page", "1.json")));
        Assert.True(File.Exists(Path.Combine(_testDistDir, "tags", "test.json")));
        
        var rss = await File.ReadAllTextAsync(Path.Combine(_testDistDir, "rss.xml"));
        Assert.Contains("Test Post &amp; Special Characters", rss);
        Assert.Contains("summary &lt;with&gt; XML &amp; stuff", rss);

        Assert.True(File.Exists(Path.Combine(_testDistDir, "sitemap.xml")));
        Assert.True(File.Exists(Path.Combine(_testDistDir, "search-index.json")));
    }

    [Fact]
    public async Task Generate_WithDrafts_SkipsDraftPosts()
    {
        // Arrange
        var postDir1 = Path.Combine(_testContentDir, "published");
        Directory.CreateDirectory(postDir1);
        await File.WriteAllTextAsync(Path.Combine(postDir1, "index.md"), "---\ntitle: Published\nslug: published\ndate: 2026-05-16\n---\nContent");

        var postDir2 = Path.Combine(_testContentDir, "draft");
        Directory.CreateDirectory(postDir2);
        await File.WriteAllTextAsync(Path.Combine(postDir2, "index.md"), "---\ntitle: Draft\nslug: draft\ndate: 2026-05-16\ndraft: true\n---\nContent");

        var config = new BlogConfiguration { ContentDirectory = _testContentDir, OutputDirectory = _testDistDir };
        var contentProcessor = new PisitBlog.Application.ContentProcessor();
        var imageProcessor = new PisitBlog.Application.ImageProcessor(Path.Combine(config.OutputDirectory, "assets"), config.MaxImageWidth);
        var generator = new Generator(config, contentProcessor, imageProcessor);

        // Act
        await generator.GenerateAsync();

        // Assert
        Assert.True(File.Exists(Path.Combine(_testDistDir, "posts", "published.json")));
        Assert.False(File.Exists(Path.Combine(_testDistDir, "posts", "draft.json")));
    }

    [Fact]
    public async Task Generate_MultiplePosts_HandlesPaginationAndSorting()
    {
        // Arrange
        var config = new BlogConfiguration 
        { 
            ContentDirectory = _testContentDir, 
            OutputDirectory = _testDistDir,
            PageSize = 2 
        };

        for (int i = 1; i <= 3; i++)
        {
            var postDir = Path.Combine(_testContentDir, $"post-{i}");
            Directory.CreateDirectory(postDir);
            await File.WriteAllTextAsync(Path.Combine(postDir, "index.md"), $"---\ntitle: Post {i}\nslug: post-{i}\ndate: 2026-05-0{i}\n---\nContent");
        }

        var contentProcessor = new PisitBlog.Application.ContentProcessor();
        var imageProcessor = new PisitBlog.Application.ImageProcessor(Path.Combine(config.OutputDirectory, "assets"), config.MaxImageWidth);
        var generator = new Generator(config, contentProcessor, imageProcessor);

        // Act
        await generator.GenerateAsync();

        // Assert
        Assert.True(File.Exists(Path.Combine(_testDistDir, "posts", "page", "1.json")));
        Assert.True(File.Exists(Path.Combine(_testDistDir, "posts", "page", "2.json")));

        var page1Json = await File.ReadAllTextAsync(Path.Combine(_testDistDir, "posts", "page", "1.json"));
        // Post 3 should be first because it's the newest
        Assert.Contains("\"slug\": \"post-3\"", page1Json);
        Assert.Contains("\"slug\": \"post-2\"", page1Json);
        Assert.DoesNotContain("\"slug\": \"post-1\"", page1Json);

        var page2Json = await File.ReadAllTextAsync(Path.Combine(_testDistDir, "posts", "page", "2.json"));
        Assert.Contains("\"slug\": \"post-1\"", page2Json);
    }

    [Fact]
    public async Task Generate_TagsAreCaseInsensitive()
    {
        // Arrange
        var postDir1 = Path.Combine(_testContentDir, "post1");
        Directory.CreateDirectory(postDir1);
        await File.WriteAllTextAsync(Path.Combine(postDir1, "index.md"), "---\ntitle: Post 1\nslug: post1\ndate: 2026-05-16\ntags: [Tech]\n---\nContent");

        var postDir2 = Path.Combine(_testContentDir, "post2");
        Directory.CreateDirectory(postDir2);
        await File.WriteAllTextAsync(Path.Combine(postDir2, "index.md"), "---\ntitle: Post 2\nslug: post2\ndate: 2026-05-17\ntags: [tech]\n---\nContent");

        var config = new BlogConfiguration { ContentDirectory = _testContentDir, OutputDirectory = _testDistDir };
        var contentProcessor = new PisitBlog.Application.ContentProcessor();
        var imageProcessor = new PisitBlog.Application.ImageProcessor(Path.Combine(config.OutputDirectory, "assets"), config.MaxImageWidth);
        var generator = new Generator(config, contentProcessor, imageProcessor);

        // Act
        await generator.GenerateAsync();

        // Assert
        Assert.True(File.Exists(Path.Combine(_testDistDir, "tags", "tech.json")));
        var tagJson = await File.ReadAllTextAsync(Path.Combine(_testDistDir, "tags", "tech.json"));
        Assert.Contains("\"slug\": \"post1\"", tagJson);
        Assert.Contains("\"slug\": \"post2\"", tagJson);
    }
}
