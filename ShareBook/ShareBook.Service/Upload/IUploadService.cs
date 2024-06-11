using System.Threading.Tasks;

namespace ShareBook.Service.Upload
{
    public interface IUploadService
    {
        Task<string> UploadImageAsync(byte[] imageBytes, string imageName, string lastDirectory);
        Task<string> UploadPdfAsync(byte[] imageBytes, string imageName, string lastDirectory);
        string GetImageUrl(string imageName, string lastDirectory);
    }
}
