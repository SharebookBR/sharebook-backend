using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Service.Upload
{
    public interface IUploadService
    {
        Task<string> UploadImageAsync(byte[] imageBytes, string imageName, string lastDirectory);
        Task<string> UploadPdfAsync(byte[] imageBytes, string imageName, string lastDirectory);
        Task DeleteFileIfExistsAsync(string fileName, string lastDirectory);
        Task DeleteReplacedImageAsync(string oldFileName, string newFileName, string lastDirectory);
        string GetImageUrl(string imageName, string lastDirectory);
        string GetImageUrl(string imageName, string lastDirectory, int imageVersion);
        string GetBookThumbnailUrl(string imageName);
        string GetBookThumbnailUrl(string imageName, int imageVersion);
        Task<BookThumbnailBackfillResult> BackfillBookThumbnailsAsync(
            bool overwrite = false,
            int offset = 0,
            int batchSize = 50,
            CancellationToken cancellationToken = default);
    }
}
