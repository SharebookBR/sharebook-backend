using ShareBook.Domain;
using ShareBook.Domain.Common;

namespace ShareBook.Service
{
    public interface IContactUsService
    {
        Result<ContactUs> SendContactUs(ContactUs contactUs);
    }
}
