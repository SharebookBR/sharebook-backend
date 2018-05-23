using FluentValidation;

namespace ShareBook.Domain.Validators
{
    public class BookValidator : AbstractValidator<Book>
    {
        #region Messages
        public const string Name = "Nome do livro é obrigatório";
        #endregion

        public BookValidator()
        {
            RuleFor(u => u.Name)
                .NotEmpty()
                .WithMessage(Name);
        }
    }
}
