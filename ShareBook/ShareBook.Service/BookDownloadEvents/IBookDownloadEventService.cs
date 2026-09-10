using ShareBook.Domain.DTOs;
using ShareBook.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Service.BookDownloadEvents
{
    public interface IBookDownloadEventService
    {
        Task RecordAsync(Guid bookId, Guid? userId, BookDownloadEventSource source);

        Task<List<HomeShowcaseBookDTO>> GetTopDownloadedEbooksAsync(int days, int limit);
    }
}
