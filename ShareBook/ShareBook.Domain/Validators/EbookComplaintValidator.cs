using FluentValidation;
using ShareBook.Domain.Enums;

namespace ShareBook.Domain.Validators
{
    public class EbookComplaintValidator : AbstractValidator<EbookComplaint>
    {
        public EbookComplaintValidator()
        {
            RuleFor(r => r.UserId)
                .NotEmpty()
                .WithMessage("Obrigatório vincular um usuário.");

            RuleFor(r => r.BookId)
                .NotEmpty()
                .WithMessage("Obrigatório vincular um ebook.");

            RuleFor(r => r.Book.Type)
                .IsInEnum()
                .Equal(BookType.Printed)
                .WithMessage("Não é possível denunciar um livro impresso.");
        }
    }
}
