using ShareBook.Domain.Common;
using System.Threading.Tasks;

namespace ShareBook.Service.Upload
{
    public interface IUploadService<T>
    {
        string Upload(byte[] bytes, string name);

        string GetUrl(string name);
    }
}
