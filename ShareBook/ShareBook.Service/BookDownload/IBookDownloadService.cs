using ShareBook.Domain;
using System;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IBookDownloadService
    {
        Task RegisterDownloadAsync(Guid bookId, Guid? userId, string userAgent, string ipAddress);
        Task<int> GetDownloadCountAsync(Guid bookId);
    }
}