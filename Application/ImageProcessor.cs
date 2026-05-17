using System.Security.Cryptography;
using SkiaSharp;

namespace PisitBlog.Application;

public class ImageProcessor : IImageProcessor
{
    private readonly string _outputDir;
    private readonly int _maxImageWidth;
    private readonly SemaphoreSlim[] _locks;

    public ImageProcessor(string outputDir, int maxImageWidth = 1600)
    {
        _outputDir = Path.GetFullPath(outputDir);
        _maxImageWidth = maxImageWidth;
        _locks = Enumerable.Range(0, 32).Select(_ => new SemaphoreSlim(1, 1)).ToArray();
    }

    public async Task<string> ProcessImageAsync(string sourcePath, string baseDirectory)
    {
        Directory.CreateDirectory(_outputDir);

        // Path Traversal Protection
        var fullPath = Path.GetFullPath(Path.Combine(baseDirectory, sourcePath));
        var basePath = Path.GetFullPath(baseDirectory);
        if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            basePath += Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(basePath))
        {
            throw new UnauthorizedAccessException($"Attempted to access file outside of directory: {sourcePath}");
        }

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Image file not found.", fullPath);
        }

        var hash = await GetFileHashAsync(fullPath);
        var extension = ".webp";
        var fileName = $"{Path.GetFileNameWithoutExtension(fullPath)}.{hash[..8]}{extension}";
        var outputPath = Path.Combine(_outputDir, fileName);

        var lockIndex = Math.Abs(outputPath.GetHashCode()) % _locks.Length;
        var fileLock = _locks[lockIndex];
        await fileLock.WaitAsync();
        try
        {
            if (!File.Exists(outputPath))
            {
                await Task.Run(() =>
                {
                    using var input = File.OpenRead(fullPath);
                    using var bitmap = SKBitmap.Decode(input);
                    if (bitmap == null) throw new InvalidOperationException($"Failed to decode image: {fullPath}");

                    SKBitmap finalBitmap = bitmap;
                    bool wasResized = false;

                    if (bitmap.Width > _maxImageWidth)
                    {
                        float ratio = (float)_maxImageWidth / bitmap.Width;
                        int newHeight = (int)(bitmap.Height * ratio);
                        
                        var resizedInfo = new SKImageInfo(_maxImageWidth, newHeight);
                        // Standard Lanczos-like filter for high quality resizing
                        var resizedBitmap = bitmap.Resize(resizedInfo, SKSamplingOptions.Default);
                        
                        if (resizedBitmap != null)
                        {
                            finalBitmap = resizedBitmap;
                            wasResized = true;
                        }
                    }

                    try
                    {
                        using var image = SKImage.FromBitmap(finalBitmap);
                        using var data = image.Encode(SKEncodedImageFormat.Webp, 80);
                        using var output = File.Create(outputPath);
                        data.SaveTo(output);
                    }
                    finally
                    {
                        if (wasResized) finalBitmap.Dispose();
                    }
                });
            }
        }
        finally
        {
            fileLock.Release();
        }

        return $"/assets/{fileName}";
    }

    private static async Task<string> GetFileHashAsync(string filePath)
    {
        using var sha256 = SHA256.Create();
        await using var stream = File.OpenRead(filePath);
        var hashBytes = await sha256.ComputeHashAsync(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}
