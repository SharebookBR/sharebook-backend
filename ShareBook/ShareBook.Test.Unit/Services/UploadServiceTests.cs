using Microsoft.Extensions.Options;
using ShareBook.Service.Server;
using ShareBook.Service.Upload;
using Xunit;

namespace ShareBook.Test.Unit.Services
{
    public class UploadServiceTests
    {
        private readonly UploadService _uploadService;
        public UploadServiceTests()
        {
            var image = Options.Create(new ImageSettings()
            {
                ImagePath = "wwwroot/images/books"
            });
           
            var server = Options.Create(new ServerSettings()
            {
                DefaultUrl = "http://dev.sharebook.com.br"
            });

            _uploadService = new UploadService(image, server);
        }
        [Fact]
        public void ImageUrlValid()
        {
            var expected = @"http://dev.sharebook.com.br/images/books/image.jpg";
            var actual = _uploadService.GetImageUrl("image.jpg");

            Assert.Equal(expected, actual);
        }
    }
}
