using ShareBook.Domain;
using ShareBook.Service.AWSSQS.Dto;
using System;
using System.Threading.Tasks;

namespace ShareBook.Service.AWSSQS
{
    public interface IAWSSQSService
    {
        Task SendNewBookNotifyToAWSSQSAsync(SendEmailRequest message);
        Task<AWSSQSMessageNewBookNotifyResponse> GetNewBookNotifyFromAWSSQSAsync();
        Task DeleteNewBookNotifyFromAWSSQSAsync(string receiptHandle);

        Task NotifyBookApproved(Book book);
        
    }
}