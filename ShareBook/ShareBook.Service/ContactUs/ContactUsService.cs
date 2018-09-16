using ShareBook.Domain;

namespace ShareBook.Service
{
    public class ContactUsService : IContactUsService
    {
        IContactUsEmailService _contactUsEmailService;
        public ContactUsService(IContactUsEmailService contactUsEmailService)
        {
            _contactUsEmailService = contactUsEmailService;
        }
        public void SendContactUs(ContactUs contactUs) 
            => _contactUsEmailService.SendEmailContactUs(contactUs);
    }
}
