using FluentValidation;

namespace ShareBook.Domain.Validators
{
    public class UserValidator : AbstractValidator<User>
    {
        #region Messages
        public const string Email = "O email é obrigatório";
        public const string Password = "A senha é obrigatória";
        #endregion

        public UserValidator()
        {
            RuleFor(u => u.Email)
               .EmailAddress()
               .NotEmpty()
               .WithMessage(Email);

            RuleFor(u => u.Password)
              .NotEmpty()
              .WithMessage(Password);
        }
    }
}
