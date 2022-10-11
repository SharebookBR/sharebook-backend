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
                RecaptchaReactive = "asdaasdjasodaj7i364yubki23y728374234b2jk34h2347i26348724yh2bjhk34g2j34t273842384iuh2h4j234g2j34t27834bjh",
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
                RecaptchaReactive = "asdaasdjasodaj7i364yubki23y728374234b2jk34h2347i26348724yh2bjhk34g2j34t273842384iuh2h4j234g2j34t27834bjh",
            };

            ValidationResult result = contactUsValidator.Validate(contactUs);
            Assert.False(result.IsValid);
        }

        [Fact]
        public void InvalidRecaptcha()
        {
            ContactUs contactUs = new ContactUs
            {
                Email = "joao@sharebook.com.br",
                Message = "Essa mensagem é obrigatória",
                Name = "Joao",
                Phone = "4499988-7766",
                RecaptchaReactive = "",
            };

            ValidationResult result = contactUsValidator.Validate(contactUs);
            Assert.False(result.IsValid);
        }
    }
}
