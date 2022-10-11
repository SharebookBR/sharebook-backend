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
        public const string RECAPTCHA_REQUIRED = "RecaptchaReactive é obrigatório";
        public const string RECAPTCHA_FORMAT = "RecaptchaReactive está inválido";
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

            RuleFor(c => c.RecaptchaReactive)
                .NotNull()
                .WithMessage(RECAPTCHA_REQUIRED)
                .NotEmpty()
                .WithMessage(RECAPTCHA_REQUIRED)
                .MinimumLength(100)
                .WithMessage(RECAPTCHA_FORMAT);
        }
    }
}
