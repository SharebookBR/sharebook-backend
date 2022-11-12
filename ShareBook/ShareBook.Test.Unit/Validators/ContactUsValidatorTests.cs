using FluentValidation.Results;
using ShareBook.Domain;
using ShareBook.Domain.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ShareBook.Test.Unit.Validators
{
    public class ContactUsValidatorTests
    {
        ContactUsValidator contactUsValidator = new ContactUsValidator();

        [Fact]
        public void ValidEntities()
        {
            ContactUs contactUs = new ContactUs
            {
                Email = "joao@sharebook.com.br",
                Message = "Essa mensagem é obrigatória",
                Name = "Joao",
                Phone = "4499988-7766",
            };
            ValidationResult result = contactUsValidator.Validate(contactUs);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void InvalidEmail()
        {
            ContactUs contactUs = new ContactUs
            {
                Email = "joaoarebook.com.br",
                Message = "Essa mensagem é obrigatória",
                Name = "Joao",
                Phone = "4499988-7766",
            };

            ValidationResult result = contactUsValidator.Validate(contactUs);
            Assert.False(result.IsValid);
        }
    }
}
