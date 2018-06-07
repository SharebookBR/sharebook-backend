using FluentValidation;

namespace ShareBook.Domain.Validators
{
    public class BookValidator : AbstractValidator<Book>
    {
        #region Messages
        public const string Title = "Titulo do livro é obrigatório";
        public const string Author = "Autor do livro é obrigatório";
        public const string Image = "Imagem do livro é obrigatório";
        #endregion

        public BookValidator()
        {
            RuleFor(u => u.Title)
                .NotEmpty()
                .WithMessage(Title);

            RuleFor(u => u.Author)
               .NotEmpty()
               .WithMessage(Author);

            RuleFor(u => u.Image)
               .NotEmpty()
               .WithMessage(Image);
        }
    }
}
