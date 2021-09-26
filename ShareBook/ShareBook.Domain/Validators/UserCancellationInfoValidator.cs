using FluentValidation;

namespace ShareBook.Domain.Validators {
    public class UserCancellationInfoValidator : AbstractValidator<UserCancellationInfo> {
        private const string ReasonRequired = "O campo Motivo deve ser preenchido e muito importante para nós!";
        private const string ReasonMaxLength = "O campo Motivo poderá ter no máximo 250 caractres";
        public UserCancellationInfoValidator() {
            string reasonMaxLeng;
            RuleFor(u => u.Reason)
                .NotEmpty()
                .WithMessage(ReasonRequired)
                .Must(r => r is not null && r.Length <= 250)
                .WithMessage(ReasonMaxLength);
        }
    }
}