using FluentValidation;

namespace ShareBook.Domain.Validators
{
    public class BookValidator : AbstractValidator<Book>
    {
        #region Messages
        public const string Title = "Titulo do livro é obrigatório";
        public const string Author = "Autor do livro é obrigatório";
        public const string Image = "Imagem do livro é obrigatória";
        public const string User = "O usuário deve ter vinculo com o livro";
        public const string HasNotImageExtension = "A extensão da imagem é obrigatória";
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
               .WithMessage(Image)
               .Must(HasImageExtension)
               .WithMessage(HasNotImageExtension);

            RuleFor(u => u.UserId)
                .NotEmpty()
                .WithMessage(User);
        }


        private bool HasImageExtension(string image)
        {
            return (image.EndsWith(".png") || image.EndsWith(".jpg"));
        }
    }
}
