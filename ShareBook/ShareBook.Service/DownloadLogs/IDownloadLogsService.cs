using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Service.DownloadLogs;

public interface IDownloadLogsService
{
    Task<IList<DownloadLogsSummaryDto>> GetSummaryAsync(DateTime from, DateTime to);
    Task<PagedDownloadLogEventsDto> GetEventsAsync(DateTime from, DateTime to, int page, int pageSize);
}
