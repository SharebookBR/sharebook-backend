using FluentValidation;
using ShareBook.Domain.Enums;

namespace ShareBook.Domain.Validators
{
    public class BookUserValidator : AbstractValidator<BookUser>
    {
        #region Messages
        public const string Book = "Livro é obrigatório";
        public const string Requester = "Solicitante do livro é obrigatório";
        public const string RequesterReason = "Justificativa do solicitante é obrigatória";
        #endregion

        public BookUserValidator()
        {
            RuleFor(b => b.BookId)
                .NotEmpty()
                .WithMessage(Book);

            RuleFor(b => b.UserId)
                .NotEmpty()
                .WithMessage(Requester);

            RuleFor(b => b.Reason)
                .NotEmpty()
                .WithMessage(RequesterReason);

        }
    }
}
