using PisitBlog.Application;
using SkiaSharp;
using Xunit;

namespace PisitBlog.Tests;

public class ImageProcessorTests : IDisposable
{
    private readonly string _testOutputDir = "test-output-assets";

    public ImageProcessorTests()
    {
        if (Directory.Exists(_testOutputDir)) Directory.Delete(_testOutputDir, true);
    }

    [Fact]
    public async Task ProcessImage_ValidImage_ReturnsHashedPathAndSavesFile()
    {
        // Arrange
        var processor = new ImageProcessor(_testOutputDir, 1600);
        var sourcePath = "test-image.png";
        
        using (var bitmap = new SKBitmap(100, 100))
        using (var image = SKImage.FromBitmap(bitmap))
        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
        using (var stream = File.Create(sourcePath))
        {
            data.SaveTo(stream);
        }

        try
        {
            // Act
            var result = await processor.ProcessImageAsync(sourcePath, ".");

            // Assert
            Assert.StartsWith("/assets/test-image.", result);
            Assert.EndsWith(".webp", result);
            
            var fileName = Path.GetFileName(result);
            var outputPath = Path.Combine(_testOutputDir, fileName);
            Assert.True(File.Exists(outputPath));
        }
        finally
        {
            if (File.Exists(sourcePath)) File.Delete(sourcePath);
        }
    }

    [Fact]
    public async Task ProcessImage_FileNotFound_ThrowsException()
    {
        var processor = new ImageProcessor(_testOutputDir);
        await Assert.ThrowsAsync<FileNotFoundException>(() => processor.ProcessImageAsync("non-existent.png", "."));
    }

    [Fact]
    public async Task ProcessImage_PathTraversal_ThrowsException()
    {
        var processor = new ImageProcessor(_testOutputDir);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => processor.ProcessImageAsync("../secret.txt", "content"));
    }

    [Fact]
    public async Task ProcessImage_LargeImage_ResizesImage()
    {
        // Arrange
        int maxWidth = 100;
        var processor = new ImageProcessor(_testOutputDir, maxWidth);
        var sourcePath = "large-image.png";
        
        using (var bitmap = new SKBitmap(200, 100))
        using (var image = SKImage.FromBitmap(bitmap))
        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
        using (var stream = File.Create(sourcePath))
        {
            data.SaveTo(stream);
        }

        try
        {
            // Act
            var result = await processor.ProcessImageAsync(sourcePath, ".");

            // Assert
            var fileName = Path.GetFileName(result);
            var outputPath = Path.Combine(_testOutputDir, fileName);
            
            using var input = File.OpenRead(outputPath);
            using var outputBitmap = SKBitmap.Decode(input);
            Assert.Equal(maxWidth, outputBitmap.Width);
        }
        finally
        {
            if (File.Exists(sourcePath)) File.Delete(sourcePath);
        }
    }

    [Fact]
    public async Task ProcessImage_IsIdempotent()
    {
        // Arrange
        var processor = new ImageProcessor(_testOutputDir);
        var sourcePath = "idempotent-test.png";
        
        using (var bitmap = new SKBitmap(10, 10))
        using (var image = SKImage.FromBitmap(bitmap))
        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
        using (var stream = File.Create(sourcePath))
        {
            data.SaveTo(stream);
        }

        try
        {
            // Act
            var result1 = await processor.ProcessImageAsync(sourcePath, ".");
            var result2 = await processor.ProcessImageAsync(sourcePath, ".");

            // Assert
            Assert.Equal(result1, result2);
        }
        finally
        {
            if (File.Exists(sourcePath)) File.Delete(sourcePath);
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(_testOutputDir)) Directory.Delete(_testOutputDir, true);
    }
}
