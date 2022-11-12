using ShareBook.Domain.Common;

namespace ShareBook.Service.Recaptcha
{
    public interface IRecaptchaService
    {
        Result SimpleValidationRecaptcha(string recaptcha);
    }
}
