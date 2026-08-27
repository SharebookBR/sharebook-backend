using ShareBook.Helper.Extensions;
using ShareBook.Helper.Image;
using Xunit;
using Flurl.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ShareBook.Test.Unit.Helpers
{
    public class HelperTests
    {
        [Fact]
        public void GenerateSlugValid()
        {
            var phrase = "Harry Potter and the Philosopher's Stone";

            var actual = phrase.GenerateSlug();
            var expected = "harry-potter-and-the-philosophers-stone";

            Assert.Equal(actual, expected);
        }

        [Fact]
        public void GenerateSlugInvalid()
        {
            var phrase = "Harry Potter and the Philosopher's Stone";

            var actual = phrase.GenerateSlug();
            var expected = "Harry-potter-and-thE-philosophers-stone";

            Assert.NotEqual(actual, expected);
        }

        [Fact]
        public void NextAvailableCopySlug_ShouldKeepCleanBaseWhenAvailable()
        {
            var baseSlug = "o-pequeno-principe";

            var actual = baseSlug.NextAvailableCopySlug(Array.Empty<string>());

            Assert.Equal("o-pequeno-principe", actual);
        }

        [Fact]
        public void NextAvailableCopySlug_ShouldUseFirstFreeCopyNumber()
        {
            var baseSlug = "o-pequeno-principe";
            var existingSlugs = new[]
            {
                baseSlug,
                $"{baseSlug}_copy1",
                $"{baseSlug}_copy3",
                $"{baseSlug}-edicao-especial"
            };

            var actual = baseSlug.NextAvailableCopySlug(existingSlugs);

            Assert.Equal("o-pequeno-principe_copy2", actual);
        }

        [Fact]
        public void FormatImageValid()
        {

            var imageName = "image-name.png";

            var title = "Harry Potter and the Philosopher's Stone";

            var expected = "harry-potter-and-the-philosophers-stone.png";

            var actual = ImageHelper.FormatImageName(imageName, title.GenerateSlug());

            Assert.Equal(actual, expected);
        }

        [Fact]
        public void ImageUrlValid()
        {
            var expected = @"http://dev.sharebook.com.br/Images/Books/image.jpg";
            var actual = ImageHelper.GenerateImageUrl("image.jpg", "wwwroot/Images/Books", "http://dev.sharebook.com.br");

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("o-mar-de-monstros.png", "o-mar-de-monstros.webp")]
        [InlineData("uma-capa.jpeg", "uma-capa.webp")]
        public void ThumbnailNameUsesBookSlug(string imageName, string expected)
        {
            var actual = ImageHelper.FormatThumbnailName(imageName);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void BookThumbnailPreservesAspectRatioAndUsesWebp()
        {
            using var source = new Image<Rgba32>(800, 1000, Color.CornflowerBlue);
            using var sourceStream = new MemoryStream();
            source.SaveAsPng(sourceStream);

            var result = ImageHelper.CreateBookThumbnail(sourceStream.ToArray());

            using var thumbnail = SixLabors.ImageSharp.Image.Load(result);
            Assert.Equal(360, thumbnail.Width);
            Assert.Equal(450, thumbnail.Height);
            Assert.Equal("Webp", SixLabors.ImageSharp.Image.DetectFormat(result).Name);
        }

        [Fact]
        public void BookThumbnailDoesNotUpscaleSmallCover()
        {
            using var source = new Image<Rgba32>(120, 180, Color.CornflowerBlue);
            using var sourceStream = new MemoryStream();
            source.SaveAsPng(sourceStream);

            var result = ImageHelper.CreateBookThumbnail(sourceStream.ToArray());

            using var thumbnail = SixLabors.ImageSharp.Image.Load(result);
            Assert.Equal(120, thumbnail.Width);
            Assert.Equal(180, thumbnail.Height);
        }

        [Fact]
        public void IncrementalIsValidCopy1()
        {
            var phrase = "Harry-potter-and-thE-philosophers-stone";
            var expected = "Harry-potter-and-thE-philosophers-stone_copy1";
            var actual = phrase.AddIncremental();

            Assert.Equal(expected, actual);
        }


        [Fact]
        public void IncrementalIsValidCopy2()
        {
            var phrase = "Harry-potter-and-thE-philosophers-stone_copy1";
            var expected = "Harry-potter-and-thE-philosophers-stone_copy2";
            var actual = phrase.AddIncremental();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IncrementalIsValidCopy11002()
        {
            var phrase = "Harry-potter-and-thE-philosophers-stone_copy11001";
            var expected = "Harry-potter-and-thE-philosophers-stone_copy11002";
            var actual = phrase.AddIncremental();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IncrementalIsValidCopy100()
        {
            var phrase = "Harry-potter-and-thE-philosophers-stone_copy99";
            var expected = "Harry-potter-and-thE-philosophers-stone_copy100";
            var actual = phrase.AddIncremental();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ClientVersionValidation()
        {
            var minVersion = "v0.3.1";

            bool result = Helper.ClientVersionValidation.IsValidVersion("v1.2.3", minVersion);
            Assert.True(result);

            result = Helper.ClientVersionValidation.IsValidVersion("v1.0.0", minVersion);
            Assert.True(result);

            result = Helper.ClientVersionValidation.IsValidVersion("v0.4.0", minVersion);
            Assert.True(result);

            result = Helper.ClientVersionValidation.IsValidVersion("v0.3.2", minVersion);
            Assert.True(result);

            result = Helper.ClientVersionValidation.IsValidVersion("v0.3.1", minVersion);
            Assert.True(result);

            result = Helper.ClientVersionValidation.IsValidVersion("v0.3.10", minVersion);
            Assert.True(result);

            result = Helper.ClientVersionValidation.IsValidVersion("v0.3", minVersion);
            Assert.False(result);

            result = Helper.ClientVersionValidation.IsValidVersion("v0.3.0", minVersion);
            Assert.False(result);

            result = Helper.ClientVersionValidation.IsValidVersion("", minVersion);
            Assert.False(result);

            result = Helper.ClientVersionValidation.IsValidVersion("aaa", minVersion);
            Assert.False(result);

            result = Helper.ClientVersionValidation.IsValidVersion("0.3.1", minVersion);
            Assert.False(result);

            result = Helper.ClientVersionValidation.IsValidVersion("v0x3x1", minVersion);
            Assert.False(result);

            result = Helper.ClientVersionValidation.IsValidVersion("v0.20.0", minVersion);
            Assert.True(result);

        }

        [Fact]
        public async Task ImageResize()
        {
            var imageurl = "https://images.sympla.com.br/62b34c1818c0f.png";

            var imageBytes = await imageurl.GetBytesAsync();
            var result = ImageHelper.ResizeImage(imageBytes, 50);
            Assert.Equal(typeof(byte[]), result.GetType());
        }
    }
}
