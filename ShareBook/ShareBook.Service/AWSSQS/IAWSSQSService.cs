using ShareBook.Service.AWSSQS.Dto;
using System.Threading.Tasks;

namespace ShareBook.Service.AWSSQS
{
    public interface IAWSSQSService
    {
        Task SendNewBookNotifyToAWSSQSAsync(AWSSQSMessageNewBookNotifyRequest message);
        Task<AWSSQSMessageNewBookNotifyResponse> GetNewBookNotifyFromAWSSQSAsync();
        Task DeleteNewBookNotifyFromAWSSQSAsync(string receiptHandle);
    }
}