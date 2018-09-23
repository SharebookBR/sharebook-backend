using FluentValidation;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service.Generic;

namespace ShareBook.Service
{
    public class ContactUsService : BaseService<ContactUs>, IContactUsService
    {
        IContactUsEmailService _contactUsEmailService;
        public ContactUsService(IContactUsEmailService contactUsEmailService, IValidator<ContactUs> validator)
            : base(validator)
        {
            _contactUsEmailService = contactUsEmailService;
        }
        public Result<ContactUs> SendContactUs(ContactUs entity)
        {

            Result<ContactUs> result = Validate(entity, x =>
               x.Name,
               x => x.Phone,
               x => x.Email,
               x => x.Message);

            if (!result.Success)
                return result;

            _contactUsEmailService.SendEmailContactUs(entity);

            return result;
        }
    }
}
