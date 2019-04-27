using ShareBook.Domain.Common;
using System.Threading.Tasks;

namespace ShareBook.Service.Upload
{
    public interface IUploadService<T>
    {
        string UploadImage(byte[] imageBytes, string imageName, string lastDirectory);
        string GetImageUrl(string imageName, string lastDirectory);
    }
}
