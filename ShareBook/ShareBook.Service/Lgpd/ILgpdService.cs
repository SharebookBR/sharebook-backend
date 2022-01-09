using ShareBook.Domain.DTOs;

namespace ShareBook.Service.Lgpd
{
    public interface ILgpdService
    {
        public void Anonymize(UserAnonymizeDTO dto);
    }
}
