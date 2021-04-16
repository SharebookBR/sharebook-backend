using FluentValidation;

namespace ShareBook.Domain.Validators
{
    public class AddressValidator : AbstractValidator<Address>
    {
        public AddressValidator()
        {
            RuleFor(x => x.City)
                 .NotEmpty()
                 .WithMessage("Cidade é obrigatorio")
                 .MaximumLength(50)
                 .WithMessage("Cidade deve ter no máximo 50 caracteres");

            RuleFor(x => x.Country)
                .NotEmpty()
                .WithMessage("País é obrigatorio")
                .MaximumLength(50)
                .WithMessage("País deve ter no máximo 50 caracteres");

            RuleFor(x => x.Neighborhood)
                .NotEmpty()
                .WithMessage("Bairro é obrigatorio")
                .MaximumLength(50)
                .WithMessage("Bairro deve ter no máximo 50 caracteres");

            RuleFor(x => x.Number)
                .NotEmpty()
                .WithMessage("Número é obrigatorio")
                .MaximumLength(10)
                .WithMessage("Número deve ter no máximo 10 caracteres");

            RuleFor(x => x.PostalCode)
                .NotEmpty()
                .WithMessage("CEP é obrigatorio")
                .MaximumLength(15)
                .WithMessage("CEP deve ter no máximo 15 caracteres");

            RuleFor(x => x.State)
                .NotEmpty()
                .WithMessage("Estado é obrigatorio")
                .MaximumLength(30)
                .WithMessage("Estado deve ter no máximo 30 caracteres");

            RuleFor(x => x.Street)
                .NotEmpty()
                .WithMessage("Rua é obrigatorio")
                .MaximumLength(80)
                .WithMessage("Rua deve ter no máximo 80 caracteres");

            RuleFor(x => x.Complement)
                .Must(x => OptionalFieldIsValid(x, 0 , 50))
                .WithMessage("Complemento deve ter no máximo 50 caracteres");
        }

        private bool OptionalFieldIsValid(string value, int minimum, int maximum)
        {
            if (value == null)
                return true;

            return value.Length > minimum && value.Length < maximum;
        }
    }
}
