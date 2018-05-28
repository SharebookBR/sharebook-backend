namespace ShareBook.Service.Email
{
    public interface IEmailService
    {
        void Send(string emailRecipient, string nameRecipient, string messageText, string subject);
    }
}
