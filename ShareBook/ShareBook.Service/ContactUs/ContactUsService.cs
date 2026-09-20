using FluentValidation;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service.Recaptcha;
using System.Threading.Tasks;

namespace ShareBook.Service;

public class ContactUsService(IContactUsEmailService contactUsEmailService, IValidator<ContactUs> validator, IRecaptchaService recaptchaService) : IContactUsService
{
    readonly IContactUsEmailService _contactUsEmailService = contactUsEmailService;
    readonly IValidator<ContactUs> _validator = validator;
    readonly IRecaptchaService _recaptchaService = recaptchaService;

    public async Task<Result<ContactUs>> SendContactUsAsync(ContactUs contactUs, string recaptchaReactive)
    {

        var result = new Result<ContactUs>(_validator.Validate(contactUs));

        Result resultRecaptcha = _recaptchaService.SimpleValidationRecaptcha(recaptchaReactive);
        if (!resultRecaptcha.Success)
            result.Messages.AddRange(resultRecaptcha.Messages);

        if (!result.Success)
            return result;

        await _contactUsEmailService.SendEmailContactUsAsync(contactUs);

        return result;
    }
}
