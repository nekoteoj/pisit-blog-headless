using PisitBlog.Domain;

namespace PisitBlog.Application;

public interface IContentProcessor
{
    Task<PostContent?> ProcessPostAsync(string markdownContent, Func<string, Task<string>>? imageRewriter = null);
}
