namespace PisitBlog.Configuration;

public record BlogConfiguration
{
    public string BaseUrl { get; init; } = "https://yourblog.com";
    public int PageSize { get; init; } = 10;
    public string SiteName { get; init; } = "My Blog";
    public string SiteDescription { get; init; } = "Insights and updates";
    public int MaxImageWidth { get; init; } = 1600;
    public string ContentDirectory { get; init; } = "content";
    public string OutputDirectory { get; init; } = "dist";
}
