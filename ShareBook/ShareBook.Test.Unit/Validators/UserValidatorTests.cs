using FluentValidation.Results;
using ShareBook.Data;
using ShareBook.Data.Entities.User;
using Xunit;

namespace ShareBook.Test.Unit.Validators
{
    public class UserValidatorTests
    {
        private readonly UserValidation userValidation = new UserValidation();

        [Theory]
        [InlineData("emailinvalido", "")]
        public void InvalidEntities(string email, string password)
        {
            var user = new User
            {
                Email = email,
                Password = password
            };

            ValidationResult result = userValidation.Validate(user);

            Assert.False(result.IsValid);
        }

        [Theory]
        [InlineData("lionel.messi@gmail.com", "barcelona")]
        public void ValidEntities(string email, string password)
        {
            var user = new User
            {
                Email = email,
                Password = password
            };

            ValidationResult result = userValidation.Validate(user);

            Assert.True(result.IsValid);
        }
    }
}