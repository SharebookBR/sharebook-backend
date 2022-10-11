using FluentValidation;
using ShareBook.Domain;
using ShareBook.Domain.Common;

namespace ShareBook.Service
{
    public class ContactUsService : IContactUsService
    {
        IContactUsEmailService _contactUsEmailService;
        IValidator<ContactUs> _validator;
        public ContactUsService(IContactUsEmailService contactUsEmailService, IValidator<ContactUs> validator)
        {
            _contactUsEmailService = contactUsEmailService;
            _validator = validator;
        }
        public Result<ContactUs> SendContactUs(ContactUs entity)
        {

            var result = new Result<ContactUs>(_validator.Validate(entity));
            // TODO: Fazer a validação real do "RecaptchaReactive" (https://developers.google.com/recaptcha/docs/verify)

            if (!result.Success)
                return result;

            _contactUsEmailService.SendEmailContactUs(entity).Wait();

            return result;
        }
    }
}
