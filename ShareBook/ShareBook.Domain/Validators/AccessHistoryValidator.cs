using FluentValidation;

namespace ShareBook.Domain.Validators {
    public class AccessHistoryValidator : AbstractValidator<AccessHistory> {
        private const string nameRequired = "O nome é obrigatório";
        private const string nameMaxLength = "Nome deve ter no máximo 100 caracteres";

        public AccessHistoryValidator() {
            RuleFor(a => a.VisitorName)
                .NotEmpty()
                .WithMessage(nameRequired)
                .Must(x => x != null && x.Length < 200)
                .WithMessage(nameMaxLength);
        }
    }
}