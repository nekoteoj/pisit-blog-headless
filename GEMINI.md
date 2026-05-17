# Gemini Workspace Instructions: Pisit Blog Headless

This document provides architectural context and engineering standards for Gemini CLI when operating within the `pisit-blog-headless` repository.

## 🚀 Project Overview

**Pisit Blog Headless** is a high-performance static content generator built with **.NET 10** and **C# 14**. It is designed to transform a directory of Markdown files into a production-ready, headless "JSON API" that can be consumed by any modern frontend framework (React, Vue, Astro, etc.).

### Key Technologies
- **Runtime:** .NET 10 Console Application.
- **Markdown:** [Markdig](https://github.com/xoofx/markdig) with YAML and Advanced extensions.
- **Image Processing:** [SkiaSharp](https://github.com/mono/SkiaSharp) for high-quality WebP conversion and resizing.
- **XML:** `System.Xml.Linq` (XDocument) for secure RSS and Sitemap generation.
- **Serialization:** `System.Text.Json` (JSON) and `YamlDotNet` (Frontmatter).
- **Testing:** xUnit for Unit and Integration testing.

### Architecture (Pragmatic DDD)
The project follows Domain-Driven Design principles to ensure high maintainability:
- **`PisitBlog.Domain`**: Core entities and data contracts (`PostMetadata`, `PostContent`).
- **`PisitBlog.Application`**: Domain services handling business logic (`ContentProcessor`, `ImageProcessor`).
- **`PisitBlog.Infrastructure`**: Implementation details, including the `Generator` orchestrator and configuration loading.
- **`PisitBlog.Configuration`**: Strongly-typed mapping for the configuration system.

---

## 🛠 Building and Running

### Key Commands
- **Build:** `dotnet build`
- **Run Generator:** `dotnet run` (Uses paths defined in `appsettings.json`)
- **Custom Paths:** `dotnet run <content_path> <output_path>`
- **Test:** `dotnet test`

### Configuration (`appsettings.json`)
The system uses the standard .NET configuration provider.
- `appsettings.json`: Contains default repository-wide settings.
- `appsettings.Local.json`: (Ignored by Git) Use this for local environment overrides.

---

## 📐 Development Conventions

### Coding Standards
- **Modern C#:** Exclusively use C# 14 features. Use `record` types for DTOs, the `required` modifier for mandatory properties, and **Collection Expressions** `[]` for arrays.
- **Async First:** All I/O operations (File reading/writing, Image encoding, XML saving) must be asynchronous using `Task` and `await`.
- **Resource Management:** Strictly use `using` blocks or declarations for `IDisposable` types (e.g., `SKBitmap`, `SKImage`, `FileStream`).
- **File-Scoped Namespaces:** Use the `namespace MyNamespace;` syntax.

### Engineering Mandates
- **Security:**
    - **Path Traversal Protection:** Always normalize and validate paths using `Path.GetFullPath`. Ensure resolved paths stay within the intended source directory.
    - **XML Safety:** Never use string interpolation for XML generation. Always use `XDocument` or `XmlWriter` to guarantee proper escaping.
- **Image Pipeline:**
    - Always optimize local images to **WebP** format.
    - Filenames must include a content-based **hash** (SHA256) for immutable caching.
    - Respect the `MaxImageWidth` configuration during resizing.
- **Testing Requirements:**
    - Maintain 100% test pass rate.
    - Every bug fix or feature addition **must** include a corresponding unit or integration test.
    - Use isolated temporary directories for all test-related file system operations.

---

## 📂 Expected Output Structure (`dist/`)

The generator emits the following artifacts:
- `posts/{slug}.json`: Full post data + rendered HTML.
- `posts/page/{n}.json`: Paginated metadata lists.
- `tags/{tag}.json`: Post lists filtered by tag.
- `search-index.json`: Site-wide searchable metadata.
- `rss.xml` & `sitemap.xml`: SEO-optimized feeds.
- `assets/`: Optimized, hashed image assets.
