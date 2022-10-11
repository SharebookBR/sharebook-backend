using FluentValidation;
using System.Text.RegularExpressions;

namespace ShareBook.Domain.Validators
{
    public class UserValidator : AbstractValidator<User>
    {
        #region Messages
        public const string Email = "O email é obrigatório";
        public const string EmailFormat = "O formato do email está inválido";
        public const string Password = "A senha é obrigatória.";
        public const string Name = "O nome é obrigatório";
        public const string PostalCode = "O cep é obrigatório";
        public const string PostalCodeInvalid = "O formato do cep está inválido";
        public const string Linkedin = "O seu endereço do linkedin é obrigatório";
        #endregion

        public UserValidator()
        {
            RuleFor(u => u.Name)
                .NotEmpty()
                .WithMessage(Name)
                .Must(x => x != null && x.Length < 100)
                .WithMessage("Nome deve ter no máximo 100 caracteres");

            RuleFor(u => u.Email)
               .EmailAddress()
               .WithMessage(EmailFormat)
               .NotEmpty()
               .WithMessage(Email)
               .MaximumLength(100)
               .WithMessage("Email deve ter no máximo 100 caracteres");

            RuleFor(u => u.Password)
              .NotEmpty()
              .WithMessage(Password)
              .Must(x => x != null && x.Length >= 6 && x.Length <= 32)
              .WithMessage("Senha deve ter entre 6 e 32 letras.");

            RuleFor(u => u.Linkedin)
                .Must(x => OptionalFieldIsValid(x))
                .WithMessage("Linkedln deve ter no máximo 100 caracteres");

            RuleFor(u => u.Instagram)
                .Must(x => OptionalFieldIsValid(x))
                .WithMessage("Instagram deve ter no máximo 100 caracteres");

            RuleFor(u => u.Phone)
                .NotNull()
                .WithMessage("Telefone é obrigatório")
                .MaximumLength(100)
                .WithMessage("Telefone deve ter no máximo 100 caracteres");

            RuleFor(x => x.Address)
                .SetValidator(new AddressValidator());

        }

        private bool PostalCodeIsValid(string postalCode)
        {
            Regex Rgx = new Regex(@"^\d{5}-\d{3}$");
            if (string.IsNullOrEmpty(postalCode) || !Rgx.IsMatch(postalCode)) return false;

            return true;
        }

        private bool OptionalFieldIsValid(string value)
        {
            if (value == null || value.Length == 0 || value == string.Empty)
                return true;

            return value.Length > 0 && value.Length < 100;
        }
    }
}
