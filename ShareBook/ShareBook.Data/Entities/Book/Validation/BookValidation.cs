using FluentValidation;
using ShareBook.Data.Entities.Book;

namespace ShareBook.Data
{
    public class BookValidation : AbstractValidator<Book>
    {
        public BookValidation()
        {
            RuleFor(u => u.Name)
                .NotEmpty().WithMessage(BookMessage.Validation.Name);
        }
    }
}
