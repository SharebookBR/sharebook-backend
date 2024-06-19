using ShareBook.Service.AwsSqs.Dto;
using System.Threading.Tasks;

namespace ShareBook.Service.AwsSqs
{
    public interface IAwsSqsQueue<T>
    {
        Task SendMessageAsync(T message);

        Task<SharebookMessage<T>> GetMessageAsync();

        Task DeleteMessageAsync(string receiptHandle);
    }
}