namespace PisitBlog.Configuration;

public class BlogConfiguration
{
    public string BaseUrl { get; set; } = "https://yourblog.com";
    public int PageSize { get; set; } = 10;
    public string SiteName { get; set; } = "My Blog";
    public string SiteDescription { get; set; } = "Recent posts";
    public int MaxImageWidth { get; set; } = 1600;
    public string ContentDirectory { get; set; } = "content";
    public string OutputDirectory { get; set; } = "dist";
}
