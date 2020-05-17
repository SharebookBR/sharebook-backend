using ShareBook.Service.AWSSQS.Dto;
using System;
using System.Threading.Tasks;

namespace ShareBook.Service.AWSSQS
{
    public interface IAWSSQSService
    {
        Task SendNewBookNotifyToAWSSQSAsync(AWSSQSMessageNewBookNotify message);
    }
}