namespace PisitBlog.Domain;

public static class UrlHelper
{
    public static bool IsExternalUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;

        if (url.StartsWith("//") || 
            url.StartsWith("data:", StringComparison.OrdinalIgnoreCase) || 
            url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }

        return false;
    }
}
