using ShareBook.Domain;
using ShareBook.Service.AwsSqs.Dto;
using System;
using System.Threading.Tasks;

namespace ShareBook.Service.AwsSqs
{
    public interface IAwsSqsQueue<T>
    {
        Task SendMessage(T message);

        Task<T> GetMessage();

        Task DeleteMessage(string receiptHandle);
    }
}