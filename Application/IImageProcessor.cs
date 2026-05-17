namespace PisitBlog.Application;

public interface IImageProcessor
{
    Task<string> ProcessImageAsync(string sourcePath, string baseDirectory);
}
