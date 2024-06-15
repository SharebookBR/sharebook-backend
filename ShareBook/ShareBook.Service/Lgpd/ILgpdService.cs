using ShareBook.Domain.DTOs;
using System.Threading.Tasks;

namespace ShareBook.Service.Lgpd
{
    public interface ILgpdService
    {
        public Task AnonymizeAsync(UserAnonymizeDTO dto);
    }
}
