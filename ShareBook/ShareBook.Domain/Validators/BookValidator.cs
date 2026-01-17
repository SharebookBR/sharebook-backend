using FluentValidation;
using ShareBook.Domain.Enums;

namespace ShareBook.Domain.Validators
{
    public class BookValidator : AbstractValidator<Book>
    {
        #region Messages
        public const string Title = "Titulo do livro é obrigatório";
        public const string Author = "Autor do livro é obrigatório";
        public const string Image = "Imagem do livro é obrigatória";
        public const string Categoria = "Categoria do livro é obrigatória";
        public const string User = "O usuário deve ter vinculo com o livro";
        public const string FreightOption = "A opção de frete é obrigatória para livros físicos";
        public const string HasNotImageExtension = "A extensão da imagem não é válida. É preciso ser png, jpg ou jpeg.";
        public const string EBookPdfRequired = "É necessário enviar o arquivo PDF para cadastrar um E-Book";
        #endregion

        public BookValidator()
        {
            RuleFor(b => b.Title)
                .NotEmpty()
                .WithMessage(Title);

            RuleFor(b => b.Author)
               .NotEmpty()
               .WithMessage(Author);

            RuleFor(b => b.ImageName)
               .NotEmpty()
               .WithMessage(Image)
               .Must(HasImageExtension)
               .WithMessage(HasNotImageExtension);

            RuleFor(b => b.ImageBytes)
                .NotEmpty()
                .WithMessage(Image);

            RuleFor(b => b.FreightOption)
                .NotNull()
                .When(b => b.Type == BookType.Printed)
                .WithMessage(FreightOption);

            RuleFor(b => b.UserId)
                .NotEmpty()
                .WithMessage(User);

            RuleFor(b => b.CategoryId)
               .NotEmpty()
               .WithMessage(Categoria);

            RuleFor(b => b.PdfBytes)
                .NotEmpty()
                .When(b => b.Type == BookType.Eletronic && string.IsNullOrEmpty(b.EBookPdfPath))
                .WithMessage(EBookPdfRequired);
        }


        private bool HasImageExtension(string image)
        {
            return (!string.IsNullOrEmpty(image) &&
                       (image.ToLower().EndsWith(".png")
                       || image.ToLower().EndsWith(".jpg")
                       || image.ToLower().EndsWith(".jpeg"))
                   );
        }



    }
}
