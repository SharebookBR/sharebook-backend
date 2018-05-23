using FluentValidation.Results;
using ShareBook.Domain;
using ShareBook.Domain.Validators;
using Xunit;

namespace ShareBook.Test.Unit.Validators
{
    public class UserValidatorTests
    {
        UserValidator userValidation = new UserValidator();

        [Theory]
        [InlineData("lionel.messi@gmail.com", "barcelona")]
        public void ValidEntities(string email, string password)
        {
            User user = new User()
            {
                Email = email,
                Password = password
            };

            ValidationResult result = userValidation.Validate(user);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("emailinvalido", "")]
        public void InvalidEntities(string email, string password)
        {
            User user = new User()
            {
                Email = email,
                Password = password
            };

            ValidationResult result = userValidation.Validate(user);

            Assert.False(result.IsValid);
        }
    }
}
