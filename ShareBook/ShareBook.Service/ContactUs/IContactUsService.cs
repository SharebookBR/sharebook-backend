using ShareBook.Domain;
using ShareBook.Domain.Common;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IContactUsService
    {
        Task<Result<ContactUs>> SendContactUsAsync(ContactUs contactUs, string recaptchaReactive);
    }
}
