using FluentValidation;

namespace ShareBook.Domain.Validators
{
    public class UserValidator : AbstractValidator<User>
    {
        #region Messages
        public const string Email = "O email é obrigatório";
        public const string Password = "A senha é obrigatória";
        public const string Name = "O nome é obrigatório";
        public const string Cep = "O cep é obrigatório";
        #endregion

        public UserValidator()
        {
            RuleFor(u => u.Email)
               .EmailAddress()
               .NotEmpty()
               .WithMessage(Email);

            RuleFor(u => u.Name)
                .NotEmpty()
                .WithMessage(Name);

            RuleFor(u => u.Password)
              .NotEmpty()
              .WithMessage(Password);

            RuleFor(u => u.Cep)
                .NotEmpty()
                .WithMessage(Cep);
        }
    }
}
