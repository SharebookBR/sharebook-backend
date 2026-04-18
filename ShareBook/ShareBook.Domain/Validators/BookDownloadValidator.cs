using FluentValidation;
using ShareBook.Domain;

namespace ShareBook.Domain.Validators
{
    public class BookDownloadValidator : AbstractValidator<BookDownload>
    {
        public BookDownloadValidator()
        {
            RuleFor(bd => bd.BookId).NotEmpty();
            RuleFor(bd => bd.DownloadedAt).NotEmpty();
            RuleFor(bd => bd.UserAgent).MaximumLength(500);
            RuleFor(bd => bd.IpAddress).MaximumLength(45);
        }
    }
}