using Microsoft.Extensions.Options;
using ShareBook.Service.Server;
using ShareBook.Service.Upload;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace ShareBook.Test.Unit.Services;

public class UploadServiceTests
{
    [Fact]
    public async Task BookCoverUploadCreatesAndDeletesProportionalThumbnail()
    {
        var imageRoot = Path.Combine(Path.GetTempPath(), $"sharebook-thumbnails-{Guid.NewGuid():N}");
        var service = CreateService(imageRoot);

        try
        {
            var sourceBytes = CreatePng(800, 1000);

            await service.UploadImageAsync(sourceBytes, "capa.png", "Books");

            var sourcePath = Path.Combine(imageRoot, "Books", "capa.png");
            var thumbnailPath = Path.Combine(imageRoot, "Books", "Thumbs", "capa.webp");
            Assert.True(File.Exists(sourcePath));
            Assert.True(File.Exists(thumbnailPath));

            using var thumbnail = await Image.LoadAsync(thumbnailPath);
            Assert.Equal(360, thumbnail.Width);
            Assert.Equal(450, thumbnail.Height);

            var backfill = await service.BackfillBookThumbnailsAsync();
            Assert.Equal(1, backfill.SourceFiles);
            Assert.Equal(1, backfill.Processed);
            Assert.False(backfill.HasMore);
            Assert.Equal(1, backfill.Skipped);
            Assert.Empty(backfill.Failures);

            await service.DeleteFileIfExistsAsync("capa.png", "Books");
            Assert.False(File.Exists(sourcePath));
            Assert.False(File.Exists(thumbnailPath));
        }
        finally
        {
            if (Directory.Exists(imageRoot))
                Directory.Delete(imageRoot, true);
        }
    }

    [Fact]
    public async Task ReplacingBookCoverExtensionPreservesNewThumbnail()
    {
        var imageRoot = Path.Combine(Path.GetTempPath(), $"sharebook-thumbnails-{Guid.NewGuid():N}");
        var service = CreateService(imageRoot);

        try
        {
            await service.UploadImageAsync(CreateJpeg(800, 1000), "capa.jpg", "Books");

            var oldSourcePath = Path.Combine(imageRoot, "Books", "capa.jpg");
            var newSourcePath = Path.Combine(imageRoot, "Books", "capa.png");
            var thumbnailPath = Path.Combine(imageRoot, "Books", "Thumbs", "capa.webp");
            var oldThumbnailBytes = await File.ReadAllBytesAsync(thumbnailPath);

            await service.UploadImageAsync(CreatePng(800, 1000), "capa.png", "Books");
            var newThumbnailBytes = await File.ReadAllBytesAsync(thumbnailPath);

            Assert.NotEqual(oldThumbnailBytes, newThumbnailBytes);

            await service.DeleteReplacedImageAsync("capa.jpg", "capa.png", "Books");

            Assert.False(File.Exists(oldSourcePath));
            Assert.True(File.Exists(newSourcePath));
            Assert.True(File.Exists(thumbnailPath));
            Assert.Equal(newThumbnailBytes, await File.ReadAllBytesAsync(thumbnailPath));
        }
        finally
        {
            if (Directory.Exists(imageRoot))
                Directory.Delete(imageRoot, true);
        }
    }

    private static UploadService CreateService(string imageRoot)
    {
        var imageSettings = Options.Create(new ImageSettings
        {
            ImagePath = imageRoot,
            EBookPdfPath = Path.Combine(imageRoot, "Ebooks")
        });
        var serverSettings = Options.Create(new ServerSettings
        {
            BackendUrl = "https://api.sharebook.com.br"
        });

        return new UploadService(imageSettings, serverSettings);
    }

    private static byte[] CreatePng(int width, int height)
    {
        using var image = new Image<Rgba32>(width, height, Color.CornflowerBlue);
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }

    private static byte[] CreateJpeg(int width, int height)
    {
        using var image = new Image<Rgba32>(width, height, Color.OrangeRed);
        using var stream = new MemoryStream();
        image.SaveAsJpeg(stream);
        return stream.ToArray();
    }
}
