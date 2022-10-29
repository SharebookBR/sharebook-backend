using FluentValidation;

namespace ShareBook.Domain.Validators
{
    public class ContactUsValidator : AbstractValidator<ContactUs>
    {
        #region Messages
        public const string NAME_REQUIRED = "Nome é obrigatório";
        public const string EMAIL_REQUIRED = "Email é obrigatório";
        public const string EMAIL_FORMAT = "O formato do email está inválido";
        public const string PHONE_REQUIRED = "Telefone é obrigatório";
        public const string MESSAGE_REQUIRED = "Mensagem é obrigatória";
        #endregion

        public ContactUsValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty()
                .WithMessage(NAME_REQUIRED);

            RuleFor(c => c.Email)
               .EmailAddress()
               .WithMessage(EMAIL_FORMAT)
               .NotEmpty()
               .WithMessage(EMAIL_REQUIRED);

            RuleFor(c => c.Phone)
               .NotEmpty()
               .WithMessage(PHONE_REQUIRED);

            RuleFor(c => c.Message)
                .NotEmpty()
                .WithMessage(MESSAGE_REQUIRED);
        }
    }
}
