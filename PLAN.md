# Project Plan: Pisit Blog Headless (C# Static Generator)

This document tracks the progress and architecture of the C# / Markdig-based static blog content pipeline.

## 📋 Roadmap

- [x] **Phase 1: Project Setup & Core Infrastructure**
    - [x] Initialize `PLAN.md` (Self-tracking)
    - [x] Create Test Project (`pisit-blog-headless.Tests` using xUnit)
    - [x] Add NuGet dependencies (`Markdig`, `YamlDotNet`, `SixLabors.ImageSharp`, `System.Text.Json`)
    - [x] Define folder structure (`content/`, `output/`)
    - [x] Create core Data Models (C# Records for Metadata, Post, Page)

- [x] **Phase 2: Markdown & Frontmatter Processing**
    - [x] Implement YAML Frontmatter extraction
    - [x] Configure Markdig Pipeline (Extensions: YAML, Table of Contents, Auto-links)
    - [x] Implement Markdown to HTML conversion
    - [x] Add Metadata validation logic
    - [x] Add Unit Tests for Markdown parsing and metadata validation

- [x] **Phase 3: Image Processing & Asset Management**
    - [x] Implement Image discovery within post directories
    - [x] Implement Asset hashing (SHA256) for cache busting
    - [x] Integrate `ImageSharp` for WebP optimization and resizing
    - [x] Implement path rewriting in Markdown (point local images to hashed assets)
    - [x] Add Tests for image processing and path rewriting

- [x] **Phase 4: Static Site Generation (JSON Artifacts)**
    - [x] Implement individual post JSON generation
    - [x] Implement Paginated Post List generation
    - [x] Implement Tag/Category index generation
    - [x] Generate `rss.xml` and `sitemap.xml`
    - [x] Generate `search-index.json`
    - [x] Add Integration Tests for end-to-end JSON generation

- [x] **Phase 5: Validation & Cleanup**
    - [x] Verify output structure against frontend expectations
    - [x] Performance benchmarking
    - [x] Error handling and build reporting
    - [x] Run all tests and ensure 100% pass rate

- [x] **Phase 6: Configuration & DDD Refactoring**
    - [x] Extract hardcoded values to `appsettings.json`
    - [x] Implement strongly-typed configuration loading
    - [x] Restructure project into `Domain`, `Application`, and `Infrastructure`
    - [x] Update namespaces and dependencies across the solution
    - [x] Verify refactored code with passing tests

- [x] **Phase 7: Example Reorganization**
    - [x] Move sample `content` and `dist` to `Example/`
    - [x] Update `README.md` with example usage instructions
    - [x] Create clean `content/` directory for user start

## 🛠 Tech Stack

- **Runtime:** .NET 10 / C# 14
- **Markdown:** Markdig
- **YAML:** YamlDotNet
- **Images:** SixLabors.ImageSharp
- **JSON:** System.Text.Json

## 📂 Expected Output Structure (`dist/`)

```text
dist/
  posts/
    page/
      1.json
    hello-world.json
  tags/
    tech.json
  assets/
    cover.a1b2c3.webp
  rss.xml
  sitemap.xml
  search-index.json
```
