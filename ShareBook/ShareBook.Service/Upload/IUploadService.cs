using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Service.Upload
{
    public interface IUploadService
    {
        Task<string> UploadImageAsync(byte[] imageBytes, string imageName, string lastDirectory);
        Task<string> UploadPdfAsync(byte[] imageBytes, string imageName, string lastDirectory);
        Task DeleteFileIfExistsAsync(string fileName, string lastDirectory);
        string GetImageUrl(string imageName, string lastDirectory);
        string GetBookThumbnailUrl(string imageName);
        Task<BookThumbnailBackfillResult> BackfillBookThumbnailsAsync(
            bool overwrite = false,
            int offset = 0,
            int batchSize = 50,
            CancellationToken cancellationToken = default);
    }
}
