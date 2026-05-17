using Microsoft.Extensions.Configuration;
using PisitBlog.Configuration;
using PisitBlog.Infrastructure;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .Build();

var blogConfig = configuration.GetSection("Blog").Get<BlogConfiguration>() ?? new BlogConfiguration();

// Override with command line args if provided
if (args.Length > 0) blogConfig.ContentDirectory = args[0];
if (args.Length > 1) blogConfig.OutputDirectory = args[1];

Console.WriteLine($"Starting generation for '{blogConfig.SiteName}'...");
Console.WriteLine($"Input: {Path.GetFullPath(blogConfig.ContentDirectory)}");
Console.WriteLine($"Output: {Path.GetFullPath(blogConfig.OutputDirectory)}");

try
{
    var generator = new Generator(blogConfig);
    await generator.GenerateAsync();
    Console.WriteLine("Generation completed successfully.");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error during generation: {ex.Message}");
    Environment.Exit(1);
}
