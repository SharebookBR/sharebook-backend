using FluentValidation.Results;
using ShareBook.Domain;
using ShareBook.Domain.Validators;
using System;
using System.Text;
using Xunit;

namespace ShareBook.Test.Unit.Validators
{
    public class BookValidatorTests
    {
        BookValidator bookValidator = new BookValidator();

        [Fact]
        public void ValidEntities()
        {
            Book book = new Book()
            {
               Title = "Lord of the Rings",
               Author = "J. R. R. Tolkien",
               Image = "lotr.png",
               ImageBytes = Encoding.UTF8.GetBytes("STRINGBASE64"),
               UserId = new Guid("5489A967-9320-4350-E6FC-08D5CC8498F3"),
            };

            ValidationResult result = bookValidator.Validate(book);

            Assert.True(result.IsValid);
        }


        [Fact]
        public void InvalidEntities()
        {
            Book book = new Book()
            {
                Title = "Lord of the Rings",
                Author = null,
                Image = "lotr.png"
            };

            ValidationResult result = bookValidator.Validate(book);

            Assert.False(result.IsValid);
        }


        [Fact]
        public void InvalidImageExtension()
        {
            Book book = new Book()
            {
                Title = "Lord of the Rings",
                Author = "J. R. R. Tolkien",
                Image = "lotrnoextension"
            };

            ValidationResult result = bookValidator.Validate(book);

            Assert.False(result.IsValid);
        }
    }
}
