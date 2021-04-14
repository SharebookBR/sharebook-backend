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
                .Must(x => x != null && x.Length > 3 && x.Length < 100)
                .WithMessage("Nome deve ter entre 3 e 100 caracteres");

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
                .Must(x => x != null && x.Length < 100)
                .WithMessage("Linkedln deve ter no máximo 100 caracteres");

            RuleFor(u => u.Phone)
                .Must(x => x != null && x.Length < 100)
                .WithMessage("Telefone deve ter no máximo 100 caracteres");

            RuleFor(x => x.Address.City)
                .NotEmpty()
                .WithMessage("Cidade é obrigatorio")
                .MaximumLength(50)
                .WithMessage("Cidade deve ter no máximo 50 caracteres");

            RuleFor(x => x.Address.Country)
                .NotEmpty()
                .WithMessage("País é obrigatorio")
                .MaximumLength(50)
                .WithMessage("País deve ter no máximo 50 caracteres");

            RuleFor(x => x.Address.Neighborhood)
                .NotEmpty()
                .WithMessage("Bairro é obrigatorio")
                .MaximumLength(50)
                .WithMessage("Bairro deve ter no máximo 50 caracteres");

            RuleFor(x => x.Address.Number)
                .NotEmpty()
                .WithMessage("Número é obrigatorio")
                .MaximumLength(10)
                .WithMessage("Número deve ter no máximo 10 caracteres");

            RuleFor(x => x.Address.PostalCode)
                .NotEmpty()
                .WithMessage("CEP é obrigatorio")
                .MaximumLength(15)
                .WithMessage("CEP deve ter no máximo 15 caracteres");

            RuleFor(x => x.Address.State)
                .NotEmpty()
                .WithMessage("Estado é obrigatorio")
                .MaximumLength(30)
                .WithMessage("Estado deve ter no máximo 30 caracteres");

            RuleFor(x => x.Address.Street)
                .NotEmpty()
                .WithMessage("Rua é obrigatorio")
                .MaximumLength(80)
                .WithMessage("Rua deve ter no máximo 80 caracteres");

            RuleFor(x => x.Address.Complement)
                .Must(x => x != null && x.Length < 50)
                .WithMessage("Complemento deve ter no máximo 50 caracteres");
        }

        private bool PostalCodeIsValid(string postalCode)
        {
            Regex Rgx = new Regex(@"^\d{5}-\d{3}$");
            if (string.IsNullOrEmpty(postalCode) || !Rgx.IsMatch(postalCode)) return false;

            return true;
        }
    }
}
