namespace ShareBook.Service
{
    public interface IEmailService
    {
        void Send(string emailRecipient, string nameRecipient, string messageText, string subject, bool copyAdmins);
    }
}
