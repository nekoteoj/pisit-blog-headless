using Microsoft.Extensions.Configuration;
using PisitBlog.Application;
using PisitBlog.Configuration;
using PisitBlog.Infrastructure;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .Build();

var blogConfig = configuration.GetSection("Blog").Get<BlogConfiguration>() ?? new BlogConfiguration();

// Override with command line args if provided
if (args.Length > 0) blogConfig = blogConfig with { ContentDirectory = args[0] };
if (args.Length > 1) blogConfig = blogConfig with { OutputDirectory = args[1] };

if (!Uri.IsWellFormedUriString(blogConfig.BaseUrl, UriKind.Absolute))
{
    Console.Error.WriteLine("Error: BlogConfiguration.BaseUrl must be a valid absolute URL.");
    return 1;
}
if (string.IsNullOrWhiteSpace(blogConfig.SiteName))
{
    Console.Error.WriteLine("Error: BlogConfiguration.SiteName cannot be empty.");
    return 1;
}
if (string.IsNullOrWhiteSpace(blogConfig.OutputDirectory))
{
    Console.Error.WriteLine("Error: BlogConfiguration.OutputDirectory cannot be empty.");
    return 1;
}
if (!Directory.Exists(blogConfig.ContentDirectory))
{
    Console.Error.WriteLine($"Error: ContentDirectory '{blogConfig.ContentDirectory}' does not exist.");
    return 1;
}

Console.WriteLine($"Starting generation for '{blogConfig.SiteName}'...");
Console.WriteLine($"Input: {Path.GetFullPath(blogConfig.ContentDirectory)}");
Console.WriteLine($"Output: {Path.GetFullPath(blogConfig.OutputDirectory)}");

try
{
    var contentProcessor = new ContentProcessor();
    var imageProcessor = new ImageProcessor(
        Path.Combine(blogConfig.OutputDirectory, "assets"), 
        blogConfig.MaxImageWidth);
        
    var generator = new Generator(blogConfig, contentProcessor, imageProcessor);
    await generator.GenerateAsync();
    Console.WriteLine("Generation completed successfully.");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error during generation: {ex.Message}");
    return 1;
}
