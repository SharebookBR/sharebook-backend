using FluentValidation;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service.Recaptcha;

namespace ShareBook.Service
{
    public class ContactUsService : IContactUsService
    {
        IContactUsEmailService _contactUsEmailService;
        IValidator<ContactUs> _validator;
        IRecaptchaService _recaptchaService;
        public ContactUsService(IContactUsEmailService contactUsEmailService, IValidator<ContactUs> validator, IRecaptchaService recaptchaService)
        {
            _contactUsEmailService = contactUsEmailService;
            _validator = validator;
            _recaptchaService = recaptchaService;
        }
        public Result<ContactUs> SendContactUs(ContactUs entity, string recaptchaReactive)
        {

            var result = new Result<ContactUs>(_validator.Validate(entity));

            Result resultRecaptcha = _recaptchaService.SimpleValidationRecaptcha(recaptchaReactive);
            if (!resultRecaptcha.Success)
                result.Messages.AddRange(resultRecaptcha.Messages);

            if (!result.Success)
                return result;

            _contactUsEmailService.SendEmailContactUs(entity).Wait();

            return result;
        }
    }
}
