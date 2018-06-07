using FluentValidation.Results;
using ShareBook.Domain;
using ShareBook.Domain.Validators;
using System;
using System.Collections.Generic;

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
               Image = "lotr.png"
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
