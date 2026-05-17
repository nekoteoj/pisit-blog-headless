# Pisit Blog Headless (C# Static Generator)

A high-performance, .NET 10 powered static content generator for headless blogs. This tool transforms Markdown files and local assets into a production-ready static JSON "API", optimized images, and SEO artifacts.

## 🚀 Overview

This project implements a **fully static content delivery architecture**. It eliminates the need for databases or runtime servers by pre-compiling your blog content into static JSON artifacts. These artifacts can be consumed directly by any modern frontend (React, Vue, Astro, Next.js, etc.) via simple HTTP fetches.

## ✨ Features

- **Blazing Fast Build:** Leverages .NET 10 and the high-performance [Markdig](https://github.com/xoofx/markdig) parser.
- **Smart Image Pipeline:**
  - Automatic conversion to optimized **WebP** format using **SkiaSharp**.
  - Content-based **hashing** for immutable caching (e.g., `cover.a1b2c3.webp`).
  - Automatic **resizing** (configurable `MaxImageWidth`).
  - Automatic path rewriting in both HTML and Metadata.
- **Rich Markdown Support:**
  - YAML Frontmatter extraction.
  - Automated **Table of Contents** with heading IDs.
  - Support for images inside Markdown content.
- **Headless API Generation:**
  - Pre-computed pagination, tag indexes, and site-wide search index.
- **SEO Ready:** Automatic generation of `rss.xml` and `sitemap.xml`.
- **Clean Architecture:** Organized using Pragmatic Domain-Driven Design (DDD).

---

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A text editor for writing Markdown.

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/pisit-blog-headless.git
   cd pisit-blog-headless
   ```
2. Restore dependencies:
   ```bash
   dotnet restore
   ```

### Running the Generator
Simply execute the project:
```bash
dotnet run
```
By default, it reads from the `content` folder and outputs to the `dist` folder. You can override these via command-line arguments. For example, to run the provided example:
```bash
dotnet run ./Example/content ./Example/dist
```

---

## ⚙️ Configuration (`appsettings.json`)

The generator is highly configurable via the `appsettings.json` file. You can also create an `appsettings.Local.json` file to override any settings for your local environment without affecting the repository.

### Local Overrides
Create a file named `appsettings.Local.json` in the root directory. Only define the fields you wish to change:
```json
{
  "Blog": {
    "BaseUrl": "http://localhost:3000",
    "OutputDirectory": "./local-test-dist"
  }
}
```
**Note:** `appsettings.Local.json` is already added to `.gitignore` and will not be committed.

### Configuration Fields
| Key | Description |
| :--- | :--- |
| `BaseUrl` | The root URL of your deployed blog (used for RSS/Sitemap). |
| `PageSize` | Number of posts to include in each paginated JSON file. |
| `SiteName` | Title used in RSS feeds and metadata. |
| `MaxImageWidth` | Images wider than this will be automatically resized. |
| `ContentDirectory` | Where your Markdown source files live. |
| `OutputDirectory` | Where the generated JSON and assets will be saved. |

---

## ✍️ Authoring Content

### Directory Structure
Each post must be in its own directory within the `ContentDirectory`. An example structure is provided in the `Example` folder.
```text
content/
  hello-world/
    index.md       <-- The main post
    cover.png      <-- Post assets (images, etc.)
    diagram.jpg
```

### Frontmatter (Metadata)
Every `index.md` must start with a YAML frontmatter block:

```markdown
---
title: Hello World
slug: hello-world
date: 2026-05-16
tags: [tech, web]
draft: false
summary: An introduction to my new blog system.
coverImage: ./cover.png
---
```

#### Frontmatter Field Definitions

| Field | Type | Description |
| :--- | :--- | :--- |
| `title` | `string` | **Required.** The display title of the post. |
| `slug` | `string` | **Required.** Unique URL identifier. Used as the filename for the generated post JSON. |
| `date` | `ISO 8601` | **Required.** Publication date (e.g., `2026-05-16`). Used for sorting and RSS/Sitemaps. |
| `tags` | `string[]` | List of categories or tags associated with the post. |
| `draft` | `boolean` | If `true`, the post is currently ignored during generation (useful for work-in-progress). |
| `summary` | `string` | A brief excerpt used in post listings and RSS descriptions. |
| `coverImage`| `string` | Relative path to the cover image (e.g., `./cover.png`). Will be optimized and hashed. |

---

## 📋 Consuming the API

The generator produces static JSON files that act as your "API".

### 1. Paginated Post List
**Path:** `/posts/page/{n}.json`
Used for your homepage or archive. Returns a `PageResponse` containing an array of `PostMetadata`.

| Field | Type | Description |
| :--- | :--- | :--- |
| `page` | `int` | The current page number. |
| `totalPages`| `int` | Total number of pages available. |
| `items` | `PostMetadata[]` | Array of post metadata for the current page. |

### 2. Individual Blog Post
**Path:** `/posts/{slug}.json`
Contains the full rendered HTML and metadata. Returns a `PostContent` object.

| Field | Type | Description |
| :--- | :--- | :--- |
| `metadata` | `PostMetadata` | The full metadata object (see definition below). |
| `htmlContent`| `string` | The rendered HTML content of the post. |
| `toc` | `TableOfContentsItem[]` | Structured list of headings for navigation. |

### 3. Data Schema Definitions

#### `PostMetadata`
Used in paginated lists, tag indexes, and individual post objects.

| Field | Type | Description |
| :--- | :--- | :--- |
| `title` | `string` | Post title. |
| `slug` | `string` | Unique identifier used for routing. |
| `date` | `string` | ISO 8601 formatted date string. |
| `tags` | `string[]` | List of tags. |
| `draft` | `boolean` | Draft status. |
| `summary` | `string` | Post summary/excerpt. |
| `coverImage`| `string` | **Rewritten Path.** Points to the optimized asset (e.g., `/assets/cover.a1b2c3.webp`). |

#### `TableOfContentsItem`
Used to build client-side navigation or sidebars.

| Field | Type | Description |
| :--- | :--- | :--- |
| `level` | `int` | Heading level (e.g., 1 for `<h1>`, 2 for `<h2>`). |
| `title` | `string` | The text content of the heading. |
| `id` | `string` | The HTML ID assigned to the heading (e.g., `hello-world`). |

### 4. Search Index
**Path:** `/search-index.json`
A compact array of objects `{ title, slug, summary, date }` intended for client-side fuzzy search.

---

## 🛠 Developer Info

### Project Architecture
- **Domain:** Core records and models.
- **Application:** Markdown and Image processing logic (Domain Services).
- **Infrastructure:** The build orchestrator and configuration loading.

### Testing
Run unit and integration tests:
```bash
dotnet test
```

## 📄 License
This project is licensed under the MIT License.
