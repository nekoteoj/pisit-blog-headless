using PisitBlog.Domain;
using Xunit;

namespace PisitBlog.Tests;

public class UrlHelperTests
{
    [Theory]
    [InlineData("http://example.com/image.png", true)]
    [InlineData("https://example.com/image.png", true)]
    [InlineData("//cdn.example.com/img.png", true)]
    [InlineData("data:image/png;base64,iVBORw0KGgo...", true)]
    [InlineData("mailto:test@example.com", true)]
    [InlineData("/local/path/image.png", false)]
    [InlineData("./relative/image.png", false)]
    [InlineData("../parent/image.png", false)]
    [InlineData("image.png", false)]
    [InlineData(null, false)]
    [InlineData("", false)]
    public void IsExternalUrl_ReturnsExpectedResult(string? url, bool expected)
    {
        var result = UrlHelper.IsExternalUrl(url);
        Assert.Equal(expected, result);
    }
}