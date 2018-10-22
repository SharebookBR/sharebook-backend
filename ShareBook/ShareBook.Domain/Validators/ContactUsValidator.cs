using FluentValidation;

namespace ShareBook.Domain.Validators
{
    public class ContactUsValidator : AbstractValidator<ContactUs>
    {
        #region Messages
        public const string Name = "Nome é obrigatório";
        public const string Email = "Email é obrigatório";
        public const string EmailFormat = "O formato do email está inválido";
        public const string Phone = "Telefone é obrigatório";
        public const string Message = "Mensagem é obrigatória";
        #endregion

        public ContactUsValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty()
                .WithMessage(Name);

            RuleFor(c => c.Email)
               .EmailAddress()
               .WithMessage(EmailFormat)
               .NotEmpty()
               .WithMessage(Email);

            RuleFor(c => c.Phone)
               .NotEmpty()
               .WithMessage(Phone);

            RuleFor(c => c.Message)
                .NotEmpty()
                .WithMessage(Message);
        }
    }
}
